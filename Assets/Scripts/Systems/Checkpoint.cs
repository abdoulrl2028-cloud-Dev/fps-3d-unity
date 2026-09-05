using UnityEngine;

namespace FPS.Checkpoints
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool isFinal = false;

        private bool activated = false;

        public bool IsFinal => isFinal;

        private void Awake()
        {
            if (spawnPoint == null)
                spawnPoint = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Managers.GameManager.Instance?.ActivateCheckpoint(this);
            activated = true;
        }
    }
}