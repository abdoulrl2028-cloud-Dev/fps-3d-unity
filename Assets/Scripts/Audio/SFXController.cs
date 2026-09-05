using UnityEngine;

namespace FPS.Audio
{
    /// <summary>
    /// One-shot SFX helper with named clip slots (gunshot, reload, attack,
    /// impact, footsteps...). Volume routed through AudioManager.
    /// </summary>
    public class SFXController : MonoBehaviour
    {
        [Header("SFX Clips")]
        [SerializeField] private AudioClip gunshotClip;
        [SerializeField] private AudioClip reloadClip;
        [SerializeField] private AudioClip attackClip;
        [SerializeField] private AudioClip impactClip;
        [SerializeField] private AudioClip footstepClip;

        private AudioSource src;

        private void Awake()
        {
            src = GetComponent<AudioSource>();
            if (src == null)
                src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
        }

        public void PlayGunshot() { PlayClip(gunshotClip); }
        public void PlayReload() { PlayClip(reloadClip); }
        public void PlayAttack() { PlayClip(attackClip); }
        public void PlayImpact() { PlayClip(impactClip); }
        public void PlayFootstep() { PlayClip(footstepClip, 0.5f); }
        public void PlayClip(AudioClip clip, float scale = 1f)
        {
            if (clip == null)
                return;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(src, clip, scale);
            else
                src.PlayOneShot(clip, scale);
        }

        public AudioSource Source { get { return src; } }
    }
}