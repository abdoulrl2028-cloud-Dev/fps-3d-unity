using System;
using UnityEngine;
using UnityEngine.UI;
using FPS.Weapons;
using FPS.Health;

namespace FPS.UI
{
    public class FpsHud : MonoBehaviour
    {
        [Header("References (optional; auto-created if null)")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text reserveText;
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject crosshair;

        [Header("Colours")]
        [SerializeField] private Color lowHealthColor = new Color(0.8f, 0.15f, 0.15f);
        [SerializeField] private Color highHealthColor = Color.white;

        [Header("Weapon picking")]
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private HealthSystem playerHealth;

        private RectTransform canvas;
        private Coroutine flashCoroutine;
        private float lastShowTime = 0f;
        private bool messageVisible = false;
        private bool started = false;

        private void Awake()
        {
            if (canvas == null)
                canvas = transform as RectTransform;

            if (weaponController == null)
                weaponController = GameObject.FindFirstObjectByType<WeaponController>();
            if (playerHealth == null)
                playerHealth = GameObject.FindFirstObjectByType<HealthSystem>();

            EnsureUiElements();
        }

        private void Start()
        {
            if (started)
                return;
            started = true;

            if (weaponController != null)
            {
                weaponController.OnAmmoChanged += OnAmmoChanged;
                weaponController.OnWeaponChanged += OnWeaponChanged;
                weaponController.OnReloadStarted += OnReloadStarted;
                weaponController.OnOutOfAmmo += OnOutOfAmmo;
            }

            if (playerHealth != null)
            {
                playerHealth.OnDamaged += RefreshHealth;
            }

            RefreshHealth();
        }

        public void FindPlayerReferences(GameObject playerRoot)
        {
            if (playerRoot == null)
                return;
            weaponController = playerRoot.GetComponentInChildren<WeaponController>();
            playerHealth = playerRoot.GetComponentInChildren<HealthSystem>();
            Start();
        }

        private void EnsureUiElements()
        {
            if (canvas == null)
            {
                GameObject go = new GameObject("FPS HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                go.transform.SetParent(transform);
                Canvas c = go.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvas = go.GetComponent<RectTransform>();
            }
        }

        private void OnDestroy()
        {
            if (weaponController != null)
            {
                weaponController.OnAmmoChanged -= OnAmmoChanged;
                weaponController.OnWeaponChanged -= OnWeaponChanged;
                weaponController.OnReloadStarted -= OnReloadStarted;
                weaponController.OnOutOfAmmo -= OnOutOfAmmo;
            }
            if (playerHealth != null)
                playerHealth.OnDamaged -= RefreshHealth;
        }

        private void OnAmmoChanged(WeaponData data, int current, int reserve)
        {
            UpdateText(ammoText, current.ToString());
            UpdateText(reserveText, reserve.ToString());
        }

        private void OnWeaponChanged(WeaponData data, int current, int reserve)
        {
            UpdateText(weaponNameText, data != null ? data.displayName : "");
            UpdateText(ammoText, current.ToString());
            UpdateText(reserveText, reserve.ToString());

            if (data != null)
            {
                ShowMessage(data.displayName + " equipada", 1.2f);
            }
        }

        private void OnReloadStarted(WeaponData data)
        {
            if (data == null)
                return;
            ShowMessage("Recarregando... " + data.reloadTime.ToString("0.0") + "s", data.reloadTime);
        }

        private void OnOutOfAmmo(int index)
        {
            ShowMessage("SEM MUNIÇÃO! Pressione R para recarregar", 2f);
            FlashLowHealth(Color.yellow);
        }

        private void RefreshHealth()
        {
            float normalized = playerHealth != null ? playerHealth.GetNormalized() : 1f;
            if (healthFill != null)
            {
                healthFill.fillAmount = normalized;
                healthFill.color = Color.Lerp(lowHealthColor, highHealthColor, normalized);
            }
            if (healthText != null)
                UpdateText(healthText, playerHealth != null ? playerHealth.CurrentHealth.ToString() : "100");
        }

        public void ShowMessage(string text, float duration)
        {
            if (messageText == null)
                return;

            messageText.text = text;
            messageText.gameObject.SetActive(true);
            messageVisible = true;
            lastShowTime = Time.unscaledTime + duration;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
        }

        private void FlashLowHealth(Color color)
        {
            if (messageText == null)
                return;
            messageText.color = color;
        }

        private void Update()
        {
            if (messageVisible && messageText != null && Time.unscaledTime >= lastShowTime)
            {
                messageText.gameObject.SetActive(false);
                messageVisible = false;
                if (messageText != null)
                    messageText.color = Color.white;
            }
        }

        private void UpdateText(Text text, string content)
        {
            if (text != null)
                text.text = content;
        }
    }
}