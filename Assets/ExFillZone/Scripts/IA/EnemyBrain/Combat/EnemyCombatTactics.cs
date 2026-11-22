using ExFillZone.AI.Enemy.Navigation;
using ExFillZone.AI.Enemy.Perception;
using ExFillZone.Gameplay.Enemies;
using ExFillZone.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.AI;

namespace ExFillZone.AI.Enemy.Combat
{
    // Optional combat layer: existing enemies without this component keep their original Panda BT combat.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyNavigator))]
    public sealed class EnemyCombatTactics : MonoBehaviour
    {
        [System.Serializable]
        private sealed class TacticalSettings
        {
            [Min(0.5f)] public float preferredDistance = 6f;
            [Min(0f)] public float retreatDistance = 2.5f;
            [Min(0.1f)] public float decisionInterval = 0.35f;
            [Min(0f)] public float strafeDistance = 0f;
            [Min(0.2f)] public float strafeInterval = 2f;
            [Min(0.1f)] public float speedMultiplier = 1f;
        }

        [Header("References")]
        [SerializeField] private EnemyLoadoutGenerator loadoutGenerator;
        [SerializeField] private EnemyShooter shooter;

        [Header("Militant - simple advance and hold")]
        [SerializeField] private TacticalSettings militant = new TacticalSettings
        {
            preferredDistance = 6f,
            retreatDistance = 2.2f,
            decisionInterval = 0.45f,
            speedMultiplier = 1f
        };

        [Header("Marksman - maintain long range")]
        [SerializeField] private TacticalSettings marksman = new TacticalSettings
        {
            preferredDistance = 8.5f,
            retreatDistance = 7.5f,
            decisionInterval = 0.4f,
            speedMultiplier = 0.9f
        };

        [Header("Assault - fast rush and close orbit")]
        [SerializeField] private TacticalSettings assault = new TacticalSettings
        {
            preferredDistance = 2.4f,
            retreatDistance = 0f,
            decisionInterval = 0.2f,
            strafeDistance = 2.3f,
            strafeInterval = 1.2f,
            speedMultiplier = 1.4f
        };

        [Header("Hunter - sustained flanking")]
        [SerializeField] private TacticalSettings hunter = new TacticalSettings
        {
            preferredDistance = 6.5f,
            retreatDistance = 4f,
            decisionInterval = 0.25f,
            strafeDistance = 4.5f,
            strafeInterval = 1.8f,
            speedMultiplier = 1.2f
        };

