using UnityEngine;
using UnityEngine.UI;
using FPS.Managers;

namespace FPS.UI
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            EnsureUi(gameObject);
            gameObject.SetActive(false);
        }

        private void EnsureUi(GameObject root)
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                root.AddComponent<GraphicRaycaster>();
            }

            titleText = CreateText(root.transform, "GAME OVER", 64, new Vector2(0.5f, 0.62f), Color.red);
            hintText = CreateText(root.transform, "Pressione R para reiniciar no último checkpoint", 26, new Vector2(0.5f, 0.5f), Color.white);
            restartButton = CreateButton(root.transform, "Reiniciar", 0.4f);
            restartButton.onClick.AddListener(Restart);
        }

        private void Restart()
        {
            GameManager.Instance?.RestartGame();
        }

        public static Text CreateText(Transform parent, string content, int size, Vector2 anchoredPos, Color color)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(800f, 90f);
            Text t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return t;
        }

        private Button CreateButton(Transform parent, string label, float yPos)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta = new Vector2(260f, 52f);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

            Text txt = CreateText(go.transform, label, 24, Vector2.zero, Color.white);
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.anchoredPosition = Vector2.zero;
            txtRt.sizeDelta = Vector2.zero;

            return go.GetComponent<Button>();
        }

        public void OnShow()
        {
            // Used by GameManager when activating
        }
    }
}