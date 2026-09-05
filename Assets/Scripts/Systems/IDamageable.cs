using UnityEngine;

namespace FPS.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int amount, Vector3 hitPoint, Transform attacker);
    }
}