using System.Collections;
using UnityEngine;

namespace FPS.Player
{
    /// <summary>
    /// Equip system: attaches a weapon view under the Player's WeaponHolder,
    /// plays fire/reload SFX and exposes a muzzle flash placeholder. The real
    /// weapon handling is done by WeaponController (FPS.Weapons).
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private SkinnedMeshFlash visuals;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip fireClip;
        [SerializeField] private AudioClip reloadClip;

        private GameObject equippedView;
        private FPS.Weapons.WeaponController weaponController;

        public Transform WeaponHolder
        {
            get
            {
                if (weaponHolder == null)
                {
                    Transform h = transform.Find("WeaponHolder");
                    if (h != null)
                        weaponHolder = h;
                    else
                        weaponHolder = transform;
                }
                return weaponHolder;
            }
        }

        private void Awake()
        {
            weaponController = GetComponentInChildren<FPS.Weapons.WeaponController>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        public void EquipWeapon(GameObject viewPrefab)
        {
            if (equippedView != null)
                Destroy(equippedView);
            if (viewPrefab == null)
                return;

            equippedView = Instantiate(viewPrefab, WeaponHolder);
            equippedView.transform.localPosition = Vector3.zero;
            equippedView.transform.localRotation = Quaternion.identity;
        }

        public void EquipWeaponData(FPS.Weapons.WeaponData data, GameObject viewPrefab, GameObject holder)
        {
            if (data == null)
                return;
            EquipWeapon(viewPrefab);
            if (holder != null)
                weaponHolder = holder.transform;
        }

        public void PlayFireSfx()
        {
            if (sfxSource != null && fireClip != null)
            {
                if (FPS.Audio.AudioManager.Instance != null)
                    FPS.Audio.AudioManager.Instance.PlaySfx(sfxSource, fireClip);
                else
                    sfxSource.PlayOneShot(fireClip, 0.8f);
            }
        }

        public void PlayReloadSfx()
        {
            if (sfxSource != null && reloadClip != null)
            {
                if (FPS.Audio.AudioManager.Instance != null)
                    FPS.Audio.AudioManager.Instance.PlaySfx(sfxSource, reloadClip, 0.8f);
                else
                    sfxSource.PlayOneShot(reloadClip, 0.8f);
            }
        }

        public void SetMuzzle(bool active)
        {
            if (visuals != null)
                StartCoroutine(FlashRoutine(active));
        }

        private IEnumerator FlashRoutine(bool active)
        {
            yield return null;
        }

        private void OnEnable()
        {
            // Hook weapon controller events to play SFX + muzzle flash
            if (weaponController == null)
                return;
            weaponController.OnFired += PlayFireSfx;
            weaponController.OnReloadStarted += OnReloadStarted;
        }

        private void OnDisable()
        {
            if (weaponController == null)
                return;
            weaponController.OnFired -= PlayFireSfx;
            weaponController.OnReloadStarted -= OnReloadStarted;
        }

        private void OnReloadStarted(FPS.Weapons.WeaponData data)
        {
            PlayReloadSfx();
        }
    }

    /// <summary>Simple muzzle flash material override (placeholder).</summary>
    [System.Serializable]
    public class SkinnedMeshFlash
    {
        public GameObject flashObject;
    }
}