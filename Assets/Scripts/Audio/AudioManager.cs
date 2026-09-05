using UnityEngine;

namespace FPS.Audio
{
    /// <summary>
    /// Global audio manager: master/music/sfx volumes persisted locally and
    /// applied to every MusicPlayer/ambience in the loaded level.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private float masterVolume = 0.8f;
        [SerializeField] private float musicVolume = 0.6f;
        [SerializeField] private float sfxVolume = 0.8f;

        private const string MasterKey = "FPS_Audio_Master";
        private const string MusicKey = "FPS_Audio_Music";
        private const string SfxKey = "FPS_Audio_Sfx";

        public float MasterVolume { get { return masterVolume; } }
        public float MusicVolume { get { return musicVolume; } }
        public float SfxVolume { get { return sfxVolume; } }
        public float EffectiveMusic { get { return masterVolume * musicVolume; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            masterVolume = PlayerPrefs.GetFloat(MasterKey, 0.8f);
            musicVolume = PlayerPrefs.GetFloat(MusicKey, 0.6f);
            sfxVolume = PlayerPrefs.GetFloat(SfxKey, 0.8f);
        }

        public void SetMaster(float v)
        {
            masterVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(MasterKey, masterVolume);
            PlayerPrefs.Save();
            ApplyMusicVolumes();
        }

        public void SetMusic(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(MusicKey, musicVolume);
            PlayerPrefs.Save();
            ApplyMusicVolumes();
        }

        public void SetSfx(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(SfxKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlaySfx(AudioSource src, AudioClip clip, float volumeScale = 1f)
        {
            if (src == null || clip == null)
                return;
            src.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
        }

        private void ApplyMusicVolumes()
        {
            MusicPlayer[] all = FindObjectsByType<MusicPlayer>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null)
                    all[i].SetVolume(EffectiveMusic);
            }
        }
    }
}