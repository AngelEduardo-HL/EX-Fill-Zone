using ExFillZone.AI.Director;
using ExFillZone.AI.Enemy.Navigation;
using ExFillZone.AI.Enemy.Perception;
using ExFillZone.AI.Shared.Data;
using ExFillZone.AI.Shared.Enums;
using ExFillZone.AI.Enemy.Combat;

using Panda;
using System;
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

        private EnemyState currentState;

        [Header("Target Memory")]
        [SerializeField, Min(1f)] private float maxTravelTime = 10f;

        [Header("Area Search")]
        [SerializeField, Min(1)] private int maxSearchPoints = 5;
        [SerializeField, Min(0.1f)] private float minInspectTime = 2f;
        [SerializeField, Min(0.1f)] private float maxInspectTime = 4f;
        [SerializeField, Min(0f)] private float lookAroundSpeed = 65f;

        [Header("After Search")]
        [SerializeField, Range(0f, 1f)] private float returnHomeChance = 0.5f;
        [SerializeField, Min(1f)] private float returnHomeTimeout = 25f;

        private PlayerSearchArea searchArea;
        private Vector3 spawnPosition;
        private Vector3 lastKnownPosition;
        private bool hasLastKnownPosition;
        private bool searchingLastKnown;
        private bool wasSeeingPlayer;
        private bool returningHome;
        private bool returnStarted;
        private int searchStage;
        private int completedSearchPoints;
        private float travelDeadline;
        private float inspectUntil;
        private float searchPointAssignedAt;
        private float returnDeadline;
        private float returnStartedAt;
        private float lookDirection = 1f;

        public EnemyState CurrentState
        {
            get => currentState;
            private set
            {
                if (currentState == value) return;

                currentState = value;
                StateChanged?.Invoke(currentState);
            }
        }

        public event Action<EnemyState> StateChanged;

        public float GunshotPriority => gunshotPriority;

        public Vector3 Position => transform.position;

        private void Awake()
        {
            spawnPosition = transform.position;
            navigator = GetComponent<EnemyNavigator>();
            shooter = GetComponent<EnemyShooter>();

            fov = GetComponent<EnemyFOV>();

            if (director == null)
            {
                director = FindFirstObjectByType<AIDirector>();
            }
            CurrentState = EnemyState.Idle;
        }

        private void Update()
        {
            if (fov == null || fov.Player == null) return;

            if (fov.CanSeePlayer)
            {
                lastKnownPosition = fov.Player.position;
                hasLastKnownPosition = true;

                if (searchingLastKnown || returningHome || searchArea != null)
                {
                    director?.LeavePlayerSearch(this, searchArea);
                    searchArea = null;
                    searchingLastKnown = false;
                    returningHome = false;
                    returnStarted = false;
                    searchStage = 0;
                    navigator.Stop();
                }

                wasSeeingPlayer = true;
                return;
            }

            if (!wasSeeingPlayer) return;

            wasSeeingPlayer = false;
            director?.LeavePlayerSearch(this, searchArea);
            searchArea = director?.JoinPlayerSearch(this, lastKnownPosition);
            searchingLastKnown = false;
            completedSearchPoints = 0;
            searchStage = 0;
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
            director?.LeavePlayerSearch(this, searchArea);
            searchArea = null;

            if (director != null) director.UnregisterEnemy(this);
        }

        // =====================================
        // RECIBIR PISTA DEL DIRECTOR
        // =====================================

        public void ReceiveGunshotClue(GunshotClue newClue)
        {
            if (returningHome)
            {
                returningHome = false;
                returnStarted = false;
                navigator.Stop();
            }

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
        private bool HasLastKnownPosition()
        {
            return hasLastKnownPosition && !fov.CanSeePlayer;
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
                CurrentState = EnemyState.Attacking;
                shooter.FaceTarget(fov.Player);
                shooter.TryShoot(fov.Player);
                ThisTask.debugInfo = "Persiguiendo y disparando";
                return;
            }

            ThisTask.debugInfo = "Persiguiendo jugador";
        }

        [Task]
        private void SearchLastKnownPosition()
        {
            if (fov.CanSeePlayer || !hasLastKnownPosition)
            {
                ThisTask.Fail();
                return;
            }

            if (!searchingLastKnown)
            {
                if (searchArea == null)
                    searchArea = director?.JoinPlayerSearch(this, lastKnownPosition);

                if (searchArea == null)
                {
                    hasLastKnownPosition = false;
                    ThisTask.Fail();
                    return;
                }

                searchingLastKnown = true;
                completedSearchPoints = 0;
                searchStage = 0;
            }

            if (completedSearchPoints >= maxSearchPoints)
            {
                FinishVisualSearch();
                ThisTask.Succeed();
                return;
            }

            if (searchStage == 0)
            {
                if (director == null || !director.TryGetPlayerSearchPoint(this, searchArea, out Vector3 point) ||
                    !navigator.SetDestination(point))
                {
                    completedSearchPoints++;
                    ThisTask.debugInfo = "Sin destino navegable";
                    if (completedSearchPoints >= maxSearchPoints)
                    {
                        FinishVisualSearch();
                        ThisTask.Succeed();
                    }
                    return;
                }

                searchPointAssignedAt = Time.time;
                travelDeadline = Time.time + maxTravelTime;
                searchStage = 1;
            }

            if (searchStage == 1)
            {
                CurrentState = EnemyState.MovingToInvestigation;
                ThisTask.debugInfo = $"Buscando punto {completedSearchPoints + 1}/{maxSearchPoints}";

                if (Time.time > searchPointAssignedAt + 0.15f && navigator.HasReachedDestination)
                {
                    navigator.Stop();
                    inspectUntil = Time.time + UnityEngine.Random.Range(minInspectTime, Mathf.Max(minInspectTime, maxInspectTime));
                    lookDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;
                    searchStage = 2;
                }
                else if (Time.time >= travelDeadline)
                {
                    navigator.Stop();
                    completedSearchPoints++;
                    searchStage = 0;
                    if (completedSearchPoints >= maxSearchPoints)
                    {
                        FinishVisualSearch();
                        ThisTask.Succeed();
                    }
                    return;
                }
                else return;
            }

            if (searchStage == 2)
            {
                CurrentState = EnemyState.Investigating;
                ThisTask.debugInfo = $"Inspeccionando {completedSearchPoints + 1}/{maxSearchPoints}";
                transform.Rotate(0f, lookDirection * lookAroundSpeed * Time.deltaTime, 0f);

                if (Time.time < inspectUntil) return;

                completedSearchPoints++;
                searchStage = 0;

                if (completedSearchPoints >= maxSearchPoints)
                {
                    FinishVisualSearch();
                    ThisTask.Succeed();
                }
            }
        }

        private void FinishVisualSearch()
        {
            navigator.Stop();
            director?.LeavePlayerSearch(this, searchArea);
            searchArea = null;
            hasLastKnownPosition = false;
            searchingLastKnown = false;
            searchStage = 0;
            hasClue = false;
            isCheckingArea = false;
            returningHome = UnityEngine.Random.value < returnHomeChance;
            returnStarted = false;
            CurrentState = EnemyState.Idle;
        }

        [Task]
        private bool HasReturnOrder()
        {
            return returningHome && !fov.CanSeePlayer;
        }

        [Task]
        private void ReturnHome()
        {
            if (!returningHome || fov.CanSeePlayer)
            {
                ThisTask.Fail();
                return;
            }

            if (!returnStarted)
            {
                returnStarted = true;
                returnStartedAt = Time.time;
                returnDeadline = Time.time + returnHomeTimeout;

                if (!navigator.SetDestination(spawnPosition))
                {
                    returningHome = false;
                    returnStarted = false;
                    CurrentState = EnemyState.Idle;
                    ThisTask.Fail();
                    return;
                }
            }

            CurrentState = EnemyState.ReturningHome;
            ThisTask.debugInfo = "Regresando al punto inicial";

            if ((Time.time > returnStartedAt + 0.15f && navigator.HasReachedDestination) || Time.time >= returnDeadline)
            {
                navigator.Stop();
                returningHome = false;
                returnStarted = false;
                CurrentState = EnemyState.Idle;
                ThisTask.Succeed();
            }
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