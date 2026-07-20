using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField, Min(0f)]
        private float damage = 25f;

        [SerializeField, Min(0.1f)]
        private float range = 100f;

        [SerializeField]
        private LayerMask hitMask = ~0;

        public bool Fire(Vector3 origin, Vector3 direction)
        {
            direction.Normalize();

            bool hitSomething = Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                range,
                hitMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hitSomething)
            {
                Debug.DrawRay(
                    origin,
                    direction * range,
                    Color.red,
                    1f
                );

                return false;
            }

            Debug.DrawLine(
                origin,
                hit.point,
                Color.red,
                1f
            );

            Damageable damageable =
                hit.collider.GetComponentInParent<Damageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            return true;
        }
    }
}