using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public sealed class TargetHealth : Damageable
    {
        [SerializeField, Min(1f)]
        private float maximumHealth = 50f;

        private float currentHealth;

        private void Awake()
        {
            currentHealth = maximumHealth;
        }

        public override void TakeDamage(float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            currentHealth -= damage;

            Debug.Log(
                $"{name} recibió {damage} de daño. " +
                $"Vida restante: {currentHealth}",
                this
            );

            if (currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}