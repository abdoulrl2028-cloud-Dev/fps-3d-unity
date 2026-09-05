using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    /// <summary>
    /// Weapon shop (MainMenu). Self-building panel: rotating 3D preview
    /// placeholder, name, price, Buy / Equip buttons and Locked/Unlocked state.
    /// Progress persists locally via FPS.Progression.ShopProgress.
    /// </summary>
    public class WeaponShopMenu : MonoBehaviour
    {
        private class ShopItem
        {
            public string id;
            public string name;
            public int price;
            public ShopItem(string id, string name, int price)
            {
                this.id = id;
                this.name = name;
                this.price = price;
            }
        }

        private static readonly ShopItem[] Catalog = new ShopItem[]
        {
            new ShopItem("w_ia2",    "Fuzil IA2",     0),
            new ShopItem("w_ak12",   "AK-12",         500),
            new ShopItem("w_l85a2",  "L85A2",         700),
            new ShopItem("w_mp40",   "MP40",          450),
            new ShopItem("w_pm12",   "Beretta PM12",  600),
            new ShopItem("w_sterling","Sterling SMG", 750),
            new ShopItem("w_m4a1",   "M4A1",          800),
            new ShopItem("w_pp19",   "PP-19 Vityaz",  900),
            new ShopItem("w_p90",    "FN P90",       1200),
            new ShopItem("w_honey",  "Honey Badger", 1500)
        };

        private GameObject panel;
        private Text coinText;
        private GameObject previewCube;
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
            {
                GrantStarterFundsIfNeeded();
                Refresh();
            }
        }

        public void Close()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        private static void GrantStarterFundsIfNeeded()
        {
            if (!PlayerPrefs.HasKey("FPS_Shop_StarterGranted"))
            {
                FPS.Progression.ShopProgress.AddCoins(2500);
                FPS.Progression.ShopProgress.GiveDefaultItem("w_ia2");
                PlayerPrefs.SetInt("FPS_Shop_StarterGranted", 1);
                PlayerPrefs.Save();
            }
        }

        private void BuildUi()
        {
            built = true;

            var canvasGo = new GameObject("WeaponShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            panel = new GameObject("ShopPanel");
            panel.transform.SetParent(canvasGo.transform, false);
            var prt = panel.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            var pimg = panel.AddComponent<Image>();
            pimg.color = new Color(0.04f, 0.05f, 0.08f, 0.97f);

            GameOverMenu.CreateText(panel.transform, "ARSENAL", 46, new Vector2(0f, 500f), Color.white);
            coinText = GameOverMenu.CreateText(panel.transform, "Moedas: 0", 26, new Vector2(0f, 450f), new Color(1f, 0.85f, 0.4f));

            // rotating 3D preview placeholder
            var previewGo = new GameObject("Preview3D");
            previewGo.transform.SetParent(panel.transform, false);
            var prt2 = previewGo.AddComponent<RectTransform>();
            prt2.anchorMin = new Vector2(0.5f, 0.5f);
            prt2.anchorMax = new Vector2(0.5f, 0.5f);
            prt2.anchoredPosition = new Vector2(430f, 160f);
            prt2.sizeDelta = new Vector2(300f, 300f);
            previewCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewCube.name = "WeaponPreviewPlaceholder";
            previewCube.transform.SetParent(previewGo.transform, false);
            previewCube.transform.localPosition = Vector3.zero;
            previewCube.transform.localScale = new Vector3(0.7f, 0.18f, 0.14f);
            var mat = previewCube.GetComponent<Renderer>().material;
            if (mat != null)
                mat.color = new Color(0.25f, 0.3f, 0.35f);

            float y = 300f;
            float step = 46f;
            for (int i = 0; i < Catalog.Length; i++)
                BuildRow(Catalog[i], y - i * step);

            Button closeBtn = CreateButton(panel.transform, "Fechar", new Vector2(0f, -510f), 220f, 38f);
            closeBtn.onClick.AddListener(Close);
        }

        private void BuildRow(ShopItem item, float y)
        {
            var row = new GameObject(item.name);
            row.transform.SetParent(panel.transform, false);
            var rt = row.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(820f, 40f);

            Text name = GameOverMenu.CreateText(row.transform, item.name + "  •  $" + item.price, 21, Vector2.zero, Color.white);
            var nrt = name.rectTransform;
            nrt.anchorMin = new Vector2(0.5f, 0.5f);
            nrt.anchorMax = new Vector2(0.5f, 0.5f);
            nrt.anchoredPosition = new Vector2(-290f, 0f);
            nrt.sizeDelta = new Vector2(360f, 34f);

            Text state = GameOverMenu.CreateText(row.transform, "", 19, Vector2.zero, new Color(0.6f, 1f, 0.6f));
            var srt = state.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(70f, 0f);
            srt.sizeDelta = new Vector2(140f, 30f);

            Button buy = CreateButton(row.transform, "Buy", new Vector2(150f, 0f), 100f, 34f);
            Button equip = CreateButton(row.transform, "Equip", new Vector2(260f, 0f), 110f, 34f);

            string captured = item.id;
            buy.onClick.AddListener(() =>
            {
                if (FPS.Progression.ShopProgress.IsOwned(captured))
                    return;
                FPS.Progression.ShopProgress.BuyWeapon(captured, item.price);
                Refresh();
            });
            equip.onClick.AddListener(() =>
            {
                FPS.Progression.ShopProgress.EquipWeapon(captured);
                Refresh();
            });

            row.AddComponent<ShopRowTag>().Init(captured, buy, equip, state);
        }

        private void Refresh()
        {
            if (!built)
                return;
            if (coinText != null)
                coinText.text = "Moedas: " + FPS.Progression.ShopProgress.Coins;
            ShopRowTag[] rows = GetComponentsInChildren<ShopRowTag>(true);
            for (int i = 0; i < rows.Length; i++)
                rows[i].Refresh();
        }

        private void Update()
        {
            if (built && panel != null && panel.activeSelf && previewCube != null)
                previewCube.transform.Rotate(0f, 40f * Time.deltaTime, 0f, Space.World);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 pos, float width, float height)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);
            go.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.32f);
            Text txt = GameOverMenu.CreateText(go.transform, label, 19, Vector2.zero, Color.white);
            var t = txt.rectTransform;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.sizeDelta = Vector2.zero;
            return go.GetComponent<Button>();
        }

        private class ShopRowTag : MonoBehaviour
        {
            private string id;
            private Button buy;
            private Button equip;
            private Text stateText;

            public void Init(string id, Button buy, Button equip, Text stateText)
            {
                this.id = id;
                this.buy = buy;
                this.equip = equip;
                this.stateText = stateText;
                Refresh();
            }

            public void Refresh()
            {
                bool owned = FPS.Progression.ShopProgress.IsOwned(id);
                bool equipped = FPS.Progression.ShopProgress.IsEquipped(id);
                if (buy != null) buy.interactable = !owned;
                if (equip != null) equip.interactable = owned && !equipped;
                if (stateText != null)
                    stateText.text = equipped ? "Equipado" : (owned ? "Desbloqueado" : "Bloqueado");
            }
        }
    }
}