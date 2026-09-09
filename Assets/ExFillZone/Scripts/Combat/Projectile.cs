using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public sealed class Projectile : MonoBehaviour
    {
        private Vector3 direction;
        private float damage;
        private float speed;
        private float radius;
        private float destroyTime;
        private LayerMask hitMask;
        private bool hasHit;

        public void Initialize(Vector3 shootDirection, float projectileDamage, float projectileSpeed, float lifetime, float hitRadius, LayerMask mask)
        {
            direction = shootDirection.normalized;
            damage = projectileDamage;
            speed = projectileSpeed;
            radius = hitRadius;
            hitMask = mask;
            destroyTime = Time.time + lifetime;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Update()
        {
            if (hasHit) return;

            float distance = speed * Time.deltaTime;

            if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, distance, hitMask, QueryTriggerInteraction.Ignore))
            {
                Hit(hit);
                return;
            }

            transform.position += direction * distance;

            if (Time.time >= destroyTime) Destroy(gameObject);
        }

        private void Hit(RaycastHit hit)
        {
            hasHit = true;
            transform.position = hit.point;

            Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
            if (damageable != null) damageable.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}