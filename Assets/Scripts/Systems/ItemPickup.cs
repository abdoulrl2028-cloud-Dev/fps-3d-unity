using UnityEngine;
using FPS.Health;
using FPS.Weapons;

namespace FPS.Levels
{
    public enum PickupType
    {
        Ammo,
        HealthKit,
        ObjectiveItem
    }

    /// <summary>
    /// Collectible trigger. Applies ammo / heals the player / registers an
    /// objective item with the LevelManager, then disappears.
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [Header("Pickup")]
        [SerializeField] private PickupType type = PickupType.Ammo;
        [SerializeField] private int amount = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Apply(other.transform);
            gameObject.SetActive(false);
        }

        private void Apply(Transform playerRoot)
        {
            switch (type)
            {
                case PickupType.Ammo:
                    AddAmmo(playerRoot, amount);
                    break;
                case PickupType.HealthKit:
                    Heal(playerRoot, amount);
                    break;
                case PickupType.ObjectiveItem:
                    var lm = FPS.Levels.LevelManager.Instance;
                    if (lm != null)
                        lm.ItemCollected();
                    break;
            }
        }

        private static void AddAmmo(Transform playerRoot, int amount)
        {
            var wc = playerRoot.GetComponentInChildren<WeaponController>();
            if (wc == null)
                return;
            var data = wc.EquippedWeapon;
            if (data != null)
                wc.AddAmmo(data, amount);
        }

        private static void Heal(Transform playerRoot, int amount)
        {
            var hs = playerRoot.GetComponentInChildren<HealthSystem>();
            if (hs != null)
                hs.Heal(amount);
        }
    }
}