        [Header("Navigation")]
        [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 2f;
        [SerializeField, Min(0.1f)] private float distanceTolerance = 0.75f;

        [Header("Runtime debug")]
        [SerializeField] private bool logTacticalChanges;
        [SerializeField] private string currentTactic = "Inactive";

        private EnemyNavigator navigator;
        private EnemyFOV fov;
        private NavMeshAgent agent;
        private NavMeshPath path;

        private float originalSpeed;
        private bool originalUpdateRotation;
        private bool engaged;
        private float nextDecisionTime;
        private float nextManeuverTime;
        private float maneuverUntil;
        private float flankSide = 1f;

        public string CurrentTactic => currentTactic;

        private void Awake()
        {
            navigator = GetComponent<EnemyNavigator>();
            fov = GetComponent<EnemyFOV>();
            agent = GetComponent<NavMeshAgent>();
            path = new NavMeshPath();

            if (loadoutGenerator == null) loadoutGenerator = GetComponent<EnemyLoadoutGenerator>();
            if (shooter == null) shooter = GetComponent<EnemyShooter>();

            if (agent != null)
            {
                originalSpeed = agent.speed;
                originalUpdateRotation = agent.updateRotation;
            }

            flankSide = Random.value < 0.5f ? -1f : 1f;
        }

        private void LateUpdate()
        {
            // Allow NavMeshAgent to face its navigation direction again while investigating/searching.
            if (engaged && (fov == null || !fov.CanSeePlayer)) EndCombat();
        }

        private void OnDisable()
        {
            EndCombat();
        }

        public bool Execute(Transform target, out bool shooting)
        {
            shooting = false;
            if (target == null || shooter == null || navigator == null || agent == null || !agent.isOnNavMesh)
                return false;

            TacticalSettings settings = GetSettings();
            EnemyArchetype archetype = GetArchetype();
            BeginCombat(settings);

            Vector3 toward = target.position - transform.position;
            toward.y = 0f;
            float distance = toward.magnitude;
            if (distance <= 0.01f) return true;

            shooter.FaceTarget(target);

            bool armed = HasEquippedWeapon();
            if (armed && shooter.IsInRange(target))
            {
                shooter.TryShoot(target);
                shooting = true;
            }

            if (Time.time < nextDecisionTime) return true;
            nextDecisionTime = Time.time + Mathf.Max(0.1f, settings.decisionInterval);

            if (!armed)
            {
                SetTactic("Unarmed approach", archetype);
                navigator.SetDestination(target.position);
                return true;
            }

            switch (archetype)
            {
                case EnemyArchetype.Marksman:
                    ExecuteMarksman(target, toward, distance, settings);
                    break;
                case EnemyArchetype.Assault:
                    ExecuteAssault(target, toward, distance, settings);
                    break;
                case EnemyArchetype.Hunter:
                    ExecuteHunter(target, toward, distance, settings);
                    break;
                default:
                    ExecuteMilitant(target, toward, distance, settings);
                    break;
            }

            return true;
        }

        private void ExecuteMilitant(Transform target, Vector3 toward, float distance, TacticalSettings settings)
        {
            if (distance < settings.retreatDistance && TryRetreat(target.position, toward, distance, settings.preferredDistance))
            {
                SetTactic("Militant: short retreat", EnemyArchetype.Militant);
                return;
            }

            if (distance > settings.preferredDistance + distanceTolerance)
            {
                SetTactic("Militant: approach", EnemyArchetype.Militant);
                navigator.SetDestination(target.position);
                return;
            }

            navigator.Stop();
            SetTactic("Militant: hold and fire", EnemyArchetype.Militant);
        }

        private void ExecuteMarksman(Transform target, Vector3 toward, float distance, TacticalSettings settings)
        {
            // Stay near the edge of actual firing range instead of chasing directly into the player.
            float preferred = Mathf.Min(settings.preferredDistance, shooter.ShootRange - 0.7f);
            preferred = Mathf.Max(2f, preferred);
            float retreatAt = Mathf.Min(settings.retreatDistance, preferred - distanceTolerance);

            if (distance < retreatAt)
            {
                if (TryRetreat(target.position, toward, distance, preferred))
                {
                    SetTactic("Marksman: create distance", EnemyArchetype.Marksman);
                    return;
                }

                navigator.Stop();
                SetTactic("Marksman: no retreat route", EnemyArchetype.Marksman);
                return;
            }

            if (distance > preferred + 1.2f)
            {
                Vector3 standoffPoint = target.position - toward.normalized * preferred;

                if (TryNavigate(standoffPoint))
                {
                    SetTactic("Marksman: move to firing range", EnemyArchetype.Marksman);
                    return;
                }
            }

            navigator.Stop();
            SetTactic("Marksman: hold firing position", EnemyArchetype.Marksman);
        }

        private void ExecuteAssault(Transform target, Vector3 toward, float distance, TacticalSettings settings)
        {
            if (distance > settings.preferredDistance + 0.6f)
            {
                maneuverUntil = 0f;
                SetTactic("Assault: rush", EnemyArchetype.Assault);
                navigator.SetDestination(target.position);
                return;
            }

            if (Time.time < maneuverUntil && !navigator.HasReachedDestination) return;

            if (Time.time >= nextManeuverTime)
            {
                nextManeuverTime = Time.time + settings.strafeInterval;
                flankSide *= -1f;

                if (TryOrbit(target.position, toward, settings.preferredDistance, settings.strafeDistance, flankSide))
                {
                    maneuverUntil = Time.time + settings.strafeInterval * 0.9f;
                    SetTactic("Assault: close strafe", EnemyArchetype.Assault);
                    return;
                }
            }

            if (Time.time >= maneuverUntil) navigator.Stop();
            SetTactic("Assault: close-range fire", EnemyArchetype.Assault);
        }

        private void ExecuteHunter(Transform target, Vector3 toward, float distance, TacticalSettings settings)
        {
            if (distance < settings.retreatDistance &&
                TryRetreat(target.position, toward, distance, settings.preferredDistance))
            {
                maneuverUntil = 0f;
                SetTactic("Hunter: break contact", EnemyArchetype.Hunter);
                return;
            }

            // Continue an active flank instead of overwriting its path every decision tick.
            if (Time.time < maneuverUntil && !navigator.HasReachedDestination) return;

            if (Time.time >= nextManeuverTime)
            {
                nextManeuverTime = Time.time + settings.strafeInterval;
                flankSide *= -1f;

                if (TryOrbit(target.position, toward, settings.preferredDistance, settings.strafeDistance, flankSide) ||
                    TryOrbit(target.position, toward, settings.preferredDistance, settings.strafeDistance, -flankSide))
                {
                    maneuverUntil = Time.time + settings.strafeInterval * 0.95f;
                    SetTactic("Hunter: flank", EnemyArchetype.Hunter);
                    return;
                }
            }

            if (distance > settings.preferredDistance + 1.5f)
            {
                Vector3 approach = target.position - toward.normalized * settings.preferredDistance;
                if (TryNavigate(approach))
                {
                    SetTactic("Hunter: reposition", EnemyArchetype.Hunter);
                    return;
                }
            }

            navigator.Stop();
            SetTactic("Hunter: hold angle", EnemyArchetype.Hunter);
        }

        private bool TryRetreat(Vector3 playerPosition, Vector3 toward, float distance, float preferred)
        {
            Vector3 away = -toward.normalized;
            float step = Mathf.Clamp(preferred - distance + 1.5f, 2.5f, 6f);
            float minImprovement = 0.5f;

            float[] angles = { 0f, 40f, -40f, 75f, -75f };
            foreach (float angle in angles)
            {
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * away;
                Vector3 candidate = transform.position + direction * step;
                if (TryNavigate(candidate, playerPosition, distance + minImprovement))
                    return true;
            }

            return false;
        }

        private bool TryOrbit(Vector3 playerPosition, Vector3 toward, float radius, float lateralDistance, float side)
        {
            if (toward.sqrMagnitude < 0.01f) return false;

            Vector3 radial = -toward.normalized;
            Vector3 tangent = Vector3.Cross(Vector3.up, radial) * side;
            Vector3 orbitDirection = (radial + tangent * Mathf.Max(0.1f, lateralDistance / Mathf.Max(radius, 1f))).normalized;
            Vector3 destination = playerPosition + orbitDirection * radius;

            return TryNavigate(destination) ||
                   TryNavigate(playerPosition + (radial - tangent * 0.6f).normalized * radius);
        }

        private bool TryNavigate(Vector3 destination, Vector3 playerPosition = default, float minimumPlayerDistance = 0f)
        {
            if (!agent.isOnNavMesh) return false;
            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                return false;

            if (minimumPlayerDistance > 0f)
            {
                Vector3 fromPlayer = hit.position - playerPosition;
                fromPlayer.y = 0f;
                if (fromPlayer.sqrMagnitude < minimumPlayerDistance * minimumPlayerDistance) return false;
            }

            if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path)) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            return navigator.SetDestination(hit.position);
        }

