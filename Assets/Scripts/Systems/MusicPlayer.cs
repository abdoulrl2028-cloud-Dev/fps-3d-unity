using UnityEngine;

namespace FPS.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip ambientClip;
        [SerializeField] private AudioClip combatClip;
        [SerializeField] private float volume = 0.6f;

        private AudioSource source;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.volume = volume;
            source.spatialBlend = 0f;

            if (ambientClip != null)
            {
                source.clip = ambientClip;
                source.Play();
            }
        }

        public void PlayCombatMusic(AudioClip clip)
        {
            if (clip == null)
                return;
            combatClip = clip;
            source.clip = combatClip;
            source.Play();
        }

        public void RestoreAmbient()
        {
            if (ambientClip != null)
            {
                source.clip = ambientClip;
                source.Play();
            }
        }

        public void SetVolume(float v)
        {
            volume = v;
            if (source != null)
                source.volume = v;
        }
    }
}