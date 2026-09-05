using UnityEngine;

namespace FPS.Levels
{
    /// <summary>
    /// Placed on the exit trigger of a level. When the Player enters and all
    /// objectives are done, LevelManager completes the level.
    /// </summary>
    public class ExitZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            var lm = FPS.Levels.LevelManager.Instance;
            if (lm != null)
                lm.OnExitZoneEnter();
        }
    }
}