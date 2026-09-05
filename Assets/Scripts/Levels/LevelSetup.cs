using UnityEngine;

namespace FPS.Levels
{
    /// <summary>
    /// Per-level bootstrap. Runs on level start:
    /// - ensures a Player exists at PlayerSpawn,
    /// - plays the level's unique music/ambience,
    /// - starts the intro dialogue,
    /// - configures difficulty + enemy spawners,
    /// - logs any pending manual model imports (placeholders).
    /// </summary>
    public class LevelSetup : MonoBehaviour
    {
        [Header("Level")]
        [Tooltip("1-based level number (Level01..Level09).")]
        [SerializeField] private int levelIndex = 1;

        [Header("Audio (unique per level)")]
        [SerializeField] private AudioClip levelMusicClip;
        [SerializeField] private AudioClip combatMusicClip;
        [SerializeField] private AudioClip ambientClip;

        [Header("Dialogue")]
        [SerializeField] private FPS.Dialogue.DialogueData introDialogue;

        [Header("Difficulty")]
        [SerializeField] private float difficultyScale = 1f;
        [SerializeField] private GameObject playerSpawnOverride;

        private void Start()
        {
            EnsurePlayer();
            PlayLevelAudio();
            StartLevelDialogue();
            ConfigureSpawners();
            LogPendingImports();
        }

        private void EnsurePlayer()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
                return;

            Transform spawn = FindSpawnPoint();
            GameObject prefab = FPS.Tools.PlaceholderRegistry.FindPlayer();
            if (prefab == null)
            {
                Debug.LogWarning("[FPS→Level" + levelIndex + "] No PlayerBase prefab in Resources.");
                return;
            }
            GameObject player = Instantiate(prefab, spawn != null ? spawn.position : Vector3.up,
                spawn != null ? spawn.rotation : Quaternion.identity);
            player.name = "Player";
        }

        private Transform FindSpawnPoint()
        {
            GameObject spawnGo = GameObject.Find("PlayerSpawn");
            return spawnGo != null ? spawnGo.transform : null;
        }

        private void PlayLevelAudio()
        {
            if (levelMusicClip == null && ambientClip == null)
            {
                Debug.Log("[FPS→Audio] Level" + levelIndex +
                          ": coloque seus clips únicos em Audio/Music e Audio/Ambient (manual).");
                return;
            }

            var music = FindFirstObjectByType<FPS.Audio.MusicController>();
            if (music != null && levelMusicClip != null)
                music.Play(levelMusicClip);

            var ambient = FindFirstObjectByType<FPS.Audio.AmbientSoundController>();
            if (ambient != null && ambientClip != null)
                ambient.SetClip(ambientClip);
        }

        private void StartLevelDialogue()
        {
            if (introDialogue == null)
                return;
            var dm = FPS.Dialogue.DialogueManager.Instance;
            if (dm != null)
                dm.Show(introDialogue);
        }

        private void ConfigureSpawners()
        {
            FPS.AI.EnemySpawner[] spawners = FindObjectsByType<FPS.AI.EnemySpawner>(FindObjectsSortMode.None);
            for (int i = 0; i < spawners.Length; i++)
                spawners[i].SetDifficulty(difficultyScale);
        }

        private void LogPendingImports()
        {
            FPS.Tools.ModelPlaceholder[] phs =
                FindObjectsByType<FPS.Tools.ModelPlaceholder>(FindObjectsSortMode.None);
            for (int i = 0; i < phs.Length; i++)
                phs[i].LogPending();
        }
    }
}