using System;
using UnityEngine;

namespace FPS.Managers
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        MissionComplete
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnStateChanged;

        [Header("Player")]
        [SerializeField] private Transform playerTransform;

        [Header("Mission")]
        [SerializeField] private Transform initialSpawnPoint;
        [SerializeField] private Transform currentCheckpoint;

        [Header("UI")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverMenu;
        [SerializeField] private GameObject missionCompleteMenu;
        [SerializeField] private GameObject hudObject;

        private GameState state = GameState.Playing;
        private bool isPaused = false;

        public GameState State => state;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (playerTransform == null)
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == GameState.Playing)
                    PauseGame();
                else if (state == GameState.Paused)
                    ResumeGame();
            }

            if (state == GameState.GameOver || state == GameState.MissionComplete)
            {
                if (Input.GetKeyDown(KeyCode.R))
                    RestartGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            SetCursorLocked(false);
            SetState(GameState.Paused);
            if (pauseMenu != null) pauseMenu.SetActive(true);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SetCursorLocked(true);
            SetState(GameState.Playing);
            if (pauseMenu != null) pauseMenu.SetActive(false);
        }

        public void RestartGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (currentCheckpoint != null)
                RespawnPlayer(currentCheckpoint.position);
            else if (initialSpawnPoint != null)
                RespawnPlayer(initialSpawnPoint.position);

            SetState(GameState.Playing);
            SetActiveMenu(GameState.Playing);

            // Restore player
            RestorePlayer();

            SetCursorLocked(true);
        }

        private void RespawnPlayer(Vector3 position)
        {
            if (playerTransform == null)
                return;

            var controller = playerTransform.GetComponent<FPS.Player.FpsPlayerController>();
            if (controller != null)
                controller.Teleport(position);
            else
                playerTransform.position = position;
        }

        private void RestorePlayer()
        {
            if (playerTransform == null)
                return;

            var health = playerTransform.GetComponent<FPS.Health.HealthSystem>();
            if (health != null)
                health.RestoreFull();

            var player = playerTransform.GetComponent<FPS.Player.FpsPlayerController>();
            if (player != null)
            {
                player.SetAlive(true);
                player.CanMove = true;
            }
        }

        public void GameOver()
        {
            isPaused = false;
            Time.timeScale = 0f;
            SetCursorLocked(false);
            SetState(GameState.GameOver);
            if (gameOverMenu != null) gameOverMenu.SetActive(true);
        }

        public void MissionCompleted()
        {
            isPaused = false;
            Time.timeScale = 0f;
            SetCursorLocked(false);
            SetState(GameState.MissionComplete);
            if (missionCompleteMenu != null) missionCompleteMenu.SetActive(true);
        }

        public void ActivateCheckpoint(Checkpoints.Checkpoint checkpoint)
        {
            if (checkpoint == null)
                return;

            currentCheckpoint = checkpoint.transform;

            var hud = GameObject.FindFirstObjectByType<FPS.UI.FpsHud>();
            if (hud != null)
            {
                if (checkpoint.IsFinal)
                    hud.ShowMessage("ÁREA FINAL ALCANÇADA!", 2f);
                else
                    hud.ShowMessage("Checkpoint ativado!", 1.5f);
            }

            if (checkpoint.IsFinal)
            {
                Invoke(nameof(TriggerMissionComplete), 0.5f);
            }
        }

        private void TriggerMissionComplete()
        {
            MissionCompleted();
        }

        private void SetActiveMenu(GameState gs)
        {
            if (pauseMenu != null) pauseMenu.SetActive(gs == GameState.Paused);
            if (gameOverMenu != null) gameOverMenu.SetActive(gs == GameState.GameOver);
            if (missionCompleteMenu != null) missionCompleteMenu.SetActive(gs == GameState.MissionComplete);
            if (hudObject != null) hudObject.SetActive(gs == GameState.Playing || gs == GameState.Paused);
        }

        private void SetState(GameState newState)
        {
            state = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}