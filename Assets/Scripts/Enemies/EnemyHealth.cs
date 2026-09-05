using System;
using UnityEngine;

namespace FPS.Enemies
{
    public class EnemyHealth : MonoBehaviour, FPS.Combat.IDamageable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool destroyOnDeath = true;

        public event Action OnDamaged;
        public event Action OnDied;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead { get; private set; } = false;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount, Vector3 hitPoint, Transform attacker)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamaged?.Invoke();

            if (CurrentHealth <= 0)
                Die();
        }

        private void Die()
        {
            if (IsDead)
                return;

            IsDead = true;
            OnDied?.Invoke();

            if (destroyOnDeath)
                Destroy(gameObject, 3f);
        }

        public float GetNormalized()
        {
            return maxHealth <= 0 ? 0f : (float)CurrentHealth / maxHealth;
        }
    }
}