using UnityEngine;

namespace FPS.Dialogue
{
    /// <summary>
    /// Plays dialogue triggered by a trigger area (player enters) or on level
    /// start. One dialogue per trigger type.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogue;
        [SerializeField] private bool playOnLevelStart = false;
        [SerializeField] private bool playOnce = true;

        private bool played = false;

        private void Start()
        {
            if (!playOnLevelStart)
                return;
            if (!playOnce || !played)
            {
                played = true;
                Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            if (playOnce && played)
                return;
            played = true;
            Play();
        }

        public void Play()
        {
            if (dialogue == null)
                return;
            var mgr = FPS.Dialogue.DialogueManager.Instance;
            if (mgr != null)
                mgr.Show(dialogue);
            else
                Debug.LogWarning("[FPS→Dialogue] No DialogueManager in scene (" + dialogue.name + ")");
        }
    }
}