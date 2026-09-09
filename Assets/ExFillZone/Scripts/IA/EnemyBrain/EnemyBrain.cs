using ExFillZone.AI.Director;
using ExFillZone.AI.Enemy.Navigation;
using ExFillZone.AI.Enemy.Perception;
using ExFillZone.AI.Shared.Data;
using ExFillZone.AI.Shared.Enums;
using ExFillZone.AI.Enemy.Combat;

using Panda;
using UnityEngine;

namespace ExFillZone.AI.Enemy
{
    [RequireComponent(typeof(EnemyNavigator))]
    [RequireComponent(typeof(EnemyFOV))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private AIDirector director;

        [Header("Gunshot")]
        [SerializeField, Min(0f)]
        private float gunshotPriority = 60f;

        [SerializeField, Min(0f)]
        private float investigationTime = 2.5f;

        private EnemyNavigator navigator;
        private EnemyFOV fov;

        private EnemyShooter shooter;

        private GunshotClue clue;

        private bool hasClue;
        private bool isCheckingArea;

        private float checkTimer;

        public EnemyState CurrentState
        {
            get;
            private set;
        }

        public float GunshotPriority => gunshotPriority;

        public Vector3 Position => transform.position;

        private void Awake()
        {
            navigator = GetComponent<EnemyNavigator>();
            shooter = GetComponent<EnemyShooter>();

            fov = GetComponent<EnemyFOV>();

            if (director == null)
            {
                director = FindFirstObjectByType<AIDirector>();
            }
            CurrentState = EnemyState.Idle;
        }

        private void OnEnable()
        {
            if (director != null)
            {
                director.RegisterEnemy(this);
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.UnregisterEnemy(this);
            }
        }

        // =====================================
        // RECIBIR PISTA DEL DIRECTOR
        // =====================================

        public void ReceiveGunshotClue(GunshotClue newClue)
        {
            clue = newClue;
            hasClue = true;
            
            isCheckingArea = false;
            checkTimer = 0f;
        }

        // =====================================
        // PANDA - CONDICIONES
        // =====================================

        [Task]
        private bool CanSeePlayer()
        {
            return fov.CanSeePlayer;
        }

        [Task]
        private bool InStopRange()
        {
            return shooter != null && shooter.IsInStopRange(fov.Player);
        }

        [Task]
        private bool HasClue()
        {
            return hasClue;
        }

        // =====================================
        // PANDA - PERSEGUIR
        // =====================================

        [Task]
        private void ShootPlayer()
        {
            if (!fov.CanSeePlayer || fov.Player == null || shooter == null || !shooter.IsInStopRange(fov.Player))
            {
                ThisTask.Fail();
                return;
            }

            CurrentState = EnemyState.Attacking;

            navigator.Stop();
            shooter.FaceTarget(fov.Player);
            shooter.TryShoot(fov.Player);

            ThisTask.debugInfo = "Disparando detenido";
        }

        [Task]
        private void ChasePlayer()
        {
            if (!fov.CanSeePlayer || fov.Player == null)
            {
                ThisTask.Fail();
                return;
            }

            if (shooter != null && shooter.IsInStopRange(fov.Player))
            {
                navigator.Stop();
                ThisTask.Succeed();
                return;
            }

            CurrentState = EnemyState.Chasing;
            navigator.SetDestination(fov.Player.position);

            if (shooter != null && shooter.IsInRange(fov.Player))
            {
                shooter.FaceTarget(fov.Player);
                shooter.TryShoot(fov.Player);
                ThisTask.debugInfo = "Persiguiendo y disparando";
                return;
            }

            ThisTask.debugInfo = "Persiguiendo jugador";
        }

        // =====================================
        // PANDA - INVESTIGAR
        // =====================================

        [Task]
        private void Investigate()
        {
            if (fov.CanSeePlayer)
            {
                ThisTask.Fail();
                return;
            }

            if (!hasClue)
            {
                ThisTask.Fail();
                return;
            }

            if (!isCheckingArea)
            {
                CurrentState = EnemyState.MovingToInvestigation;

                navigator.SetDestination(clue.InvestigationPoint);

                if (!navigator.HasReachedDestination)
                {
                    ThisTask.debugInfo = "Yendo a investigar";
                    return;
                }

                navigator.Stop();

                CurrentState = EnemyState.Investigating;

                isCheckingArea = true;
                checkTimer = investigationTime;
            }

            checkTimer -= Time.deltaTime;

            ThisTask.debugInfo = "Investigando";

            if (checkTimer > 0f)
            {
                return;
            }

            hasClue = false;
            isCheckingArea = false;

            navigator.Stop();

            CurrentState = EnemyState.Idle;

            ThisTask.Succeed();
        }

        // =====================================
        // PANDA - IDLE
        // =====================================

        [Task]
        private void Idle()
        {
            navigator.Stop();

            CurrentState =
                EnemyState.Idle;

            ThisTask.debugInfo =
                "Esperando";

            ThisTask.Succeed();
        }

        // =====================================
        // DEBUG
        // =====================================

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !hasClue)
            {
                return;
            }

            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(transform.position, clue.InvestigationPoint);

            Gizmos.DrawSphere(clue.InvestigationPoint, 0.25f);
        }
    }
}