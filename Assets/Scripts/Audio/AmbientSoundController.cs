using UnityEngine;

namespace FPS.Audio
{
    /// <summary>
    /// Looping ambient sound layer (wind, machinery, traffic...). Place the
    /// component on the level's Ambient node. Optionally spatial (3D).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbientSoundController : MonoBehaviour
    {
        [SerializeField] private AudioClip ambientClip;
        [SerializeField] private bool spatial = false;
        [SerializeField] private float volume = 0.5f;
        [SerializeField] private float minRadius = 10f;
        [SerializeField] private float maxRadius = 60f;

        private AudioSource src;

        private void Awake()
        {
            src = GetComponent<AudioSource>();
            src.clip = ambientClip;
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = spatial ? 1f : 0f;
            src.volume = volume;
            if (spatial)
            {
                src.rolloffMode = AudioRolloffMode.Logarithmic;
                src.minDistance = minRadius;
                src.maxDistance = maxRadius;
            }
        }

        private void Start()
        {
            if (ambientClip != null)
                src.Play();
        }

        public void SetClip(AudioClip clip)
        {
            ambientClip = clip;
            if (src == null)
                return;
            src.clip = clip;
        }

        public AudioSource Source { get { return src; } }
    }
}