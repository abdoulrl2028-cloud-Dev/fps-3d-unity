using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    /// <summary>
    /// Player selection (MainMenu). Persists the choice locally. Player models
    /// are placeholders until the Sketchfab players are imported manually.
    /// </summary>
    public class PlayerSelectionMenu : MonoBehaviour
    {
        private static readonly string[][] Options = new string[][]
        {
            new string[] { "player_default",   "Soldado Padrão" },
            new string[] { "player_russian",   "Soldado Russo" },
            new string[] { "player_ukrainian", "Soldado Ucraniano" },
            new string[] { "player_indian",    "Soldado Indiano" }
        };

        private GameObject panel;
        private bool built = false;

        public void Start()
        {
            if (!built)
                BuildUi();
            panel.SetActive(false);
        }

        public void Toggle()
        {
            if (!built)
                BuildUi();
            bool open = !panel.activeSelf;
            panel.SetActive(open);
            if (open)
                Refresh();
        }

        public void Close()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        private void BuildUi()
        {
            built = true;

            var canvasGo = new GameObject("PlayerSelectionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            panel = new GameObject("PlayerPanel");
            panel.transform.SetParent(canvasGo.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            panel.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.97f);

            GameOverMenu.CreateText(panel.transform, "ESCOLHER SOLDADO", 44, new Vector2(0f, 450f), Color.white);

            float y = 250f;
            for (int i = 0; i < Options.Length; i++)
            {
                string captured = Options[i][0];
                string label = Options[i][1];
                Button b = MakeButton(panel.transform, label, new Vector2(0f, y - i * 60f), new Vector2(420f, 48f));
                b.onClick.AddListener(() =>
                {
                    FPS.Progression.ShopProgress.SelectPlayer(captured);
                    Refresh();
                });
            }

            Button close = MakeButton(panel.transform, "Fechar", new Vector2(0f, -460f), new Vector2(220f, 44f));
            close.onClick.AddListener(Close);
        }

        private void Refresh()
        {
            if (panel == null)
                return;
            string sel = FPS.Progression.ShopProgress.SelectedPlayer;
            Text[] texts = panel.GetComponentsInChildren<Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                for (int j = 0; j < Options.Length; j++)
                {
                    string baseLabel = Options[j][1];
                    if (texts[i].text == baseLabel || texts[i].text == baseLabel + "   ✓")
                        texts[i].text = baseLabel + (sel == Options[j][0] ? "   ✓" : "");
                }
            }
        }

        private static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.32f);
            Text txt = GameOverMenu.CreateText(go.transform, label, 22, Vector2.zero, Color.white);
            var t = txt.rectTransform;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.sizeDelta = Vector2.zero;
            return go.GetComponent<Button>();
        }
    }
}