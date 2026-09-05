using UnityEngine;

namespace FPS.Audio
{
    /// <summary>
    /// Per-level music controller: optional intro + looping loop clip, volume
    /// always routed through AudioManager so the global music slider works.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        [Header("Music Clips (unique per level - place under Assets/Audio/Music)")]
        [SerializeField] private AudioClip introClip;
        [SerializeField] private AudioClip loopClip;

        private AudioSource src;

        private void Awake()
        {
            src = GetComponent<AudioSource>();
            src.loop = loopClip != null;
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            src.volume = AudioManager.Instance != null ? AudioManager.Instance.EffectiveMusic : 0.6f;
        }

        private void Start()
        {
            if (introClip != null && loopClip != null)
                PlayIntroThenLoop();
            else
                Play(loopClip != null ? loopClip : introClip);
        }

        public void Play(AudioClip clip)
        {
            if (clip == null || src == null)
                return;
            src.clip = clip;
            src.loop = true;
            src.Play();
        }

        public void PlayIntroThenLoop()
        {
            if (src == null || introClip == null || loopClip == null)
            {
                Play(loopClip != null ? loopClip : introClip);
                return;
            }
            src.clip = introClip;
            src.loop = false;
            src.Play();
            if (introClip.length > 0f)
                Invoke("StartLoopClip", introClip.length);
            else
                StartLoopClip();
        }

        private void StartLoopClip()
        {
            if (src == null || loopClip == null)
                return;
            src.clip = loopClip;
            src.loop = true;
            src.Play();
        }

        public void SetVolume(float v)
        {
            if (src != null)
                src.volume = v;
        }
    }
}