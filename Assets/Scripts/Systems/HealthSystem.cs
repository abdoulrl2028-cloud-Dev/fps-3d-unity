using System;
using UnityEngine;

namespace FPS.Health
{
    public class HealthSystem : MonoBehaviour, FPS.Combat.IDamageable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private bool destroyOnDeath = false;

        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDied;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead { get; private set; } = false;

        private void Awake()
        {
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }

        public void TakeDamage(int amount, Vector3 hitPoint, Transform attacker)
        {
            if (IsDead || amount <= 0)
                return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnDamaged?.Invoke();

            if (currentHealth <= 0)
                Die();
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealed?.Invoke();
        }

        public void RestoreFull()
        {
            currentHealth = maxHealth;
            IsDead = false;
            OnHealed?.Invoke();
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
            if (maxHealth <= 0)
                return 0f;
            return (float)currentHealth / maxHealth;
        }
    }
}