        private void BeginCombat(TacticalSettings settings)
        {
            if (engaged) return;
            engaged = true;

            agent.speed = originalSpeed * Mathf.Max(0.1f, settings.speedMultiplier);
            agent.updateRotation = false;
            nextDecisionTime = 0f;
            nextManeuverTime = 0f;
            maneuverUntil = 0f;
        }

        private void EndCombat()
        {
            if (!engaged || agent == null) return;
            engaged = false;

            agent.speed = originalSpeed;
            agent.updateRotation = originalUpdateRotation;
            nextDecisionTime = 0f;
            nextManeuverTime = 0f;
            maneuverUntil = 0f;
            SetTactic("Inactive", GetArchetype());
        }

        private bool HasEquippedWeapon()
        {
            CharacterInventory inventory = loadoutGenerator != null ? loadoutGenerator.Inventory : null;
            return inventory != null &&
                   (inventory.Equipment.GetItem(EquipmentSlotType.PrimaryWeapon) != null ||
                    inventory.Equipment.GetItem(EquipmentSlotType.SecondaryWeapon) != null);
        }

        private EnemyArchetype GetArchetype()
        {
            return loadoutGenerator != null && loadoutGenerator.Profile != null
                ? loadoutGenerator.Profile.Archetype
                : EnemyArchetype.Militant;
        }

        private TacticalSettings GetSettings()
        {
            switch (GetArchetype())
            {
                case EnemyArchetype.Marksman: return marksman;
                case EnemyArchetype.Assault: return assault;
                case EnemyArchetype.Hunter: return hunter;
                default: return militant;
            }
        }

        private void SetTactic(string tactic, EnemyArchetype archetype)
        {
            if (currentTactic == tactic) return;
            currentTactic = tactic;
            if (logTacticalChanges) Debug.Log("[AI Tactics] " + name + " (" + archetype + ") -> " + tactic, this);
        }
    }
}
