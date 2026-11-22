using ExFillZone.AI.Enemy.Navigation;
using ExFillZone.Gameplay.Enemies;
using ExFillZone.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.AI;

namespace ExFillZone.AI.Enemy.Combat
{
    // Optional tactical layer: only enemies with this component use the tactical Panda branch.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyNavigator))]
    public sealed class EnemyCombatTactics : MonoBehaviour
    {
        [System.Serializable]
        private sealed class TacticalSettings
        {
            [Min(0.5f)] public float preferredDistance = 7f;
            [Min(0f)] public float retreatDistance = 3f;
            [Min(0.1f)] public float decisionInterval = 0.4f;
            [Min(0f)] public float strafeDistance = 0f;
            [Min(0.2f)] public float strafeInterval = 2f;
        }

        [Header("References")]
        [SerializeField] private EnemyLoadoutGenerator loadoutGenerator;
        [SerializeField] private EnemyShooter shooter;

        [Header("Militant")]
        [SerializeField] private TacticalSettings militant = new TacticalSettings();

        [Header("Marksman")]
        [SerializeField] private TacticalSettings marksman = new TacticalSettings
        {
            preferredDistance = 8.5f,
            retreatDistance = 5f,
            decisionInterval = 0.4f
        };

        [Header("Assault")]
        [SerializeField] private TacticalSettings assault = new TacticalSettings
        {
            preferredDistance = 3.5f,
            retreatDistance = 0f,
            decisionInterval = 0.25f
        };

        [Header("Hunter")]
        [SerializeField] private TacticalSettings hunter = new TacticalSettings
        {
            preferredDistance = 7f,
            retreatDistance = 3.5f,
            decisionInterval = 0.35f,
            strafeDistance = 4f,
            strafeInterval = 2f
        };

        [Header("Navigation")]
        [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 2f;

        private EnemyNavigator navigator;
        private NavMeshPath path;
        private float nextDecisionTime;
        private float nextStrafeTime;
        private float strafingUntil;
        private float strafeDirection = 1f;

        private void Awake()
        {
            navigator = GetComponent<EnemyNavigator>();
            path = new NavMeshPath();

            if (loadoutGenerator == null) loadoutGenerator = GetComponent<EnemyLoadoutGenerator>();
            if (shooter == null) shooter = GetComponent<EnemyShooter>();
        }

        public bool Execute(Transform target, out bool shooting)
        {
            shooting = false;
            if (target == null || navigator == null || shooter == null) return false;

            TacticalSettings settings = GetSettings();
            bool armed = HasEquippedWeapon();

            Vector3 toPlayer = target.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance < 0.01f) return true;

            shooter.FaceTarget(target);

            if (armed && shooter.IsInRange(target))
            {
                shooter.TryShoot(target);
                shooting = true;
            }

            if (Time.time < nextDecisionTime) return true;
            nextDecisionTime = Time.time + Mathf.Max(0.1f, settings.decisionInterval);

            float preferred = armed ? Mathf.Min(settings.preferredDistance, shooter.ShootRange * 0.9f) : 2f;

            if (armed && settings.retreatDistance > 0f && distance < settings.retreatDistance)
            {
                Vector3 away = transform.position - toPlayer.normalized * Mathf.Max(3f, settings.preferredDistance - distance);
                if (TryNavigate(away))
                {
                    strafingUntil = 0f;
                    return true;
                }
            }

            if (distance > preferred + 0.75f)
            {
                strafingUntil = 0f;
                navigator.SetDestination(target.position);
                return true;
            }

            if (armed && settings.strafeDistance > 0f && Time.time >= nextStrafeTime && shooter.IsInRange(target))
            {
                nextStrafeTime = Time.time + settings.strafeInterval;
                strafeDirection *= -1f;

                Vector3 sideways = Vector3.Cross(Vector3.up, toPlayer.normalized) * strafeDirection;
                if (TryNavigate(transform.position + sideways * settings.strafeDistance))
                {
                    strafingUntil = Time.time + settings.strafeInterval * 0.8f;
                    return true;
                }
            }

            if (Time.time < strafingUntil && !navigator.HasReachedDestination) return true;

            strafingUntil = 0f;
            navigator.Stop();
            return true;
        }

        private TacticalSettings GetSettings()
        {
            EnemyArchetype archetype = loadoutGenerator != null && loadoutGenerator.Profile != null
                ? loadoutGenerator.Profile.Archetype
                : EnemyArchetype.Militant;

            switch (archetype)
            {
                case EnemyArchetype.Marksman: return marksman;
                case EnemyArchetype.Assault: return assault;
                case EnemyArchetype.Hunter: return hunter;
                default: return militant;
            }
        }

        private bool HasEquippedWeapon()
        {
            CharacterInventory inventory = loadoutGenerator != null ? loadoutGenerator.Inventory : null;
            if (inventory == null) return false;

            return inventory.Equipment.GetItem(EquipmentSlotType.PrimaryWeapon) != null ||
                   inventory.Equipment.GetItem(EquipmentSlotType.SecondaryWeapon) != null;
        }

        private bool TryNavigate(Vector3 destination)
        {
            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas)) return false;
            if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path)) return false;
            return path.status == NavMeshPathStatus.PathComplete && navigator.SetDestination(hit.position);
        }
    }
}
