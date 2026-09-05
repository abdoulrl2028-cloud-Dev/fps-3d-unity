using UnityEngine;
using UnityEngine.AI;
using FPS.Health;

namespace FPS.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Detect,
        Chase,
        Attack,
        Dead
    }

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private EnemyState state = EnemyState.Idle;

        [Header("Senses")]
        [SerializeField] private float detectionRange = 20f;
        [SerializeField] private float fovAngle = 60f;
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float loseSightRange = 30f;
        [SerializeField] private LayerMask obstacleMask = ~0;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2.5f;
        [SerializeField] private float idleTimePerPoint = 2f;
        [SerializeField] private float waypointReachDistance = 1.2f;

        [Header("Combat")]
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float chaseSpeed = 5f;

        [Header("References")]
        [SerializeField] private Transform eyeTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;

        private NavMeshAgent agent;
        private HealthSystem health;
        private Transform playerTransform;
        private int currentPatrolIndex = 0;
        private float idleTimer = 0f;
        private float attackTimer = 0f;
        private bool knowsPlayer = false;

        public EnemyState State => state;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<HealthSystem>();
            if (eyeTransform == null)
                eyeTransform = transform;

            if (health != null)
            {
                health.OnDied += Die;
                health.OnDamaged += OnHitByPlayer;
            }

            agent.speed = patrolSpeed;
        }

        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            // Only turn on obstacle avoidance if mask allows; default handles it.
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                state = EnemyState.Patrol;
                MoveTo(patrolPoints[currentPatrolIndex].position);
            }
        }

        private void Update()
        {
            if (state == EnemyState.Dead)
                return;

            if (playerTransform == null)
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            TickSensing();
            TickState();
        }

        private void TickSensing()
        {
            if (playerTransform == null)
                return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            bool hasLineOfSight = HasLineOfSight(playerTransform);

            if (state == EnemyState.Chase || state == EnemyState.Attack)
            {
                // Losing the player
                if (distance > loseSightRange || !hasLineOfSight)
                {
                    if (distance > loseSightRange)
                    {
                        knowsPlayer = false;
                        state = EnemyState.Patrol;
                        agent.speed = patrolSpeed;
                    }
                }
                return;
            }

            bool inRange = distance <= detectionRange;
            bool inFov = InFieldOfView(playerTransform, fovAngle);

            if (inRange && (inFov || health != null && health.CurrentHealth < health.MaxHealth) && hasLineOfSight)
            {
                if (!knowsPlayer)
                {
                    knowsPlayer = true;
                    state = EnemyState.Detect;
                    agent.isStopped = true;
                }
            }
        }

        private void TickState()
        {
            switch (state)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    TickPatrol();
                    break;
                case EnemyState.Detect:
                    TickDetect();
                    break;
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Attack:
                    TickAttack();
                    break;
                case EnemyState.Dead:
                    break;
            }
        }

        private void TickPatrol()
        {
            if (!agent.isOnNavMesh || agent.pathPending)
                return;

            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                state = EnemyState.Idle;
                return;
            }

            if (agent.remainingDistance <= waypointReachDistance && !agent.pathPending)
            {
                idleTimer += Time.deltaTime;
                agent.isStopped = true;
                if (idleTimer >= idleTimePerPoint)
                {
                    idleTimer = 0f;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    agent.isStopped = false;
                    MoveTo(patrolPoints[currentPatrolIndex].position);
                }
            }
        }

        private void TickDetect()
        {
            if (playerTransform == null)
            {
                state = EnemyState.Patrol;
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            agent.isStopped = true;

            // Face the player
            Vector3 lookDir = playerTransform.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

            if (distance <= attackRange)
            {
                state = EnemyState.Attack;
                attackTimer = 0f;
                agent.isStopped = false;
            }
            else if (distance <= detectionRange && HasLineOfSight(playerTransform))
            {
                state = EnemyState.Chase;
                agent.isStopped = false;
                agent.speed = chaseSpeed;
            }
            else
            {
                knowsPlayer = false;
                state = EnemyState.Patrol;
                agent.speed = patrolSpeed;
                agent.isStopped = false;
            }
        }

        private void TickChase()
        {
            if (playerTransform == null)
            {
                state = EnemyState.Patrol;
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= attackRange)
            {
                state = EnemyState.Attack;
                agent.isStopped = true;
                attackTimer = 0f;
                return;
            }

            if (distance > loseSightRange || !HasLineOfSight(playerTransform))
            {
                knowsPlayer = false;
                state = EnemyState.Patrol;
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                MoveTo(NearestPatrolPoint());
                return;
            }

            MoveTo(playerTransform.position);
        }

        private void TickAttack()
        {
            if (playerTransform == null)
            {
                state = EnemyState.Patrol;
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance > attackRange)
            {
                state = EnemyState.Chase;
                agent.isStopped = false;
                return;
            }

            // Face player
            Vector3 lookDir = playerTransform.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                AttackPlayer();
            }
        }

        private void AttackPlayer()
        {
            // Start attack animation
            SetAnimatorTrigger("Attack");

            FPS.Health.HealthSystem playerHealth = playerTransform != null
                ? playerTransform.GetComponentInParent<FPS.Health.HealthSystem>()
                : null;

            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, playerTransform.position, transform);

            PlayAttackSound();
        }

        private void OnHitByPlayer()
        {
            knowsPlayer = true;
            if (state == EnemyState.Dead)
                return;

            if (state == EnemyState.Patrol || state == EnemyState.Idle)
            {
                state = EnemyState.Chase;
                agent.speed = chaseSpeed;
                agent.isStopped = false;
            }
        }

        private void Die()
        {
            if (state == EnemyState.Dead)
                return;

            state = EnemyState.Dead;
            agent.isStopped = true;
            agent.enabled = false;
            SetAnimatorTrigger("Death");

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                if (c.isTrigger) continue;
                c.enabled = false;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            Destroy(gameObject, 4f);
        }

        private bool InFieldOfView(Transform target, float angleDegrees)
        {
            Vector3 directionToTarget = (target.position + target.up * 1.5f) - (eyeTransform != null ? eyeTransform.position : transform.position);
            if (directionToTarget.sqrMagnitude < 0.01f)
                return true;

            float angle = Vector3.Angle(transform.forward, directionToTarget);
            return angle <= angleDegrees;
        }

        private bool HasLineOfSight(Transform target)
        {
            Vector3 origin = eyeTransform != null ? eyeTransform.position : transform.position + Vector3.up * 1.5f;
            Vector3 targetHead = target.position + Vector3.up * 1.5f;

            if (Physics.Raycast(origin, (targetHead - origin).normalized, out RaycastHit hit, Mathf.Infinity, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                Transform root = hit.collider.transform;
                if (root.root == target.root)
                    return true;

                return false;
            }
            return true;
        }

        private void MoveTo(Vector3 position)
        {
            if (!agent.isOnNavMesh)
                return;
            if (agent.isStopped)
                agent.isStopped = false;
            agent.SetDestination(position);
        }

        private Vector3 NearestPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return transform.position;

            Vector3 nearest = patrolPoints[0].position;
            float nearestDist = float.MaxValue;
            foreach (Transform p in patrolPoints)
            {
                float d = Vector3.Distance(transform.position, p.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = p.position;
                }
            }
            return nearest;
        }

        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
        }

        /// <summary>Scales senses/damage for higher difficulty (Level06+).</summary>
        public void MultiplyDifficulty(float scale)
        {
            if (scale <= 1f)
                return;
            detectionRange *= scale;
            attackRange = attackRange;
            attackDamage = Mathf.RoundToInt(attackDamage * scale);
            chaseSpeed *= scale;
            attackCooldown = Mathf.Max(0.35f, attackCooldown / scale);
        }

        public void SetAnimatorTrigger(string name)
        {
            if (animator != null)
                animator.SetTrigger(name);
        }

        private void PlayAttackSound()
        {
            if (audioSource != null && audioSource.clip != null)
                audioSource.PlayOneShot(audioSource.clip);
        }
    }
}