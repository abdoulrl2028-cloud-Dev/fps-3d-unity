using System;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Dialogue
{
    /// <summary>
    /// Local dialogue player. Builds its own overlay panel (Speaker, Text,
    /// Next, Skip) and plays lines with a typewriter effect. Fully offline.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public event Action OnDialogueStarted;
        public event Action OnDialogueFinished;

        private Text speakerText;
        private Text lineText;
        private GameObject panel;
        private Button nextButton;

        private DialogueData current;
        private int index = 0;
        private string currentFullLine = "";
        private int charsShown = 0;
        private float typeSpeed = 0.025f;
        private float timer = 0f;
        private bool typing = false;
        private bool built = false;

        public bool IsOpen { get { return panel != null && panel.activeSelf; } }
        public DialogueData Current { get { return current; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Show(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
                return;

            if (!built)
                BuildUi();

            current = dialogue;
            index = 0;
            panel.SetActive(true);
            if (OnDialogueStarted != null)
                OnDialogueStarted();
            ShowLine(current.lines[0]);
        }

        public void Next()
        {
            if (current == null)
                return;

            if (typing)
            {
                charsShown = currentFullLine.Length;
                lineText.text = currentFullLine;
                typing = false;
                return;
            }

            index++;
            if (index >= current.lines.Length)
            {
                Close();
                return;
            }
            ShowLine(current.lines[index]);
        }

        public void SkipAll()
        {
            if (current == null)
                return;
            index = current.lines.Length;
            Close();
        }

        public void Close()
        {
            if (panel != null)
                panel.SetActive(false);
            current = null;
            if (OnDialogueFinished != null)
                OnDialogueFinished();
        }

        private void ShowLine(DialogueData.Line line)
        {
            speakerText.text = line.speaker;
            currentFullLine = line.text;
            charsShown = 0;
            typing = true;
            timer = 0f;
            lineText.text = "";

            if (line.voiceClip != null && Audio != null)
            {
                AudioSource temp = GetComponent<AudioSource>();
                if (temp == null)
                    temp = gameObject.AddComponent<AudioSource>();
                temp.PlayOneShot(line.voiceClip);
            }
        }

        private FPS.Audio.AudioManager Audio { get { return FPS.Audio.AudioManager.Instance; } }

        private void Update()
        {
            if (!typing || lineText == null)
                return;

            timer += Time.deltaTime;
            int target = Mathf.FloorToInt(timer / typeSpeed);
            if (target > charsShown)
            {
                charsShown = Mathf.Min(target, currentFullLine.Length);
                lineText.text = currentFullLine.Substring(0, charsShown);
                if (charsShown >= currentFullLine.Length)
                    typing = false;
            }
        }

        private void BuildUi()
        {
            built = true;

            var canvasGo = new GameObject("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            panel = new GameObject("DialogueBox");
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.15f, 0f);
            panelRt.anchorMax = new Vector2(0.85f, 0f);
            panelRt.anchoredPosition = new Vector2(0f, 40f);
            panelRt.sizeDelta = new Vector2(0f, 170f);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.07f, 0.1f, 0.92f);

            speakerText = FPS.UI.GameOverMenu.CreateText(panel.transform, "", 26, new Vector2(0f, 60f), new Color(0.6f, 0.85f, 1f));
            var spRt = speakerText.GetComponent<RectTransform>();
            spRt.anchorMin = new Vector2(0f, 1f);
            spRt.anchorMax = new Vector2(1f, 1f);
            spRt.anchoredPosition = new Vector2(0f, -40f);
            spRt.sizeDelta = new Vector2(0f, 40f);

            lineText = FPS.UI.GameOverMenu.CreateText(panel.transform, "", 22, Vector2.zero, Color.white);
            var lnRt = lineText.GetComponent<RectTransform>();
            lnRt.anchorMin = new Vector2(0.02f, 0f);
            lnRt.anchorMax = new Vector2(0.98f, 1f);
            lnRt.anchoredPosition = new Vector2(0f, 0f);
            lnRt.sizeDelta = new Vector2(0f, 80f);

            nextButton = CreateButton(panel.transform, "Next ›", new Vector2(180f, -30f));
            Button skipButton = CreateButton(panel.transform, "Skip", new Vector2(350f, -30f));

            nextButton.onClick.AddListener(Next);
            skipButton.onClick.AddListener(SkipAll);

            panel.SetActive(false);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 pos)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(140f, 42f);
            go.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.42f);
            Text txt = FPS.UI.GameOverMenu.CreateText(go.transform, label, 20, Vector2.zero, Color.white);
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            return go.GetComponent<Button>();
        }
    }
}