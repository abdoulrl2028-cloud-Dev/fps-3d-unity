using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FPS.Managers;

namespace FPS.UI
{
    /// <summary>
    /// Mission complete overlay. When a LevelManager sets a next level name it
    /// shows a Continue button; an empty name means the final level was beaten
    /// (victory) and the button returns to the main menu.
    /// </summary>
    public class MissionCompleteMenu : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueLabel;

        private string nextLevelName = "";
        private bool victoryMode = false;

        private void Awake()
        {
            EnsureUi(gameObject);
            gameObject.SetActive(false);
        }

        public void EnsureUi(GameObject root)
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                root.AddComponent<GraphicRaycaster>();
            }

            titleText = GameOverMenu.CreateText(root.transform, "MISSÃO CONCLUÍDA", 64, new Vector2(0.5f, 0.62f), Color.green);
            subtitleText = GameOverMenu.CreateText(root.transform, "Objetivo cumprido!", 28, new Vector2(0.5f, 0.52f), Color.white);
            GameOverMenu.CreateText(root.transform, "R para jogar novamente", 24, new Vector2(0.5f, 0.36f), Color.white);

            GameObject btnGo = new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(root.transform, false);
            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -0.28f);
            rt.sizeDelta = new Vector2(340f, 56f);
            btnGo.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.2f);

            Text txt = GameOverMenu.CreateText(btnGo.transform, "Continuar", 24, Vector2.zero, Color.white);
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.anchoredPosition = Vector2.zero;
            txtRt.sizeDelta = Vector2.zero;

            continueButton = btnGo.GetComponent<Button>();
            continueLabel = txt;
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        /// <summary>Called by LevelManager with the name of the following level ("" = victory).</summary>
        public void SetNextLevel(string nextLevel)
        {
            nextLevelName = nextLevel;
            victoryMode = string.IsNullOrEmpty(nextLevelName);

            if (victoryMode)
            {
                if (titleText != null) titleText.text = "VITÓRIA!";
                if (subtitleText != null) subtitleText.text = "Você completou todos os níveis!";
                if (continueLabel != null) continueLabel.text = "Voltar ao Menu";
            }
            else
            {
                if (titleText != null) titleText.text = "MISSÃO CONCLUÍDA";
                if (subtitleText != null) subtitleText.text = "Nível " + NextLevelNumber() + " desbloqueado!";
                if (continueLabel != null) continueLabel.text = "Próximo Nível";
            }
        }

        private int NextLevelNumber()
        {
            int n;
            if (int.TryParse(nextLevelName.Replace("Level", ""), out n))
                return n;
            return 0;
        }

        private void OnContinueClicked()
        {
            Time.timeScale = 1f;
            if (victoryMode || string.IsNullOrEmpty(nextLevelName))
                SceneManager.LoadScene("MainMenu");
            else
                SceneManager.LoadScene(nextLevelName);
        }
    }
}