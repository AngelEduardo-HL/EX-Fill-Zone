using ExFillZone.AI.Director;
using ExFillZone.AI.Enemy.Navigation;
using ExFillZone.AI.Shared.Data;
using ExFillZone.AI.Shared.Enums;
using UnityEngine;

namespace ExFillZone.AI.Enemy
{
    [RequireComponent(typeof(EnemyNavigator))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private AIDirector director;

        [Header("Goal Priorities")]
        [SerializeField, Min(0f)]
        private float idlePriority = 5f;

        [SerializeField, Min(0f)]
        private float gunshotPriority = 60f;

        [Header("Investigation")]
        [SerializeField, Min(0f)]
        private float investigationDuration = 2.5f;

        private EnemyNavigator navigator;
        private GunshotClue currentClue;

        private float currentPriority;
        private float investigationTimeRemaining;
        private bool hasClue;

        public EnemyState CurrentState { get; private set; }

        public float GunshotPriority => gunshotPriority;

        public Vector3 Position => transform.position;

        private void Awake()
        {
            navigator = GetComponent<EnemyNavigator>();

            if (director == null)
            {
                director =
                    FindFirstObjectByType<AIDirector>();
            }

            currentPriority = idlePriority;
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

        private void Update()
        {
            switch (CurrentState)
            {
                case EnemyState.MovingToInvestigation:
                    UpdateMovementToInvestigation();
                    break;

                case EnemyState.Investigating:
                    UpdateInvestigation();
                    break;
            }
        }

        public void ReceiveGunshotClue(
            GunshotClue clue
        )
        {
            /*
             * Permitimos reemplazar una pista por otra
             * de igual prioridad cuando es más reciente.
             */
            if (clue.Priority < currentPriority)
            {
                return;
            }

            if (!navigator.SetDestination(
                clue.InvestigationPoint
            ))
            {
                return;
            }

            currentClue = clue;
            currentPriority = clue.Priority;
            hasClue = true;

            CurrentState =
                EnemyState.MovingToInvestigation;
        }

        private void UpdateMovementToInvestigation()
        {
            if (!navigator.HasReachedDestination)
            {
                return;
            }

            navigator.Stop();

            investigationTimeRemaining =
                investigationDuration;

            CurrentState =
                EnemyState.Investigating;
        }

        private void UpdateInvestigation()
        {
            investigationTimeRemaining -=
                Time.deltaTime;

            if (investigationTimeRemaining > 0f)
            {
                return;
            }

            FinishInvestigation();
        }

        private void FinishInvestigation()
        {
            navigator.Stop();

            hasClue = false;
            currentPriority = idlePriority;
            CurrentState = EnemyState.Idle;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !hasClue)
            {
                return;
            }

            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                transform.position,
                currentClue.InvestigationPoint
            );

            Gizmos.DrawSphere(
                currentClue.InvestigationPoint,
                0.25f
            );
        }
    }
}