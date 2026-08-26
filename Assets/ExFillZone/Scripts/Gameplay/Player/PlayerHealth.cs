using ExFillZone.Gameplay.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace ExFillZone.Gameplay.Player
{
    public sealed class PlayerHealth : Damageable
    {
        [Header("Health")]
        [SerializeField, Min(1f)]
        private float maxHealth = 100f;

        [Header("Game Over")]
        [SerializeField]
        private string defeatScene = "Defeat";

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth =>
            currentHealth;

        public float MaxHealth =>
            maxHealth;



        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public event Action<float, float> HealthChanged;

        public float NormalizedHealth =>
            maxHealth > 0f
                ? currentHealth / maxHealth
                : 0f;

        public override void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
            {
                return;
            }

            currentHealth -= damage;

            currentHealth = Mathf.Max(
                currentHealth,
                0f
            );
            HealthChanged?.Invoke(
                currentHealth,
                maxHealth
);

            Debug.Log(
                $"Vida: {currentHealth}/{maxHealth}"
            );

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            SceneManager.LoadScene(
                defeatScene
            );
        }
    }
}