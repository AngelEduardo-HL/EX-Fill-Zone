using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public sealed class ProjectileWeapon : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private Projectile projectilePrefab;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 40f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 3f;
        [SerializeField, Min(0.001f)] private float hitRadius = 0.05f;
        [SerializeField] private LayerMask hitMask = ~0;

        public bool Fire(Vector3 origin, Vector3 direction)
        {
            if (projectilePrefab == null || direction.sqrMagnitude <= 0.001f) return false;

            Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            projectile.Initialize(direction, damage, projectileSpeed, projectileLifetime, hitRadius, hitMask);

            return true;
        }
    }
}