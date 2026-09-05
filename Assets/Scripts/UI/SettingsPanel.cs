using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    /// <summary>
    /// Settings panel: Master / Music / SFX volume sliders (persisted via
    /// AudioManager). Self-building overlay.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
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

            var canvasGo = new GameObject("SettingsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            panel = new GameObject("SettingsPanel");
            panel.transform.SetParent(canvasGo.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            panel.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.97f);

            GameOverMenu.CreateText(panel.transform, "CONFIGURAÇÕES", 44, new Vector2(0f, 450f), Color.white);

            float y = 300f;
            AddSliderRow("Volume Geral", y, SetMaster);
            AddSliderRow("Volume Música", y - 90f, SetMusic);
            AddSliderRow("Volume SFX", y - 180f, SetSfx);

            Button close = MakeButton(panel.transform, "Fechar", new Vector2(0f, -460f), new Vector2(220f, 44f));
            close.onClick.AddListener(Close);
        }

        private void AddSliderRow(string label, float y, System.Action<float> setter)
        {
            var row = new GameObject(label);
            row.transform.SetParent(panel.transform, false);
            var rt = row.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(700f, 50f);

            GameOverMenu.CreateText(row.transform, label, 26, new Vector2(-240f, 0f), Color.white);

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(row.transform, false);
            var srt = sliderGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(60f, 0f);
            srt.sizeDelta = new Vector2(320f, 30f);
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            System.Action<System.Action<float>> wire = null;
            setter(0.5f);
            slider.onValueChanged.AddListener((v) => setter(v));
        }

        private static void SetMaster(float v)
        {
            if (FPS.Audio.AudioManager.Instance != null)
                FPS.Audio.AudioManager.Instance.SetMaster(v);
        }

        private static void SetMusic(float v)
        {
            if (FPS.Audio.AudioManager.Instance != null)
                FPS.Audio.AudioManager.Instance.SetMusic(v);
        }

        private static void SetSfx(float v)
        {
            if (FPS.Audio.AudioManager.Instance != null)
                FPS.Audio.AudioManager.Instance.SetSfx(v);
        }

        private void Refresh()
        {
            var mgr = FPS.Audio.AudioManager.Instance;
            if (mgr == null)
                return;
            Slider[] sliders = panel.GetComponentsInChildren<Slider>();
            if (sliders.Length > 0) sliders[0].value = mgr.MasterVolume;
            if (sliders.Length > 1) sliders[1].value = mgr.MusicVolume;
            if (sliders.Length > 2) sliders[2].value = mgr.SfxVolume;
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