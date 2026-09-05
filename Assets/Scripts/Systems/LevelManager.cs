using UnityEngine;
using UnityEngine.SceneManagement;
using FPS.Progression;
using FPS.Health;

namespace FPS.Levels
{
    /// <summary>
    /// Per-level flow controller. Present in every level scene.
    /// - Counts live enemies (tag "Enemy").
    /// - Collects objective items (via ItemPickup / ExitZone triggers).
    /// - When the player reaches the exit with all enemies dead and all required
    ///   items collected: saves progress, unlocks the next level and shows the
    ///   mission complete menu with a Continue button.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Objective")]
        [SerializeField] private int requiredItems = 0;

        [Header("HUD")]
        [SerializeField] private FPS.UI.FpsHud hud;

        private int enemiesAlive = 0;
        private int itemsCollected = 0;
        private bool completed = false;
        private string nextLevelName = "";

        public int EnemiesAlive => enemiesAlive;

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
            if (hud == null)
                hud = GameObject.FindFirstObjectByType<FPS.UI.FpsHud>();

            TrackEnemies();
            PushObjectiveHint();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            foreach (var hs in FindAllEnemyHealth())
            {
                if (hs != null)
                    hs.OnDied -= OnEnemyDied;
            }
        }

        private void TrackEnemies()
        {
            enemiesAlive = 0;
            foreach (var hs in FindAllEnemyHealth())
            {
                if (hs == null || hs.IsDead)
                    continue;
                enemiesAlive++;
                hs.OnDied -= OnEnemyDied;
                hs.OnDied += OnEnemyDied;
            }
        }

        private System.Collections.Generic.List<HealthSystem> FindAllEnemyHealth()
        {
            var list = new System.Collections.Generic.List<HealthSystem>();
            foreach (var go in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                var hs = go.GetComponentInChildren<HealthSystem>();
                if (hs != null)
                    list.Add(hs);
            }
            return list;
        }

        private void OnEnemyDied()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            if (enemiesAlive <= 0)
                ShowMessage("Todos os inimigos eliminados! Vá até a saída.");
        }

        /// <summary>Called by ItemPickup when an objective item is collected.</summary>
        public void ItemCollected()
        {
            itemsCollected++;
            ShowMessage("Item coletado (" + itemsCollected + "/" + requiredItems + ")");
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (AllObjectivesDone())
                ShowMessage("Objetivos concluídos! Vá até a saída.");
        }

        /// <summary>Called by ExitZone when the player enters the exit trigger.</summary>
        public void OnExitZoneEnter()
        {
            if (completed)
                return;

            if (!AllObjectivesDone())
            {
                ShowMessage(PendingObjectiveText());
                return;
            }

            CompleteLevel();
        }

        public bool AllObjectivesDone()
        {
            return enemiesAlive <= 0 && itemsCollected >= requiredItems;
        }

        private string PendingObjectiveText()
        {
            if (enemiesAlive > 0 && itemsCollected < requiredItems)
                return "Faltam eliminar " + enemiesAlive + " inimigo(s) e coletar " +
                       (requiredItems - itemsCollected) + " item(ns).";
            if (enemiesAlive > 0)
                return "Ainda faltam " + enemiesAlive + " inimigo(s).";
            return "Ainda faltam " + (requiredItems - itemsCollected) + " item(ns).";
        }

        private void PushObjectiveHint()
        {
            int lvl = LevelProgress.GetCurrentLevelFromSceneName();
            string total = lvl > 0 ? "Nível " + lvl + " - " : "";
            if (requiredItems > 0)
                ShowMessage(total + "Objetivo: elimine todos os inimigos e colete " + requiredItems + " item(ns) para chegar à saída.");
            else
                ShowMessage(total + "Objetivo: elimine todos os inimigos e chegue à saída.");
        }

        private void CompleteLevel()
        {
            if (completed)
                return;
            completed = true;

            int lvl = LevelProgress.GetCurrentLevelFromSceneName();
            LevelProgress.CompleteLevel(lvl > 0 ? lvl : 1);
            nextLevelName = LevelProgress.GetNextLevelName(lvl > 0 ? lvl : 1);

            var gm = FPS.Managers.GameManager.Instance;
            if (gm != null)
                gm.MissionCompleted();

            var menu = GameObject.FindFirstObjectByType<FPS.UI.MissionCompleteMenu>();
            if (menu != null)
                menu.SetNextLevel(nextLevelName);

            ShowMessage(completed ? "Missão concluída!" : "");
        }

        public void ContinueToNextLevel()
        {
            Time.timeScale = 1f;
            if (string.IsNullOrEmpty(nextLevelName))
                SceneManager.LoadScene("MainMenu");
            else
                SceneManager.LoadScene(nextLevelName);
        }

        /// <summary>Restart current level (used by menus after death/restart).</summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ShowMessage(string text)
        {
            if (hud != null)
                hud.ShowMessage(text, 3f);
        }
    }
}