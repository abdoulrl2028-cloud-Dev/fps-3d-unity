using UnityEngine;

namespace FPS.Weapons
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "FPS/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string displayName = "Weapon";
        public int damage = 10;
        public float fireRate = 0.5f;
        public float range = 100f;
        public int magSize = 12;
        public int reserveAmmo = 60;
        public float reloadTime = 1.5f;
        [Tooltip("Number of pellets. Shotguns use many; pistol/rifle use 1.")]
        public int pellets = 1;
        [Tooltip("Scatter half-angle in degrees for pellets (0 = perfect spread).")]
        public float spread = 0f;
        public int slot = 1;
    }
}