using UnityEngine;

namespace FPS.Player
{
    /// <summary>
    /// Fallback weapon for placeholder levels: hitscan attack on LMB that
    /// damages any HealthSystem in range. Swap for WeaponController once the
    /// real weapon model/prefab is imported.
    /// </summary>
    public class PlaceholderWeapon : MonoBehaviour
    {
        [SerializeField] private float range = 60f;
        [SerializeField] private int damage = 30;
        [SerializeField] private float cooldown = 0.4f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private Transform muzzle;

        private float timer = 0f;

        private void Update()
        {
            if (timer > 0f)
                timer -= Time.deltaTime;

            if (Input.GetMouseButtonDown(0) && timer <= 0f)
            {
                timer = cooldown;
                Shoot();
            }
        }

        private void Shoot()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            Transform origin = muzzle != null ? muzzle : cam.transform;
            Vector3 dir = cam.transform.forward;

            if (Physics.Raycast(origin.position, dir, out RaycastHit hit, range, targetMask, QueryTriggerInteraction.Ignore))
            {
                var hs = hit.collider.GetComponentInParent<FPS.Health.HealthSystem>();
                if (hs != null)
                    hs.TakeDamage(damage, hit.point, transform);
            }
        }
    }
}