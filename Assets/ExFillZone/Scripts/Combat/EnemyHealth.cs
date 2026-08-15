using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public sealed class EnemyHealth : Damageable
    {
        [Header("Health")]
        [SerializeField, Min(1f)]
        private float maxHealth = 50f;

        [Header("Drop")]
        [SerializeField]
        private GameObject ammoDrop;

        [SerializeField]
        private Transform dropPoint;

        private float currentHealth;
        private bool isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

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

            Debug.Log(
                $"{name} recibió {damage} de daño. " +
                $"Vida: {currentHealth}/{maxHealth}",
                this
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

            DropAmmo();

            Destroy(gameObject);
        }

        private void DropAmmo()
        {
            if (ammoDrop == null)
            {
                return;
            }

            Vector3 position =
                dropPoint != null
                    ? dropPoint.position
                    : transform.position + Vector3.up * 0.25f;

            Instantiate(
                ammoDrop,
                position,
                Quaternion.identity
            );
        }
    }
}