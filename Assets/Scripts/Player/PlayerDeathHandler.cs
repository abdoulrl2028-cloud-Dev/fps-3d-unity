using UnityEngine;
using FPS.Health;
using FPS.Player;
using FPS.Managers;

namespace FPS.Player
{
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        private HealthSystem health;
        private FpsPlayerController controller;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            controller = GetComponent<FpsPlayerController>();

            if (health != null)
                health.OnDied += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (controller != null)
                controller.SetAlive(false);

            GameManager.Instance?.GameOver();
        }
    }
}