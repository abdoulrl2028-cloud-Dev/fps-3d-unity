using System;
using System.Collections;
using UnityEngine;

namespace FPS.Weapons
{
    [Serializable]
    public class WeaponEntry
    {
        public string id;
        public WeaponData data;
        public GameObject view;
        public AudioSource audioSource;
    }

    public class WeaponController : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private WeaponEntry[] weapons;

        [Header("Combat")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private LayerMask shootMask = ~0;

        [Header("Effects")]
        [SerializeField] private GameObject hitSpark;
        [SerializeField] private GameObject bloodImpact;

        [Header("Ammo State")]
        [SerializeField] private int currentAmmo = 0;
        [SerializeField] private int reserveAmmo = 0;
        [SerializeField] private int equippedIndex = -1;

        private bool isReloading = false;
        private float nextFireTime = 0f;
        private bool canShoot = true;

        public event Action<WeaponData, int, int> OnWeaponChanged;
        public event Action<WeaponData, int, int> OnAmmoChanged;
        public event Action<WeaponData> OnReloadStarted;
        public event Action<int> OnOutOfAmmo;
        public event Action OnFired;

        public int EquippedIndex => equippedIndex;
        public WeaponData EquippedWeapon => GetData(equippedIndex);
        public int CurrentAmmo => currentAmmo;
        public int ReserveAmmo => reserveAmmo;
        public bool IsReloading => isReloading;
        public Camera AimCamera => aimCamera;

        private void Start()
        {
            if (equippedIndex < 0 || equippedIndex >= weapons.Length)
                equippedIndex = weapons.Length > 0 ? 0 : -1;

            if (equippedIndex >= 0)
            {
                ResetAmmoFromData();
                EquipWeapon(equippedIndex);
            }
        }

        private void ResetAmmoFromData()
        {
            foreach (WeaponEntry entry in weapons)
            {
                if (entry.data == null)
                    continue;
                currentAmmo = entry.data.magSize;
                reserveAmmo = entry.data.reserveAmmo;
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].data != null && Input.GetKeyDown(GetSlotKey(weapons[i].data.slot)))
                {
                    if (i != equippedIndex)
                        EquipWeapon(i);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
                StartReload();

            if (Input.GetButton("Fire1") && !isReloading && canShoot)
                TryShoot();
        }

        private KeyCode GetSlotKey(int slot)
        {
            switch (slot)
            {
                case 1: return KeyCode.Alpha1;
                case 2: return KeyCode.Alpha2;
                case 3: return KeyCode.Alpha3;
                case 4: return KeyCode.Alpha4;
                default: return KeyCode.Alpha1;
            }
        }

        private void EquipWeapon(int index)
        {
            if (index < 0 || index >= weapons.Length)
                return;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].view != null)
                    weapons[i].view.SetActive(i == index);
            }

            equippedIndex = index;
            WeaponData data = weapons[index].data;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, data.magSize);
            if (reserveAmmo < 0) reserveAmmo = data.reserveAmmo;

            OnWeaponChanged?.Invoke(data, currentAmmo, reserveAmmo);
            OnAmmoChanged?.Invoke(data, currentAmmo, reserveAmmo);
        }

        private void TryShoot()
        {
            WeaponData data = EquippedWeapon;
            if (data == null)
                return;

            if (isReloading)
                return;

            if (currentAmmo <= 0)
            {
                OnOutOfAmmo?.Invoke(equippedIndex);
                StartCoroutine(OutOfAmmoCooldown());
                return;
            }

            if (Time.time < nextFireTime)
                return;

            Shoot(data);
            currentAmmo--;
            nextFireTime = Time.time + data.fireRate;
            OnAmmoChanged?.Invoke(data, currentAmmo, reserveAmmo);
            OnFired?.Invoke();

            PlayFireSound();

            if (currentAmmo <= 0 && reserveAmmo > 0)
            {
                OnOutOfAmmo?.Invoke(equippedIndex);
            }
        }

        private void Shoot(WeaponData data)
        {
            Vector3 origin = aimCamera != null ? aimCamera.transform.position : MuzzleWorldPosition();
            Vector3 baseDirection = aimCamera != null ? aimCamera.transform.forward : MuzzleForward();

            for (int i = 0; i < data.pellets; i++)
                FirePelletFrom(data, origin, SpreadDirection(baseDirection, data.spread, i));
        }

        private Vector3 MuzzleWorldPosition()
        {
            return muzzlePoint != null ? muzzlePoint.position : transform.position;
        }

        private Vector3 MuzzleForward()
        {
            return muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        }

        private void FirePelletFrom(WeaponData data, Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, data.range, shootMask, QueryTriggerInteraction.Ignore))
            {
                Transform target = hit.collider.transform;

                FPS.Combat.IDamageable damageable = target.GetComponentInParent<FPS.Combat.IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(data.damage, hit.point, transform);
                    SpawnEffect(bloodImpact, hit.point, hit.normal);
                    return;
                }

                SpawnEffect(hitSpark, hit.point, hit.normal);
            }
        }

        private Vector3 SpreadDirection(Vector3 baseDir, float spreadAngle, int seed)
        {
            if (spreadAngle <= 0f)
                return baseDir;

            Quaternion rot = Quaternion.Euler(
                UnityEngine.Random.Range(-spreadAngle, spreadAngle),
                UnityEngine.Random.Range(-spreadAngle, spreadAngle),
                0f);
            return rot * baseDir;
        }

        private void SpawnEffect(GameObject effect, Vector3 point, Vector3 normal)
        {
            if (effect == null)
                return;

            GameObject spawned = Instantiate(effect, point + normal * 0.01f, Quaternion.LookRotation(normal));
            Destroy(spawned, 2f);
        }

        private IEnumerator OutOfAmmoCooldown()
        {
            canShoot = false;
            yield return new WaitForSeconds(0.3f);
            canShoot = true;
        }

        public void StartReload()
        {
            WeaponData data = EquippedWeapon;
            if (data == null || isReloading)
                return;

            if (currentAmmo >= data.magSize || reserveAmmo <= 0)
                return;

            StartCoroutine(ReloadRoutine(data));
        }

        private IEnumerator ReloadRoutine(WeaponData data)
        {
            isReloading = true;
            OnReloadStarted?.Invoke(data);

            yield return new WaitForSeconds(data.reloadTime);

            int needed = data.magSize - currentAmmo;
            int taken = Mathf.Min(needed, reserveAmmo);
            currentAmmo += taken;
            reserveAmmo -= taken;
            isReloading = false;

            OnAmmoChanged?.Invoke(data, currentAmmo, reserveAmmo);
        }

        public void AddAmmo(WeaponData data, int amount)
        {
            reserveAmmo += amount;
            OnAmmoChanged?.Invoke(data, currentAmmo, reserveAmmo);
        }

        public void SetBlocked(bool blocked)
        {
            canShoot = !blocked;
        }

        private void PlayFireSound()
        {
            WeaponData data = EquippedWeapon;
            if (data == null)
                return;

            WeaponEntry entry = GetEntry(equippedIndex);
            if (entry != null && entry.audioSource != null)
            {
                entry.audioSource.PlayOneShot(entry.audioSource.clip);
            }
        }

        private WeaponData GetData(int index)
        {
            if (index < 0 || index >= weapons.Length)
                return null;
            return weapons[index].data;
        }

        private WeaponEntry GetEntry(int index)
        {
            if (index < 0 || index >= weapons.Length)
                return null;
            return weapons[index];
        }
    }
}