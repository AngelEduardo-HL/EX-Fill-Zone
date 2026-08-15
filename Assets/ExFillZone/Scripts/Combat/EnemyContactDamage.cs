using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.AI.Enemy.Combat
{
    public sealed class EnemyContactDamage : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField, Min(0f)]
        private float damage = 20f;

        [SerializeField, Min(0.1f)]
        private float damageCooldown = 1f;

        private float nextDamageTime;

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextDamageTime)
            {
                return;
            }

            PlayerHealth player =
                other.GetComponentInParent<PlayerHealth>();

            if (player == null)
            {
                return;
            }

            player.TakeDamage(damage);

            nextDamageTime =
                Time.time + damageCooldown;
        }
    }
}