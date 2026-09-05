using UnityEngine;
using FPS.Enemies;

namespace FPS.Dialogue
{
    /// <summary>
    /// Lets an enemy speak lines when it spots/chases/attacks/dies.
    /// Wired to EnemyAI state changes with lightweight polling.
    /// </summary>
    public class EnemyDialogue : MonoBehaviour
    {
        [Header("Lines (local, voice-ready)")]
        [SerializeField] private string enemyName = "Inimigo";
        [SerializeField] private DialogueData detectDialogue;
        [SerializeField] private DialogueData attackDialogue;
        [SerializeField] private DialogueData deathDialogue;

        private EnemyAI ai;
        private EnemyState lastState = EnemyState.Idle;
        private bool deadSpoken = false;
        private float cooldown = 0f;

        private void Awake()
        {
            ai = GetComponent<EnemyAI>();
        }

        private void Update()
        {
            if (cooldown > 0f)
                cooldown -= Time.deltaTime;

            if (ai == null)
                return;

            EnemyState s = ai.State;
            if (s == lastState)
                return;

            if (cooldown <= 0f)
            {
                if (s == EnemyState.Detect || s == EnemyState.Chase)
                    Play(detectDialogue, "Detection");
                else if (s == EnemyState.Attack)
                    Play(attackDialogue, "Attack");
                else if (s == EnemyState.Dead && !deadSpoken)
                {
                    deadSpoken = true;
                    Play(deathDialogue, "Death");
                }
            }

            lastState = s;
        }

        private void Play(DialogueData data, string kind)
        {
            if (data == null)
                return;

            cooldown = 4f;
            var mgr = DialogueManager.Instance;
            if (mgr != null)
            {
                mgr.Show(data);
                return;
            }
            Debug.Log("[FPS→Dialogue] " + enemyName + " (" + kind + "): " + (data.lines.Length > 0 ? data.lines[0].text : ""));
        }
    }
}