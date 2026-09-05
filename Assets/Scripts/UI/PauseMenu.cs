using UnityEngine;
using UnityEngine.UI;
using FPS.Managers;

namespace FPS.UI
{
    public class PauseMenu : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            BuildPauseUi();
        }

        private void BuildPauseUi()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                gameObject.AddComponent<GraphicRaycaster>();
            }

            GameOverMenu.CreateText(transform, "PAUSA", 56, new Vector2(0.5f, 0.62f), Color.white);

            GameObject btnResume = CreateButton("Continuar", 0f);
            btnResume.GetComponent<Button>().onClick.AddListener(OnResumeClicked);

            GameObject btnRestart = CreateButton("Reiniciar", -0.1f);
            btnRestart.GetComponent<Button>().onClick.AddListener(OnRestartClicked);

            GameObject btnQuit = CreateButton("Sair", -0.2f);
            btnQuit.GetComponent<Button>().onClick.AddListener(OnQuitClicked);
        }

        private GameObject CreateButton(string label, float yPos)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta = new Vector2(240f, 48f);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

            Text txt = GameOverMenu.CreateText(go.transform, label, 22, Vector2.zero, Color.white);
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.anchoredPosition = Vector2.zero;
            txtRt.sizeDelta = Vector2.zero;
            return go;
        }

        private void OnResumeClicked() => GameManager.Instance?.ResumeGame();
        private void OnRestartClicked() => GameManager.Instance?.RestartGame();
        private void OnQuitClicked() => GameManager.Instance?.QuitGame();
    }
}