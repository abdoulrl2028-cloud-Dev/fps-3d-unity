using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FPS.Progression;

namespace FPS.UI
{
    /// <summary>
    /// Main menu logic + dynamic UI. Starts at the first unlocked level, allows
    /// picking any unlocked level, resetting progress and quitting.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private bool built = false;

        private void Start()
        {
            if (built)
                return;
            built = true;

            EnsureEventSystem();
            BuildUi();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGo.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.09f, 0.12f, 1f);

            Text title = GameOverMenu.CreateText(canvasGo.transform, "FPS 3D — MISSÕES", 72, new Vector2(0f, 380f), Color.white);
            Text subtitle = GameOverMenu.CreateText(canvasGo.transform, "Elimine os inimigos, colete os itens e alcance a saída.", 28, new Vector2(0f, 320f), new Color(0.7f, 0.8f, 1f));

            // Continue (first unlocked or last played)
            int target = Mathf.Max(LevelProgress.UnlockedLevel, LevelProgress.LastPlayed);
            string targetScene = LevelProgress.SceneNameForLevel(target);
            Button continueBtn = CreateMenuButton(canvasGo.transform, "Continuar (Nível " + target.ToString("D2") + ")", 160f);
            if (!string.IsNullOrEmpty(targetScene))
                continueBtn.onClick.AddListener(() => SceneManager.LoadScene(targetScene));
            else
                continueBtn.interactable = false;

            // Level buttons
            float y = 70f;
            float step = 52f;
            for (int i = 1; i <= LevelProgress.LevelCount; i++)
            {
                bool unlocked = LevelProgress.IsLevelUnlocked(i);
                string scene = LevelProgress.SceneNameForLevel(i);
                Button b = CreateMenuButton(canvasGo.transform, "Nível " + i.ToString("D2") + (i == LevelProgress.UnlockedLevel ? "  ▼" : ""), y);
                b.interactable = unlocked;
                if (!string.IsNullOrEmpty(scene))
                {
                    string capturedScene = scene;
                    b.onClick.AddListener(() => SceneManager.LoadScene(capturedScene));
                }
                if (!unlocked)
                {
                    var txt = b.GetComponentInChildren<Text>();
                    if (txt != null)
                        txt.text += "  (bloqueado)";
                }
                y -= step;
            }

            // Reset progress
            Button resetBtn = CreateMenuButton(canvasGo.transform, "Zerar Progresso", y - 20f);
            resetBtn.onClick.AddListener(() =>
            {
                LevelProgress.ResetAllProgress();
                SceneManager.LoadScene("MainMenu");
            });

            // Quit
            Button quitBtn = CreateMenuButton(canvasGo.transform, "Sair", y - 72f);
            quitBtn.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // Arsenal (weapon shop)
            Button arsenalBtn = CreateMenuButton(canvasGo.transform, "Arsenal (Loja)", y - 124f);
            arsenalBtn.onClick.AddListener(() =>
            {
                var shop = GetComponent<WeaponShopMenu>();
                if (shop == null)
                    shop = gameObject.AddComponent<WeaponShopMenu>();
                shop.Toggle();
            });

            // Player selection
            Button playerBtn = CreateMenuButton(canvasGo.transform, "Escolher Soldado", y - 176f);
            playerBtn.onClick.AddListener(() =>
            {
                var sel = GetComponent<PlayerSelectionMenu>();
                if (sel == null)
                    sel = gameObject.AddComponent<PlayerSelectionMenu>();
                sel.Toggle();
            });

            // Settings (volume)
            Button settingsBtn = CreateMenuButton(canvasGo.transform, "Configurações", y - 228f);
            settingsBtn.onClick.AddListener(() =>
            {
                var settings = GetComponent<SettingsPanel>();
                if (settings == null)
                    settings = gameObject.AddComponent<SettingsPanel>();
                settings.Toggle();
            });
        }

        private static Button CreateMenuButton(Transform parent, string label, float y)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(420f, 44f);
            go.GetComponent<Image>().color = new Color(0.18f, 0.2f, 0.26f);

            Text txt = GameOverMenu.CreateText(go.transform, label, 24, Vector2.zero, Color.white);
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.anchoredPosition = Vector2.zero;
            txtRt.sizeDelta = Vector2.zero;
            return go.GetComponent<Button>();
        }
    }
}