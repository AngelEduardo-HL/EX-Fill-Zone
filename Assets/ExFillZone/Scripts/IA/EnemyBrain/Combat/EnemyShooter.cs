using ExFillZone.Gameplay.Combat;
using UnityEngine;

namespace ExFillZone.AI.Enemy.Combat
{
    public sealed class EnemyShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectileWeapon weapon;
        [SerializeField] private Transform muzzle;

        [Header("Shooting")]
        [SerializeField, Min(0.1f)] private float shootRange = 12f;
        [SerializeField, Min(0.1f)] private float stopRange = 5f;
        [SerializeField, Min(0.05f)] private float shootCooldown = 0.75f;
        [SerializeField, Min(0f)] private float turnSpeed = 360f;

        private float nextShotTime;

        public float ShootRange => shootRange;
        public float StopRange => stopRange;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInChildren<ProjectileWeapon>();
            if (muzzle == null && weapon != null) muzzle = weapon.transform;
        }

        public bool IsInRange(Transform target)
        {
            if (target == null) return false;

            Vector3 difference = target.position - transform.position;
            difference.y = 0f;

            return difference.sqrMagnitude <= shootRange * shootRange;
        }

        public bool IsInStopRange(Transform target)
        {
            if (target == null) return false;

            Vector3 difference = target.position - transform.position;
            difference.y = 0f;

            return difference.sqrMagnitude <= stopRange * stopRange;
        }

        public void FaceTarget(Transform target)
        {
            if (target == null) return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        public void TryShoot(Transform target)
        {
            if (target == null || weapon == null || muzzle == null || Time.time < nextShotTime) return;

            Vector3 targetPosition = target.position + Vector3.up * 0.5f;
            Vector3 direction = (targetPosition - muzzle.position).normalized;

            nextShotTime = Time.time + shootCooldown;
            weapon.Fire(muzzle.position + direction * 0.05f, direction);
        }
    }
}