using UnityEngine;
using UnityEngine.AI;

namespace ExFillZone.AI.Enemy.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyNavigator : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float arrivalTolerance = 0.2f;

        private NavMeshAgent agent;
        private bool navMeshWarningShown;

        public bool HasReachedDestination
        {
            get
            {
                if (!agent.isOnNavMesh ||
                    agent.pathPending ||
                    float.IsInfinity(agent.remainingDistance))
                {
                    return false;
                }

                float requiredDistance =
                    agent.stoppingDistance +
                    arrivalTolerance;

                if (agent.remainingDistance >
                    requiredDistance)
                {
                    return false;
                }

                return !agent.hasPath ||
                       agent.velocity.sqrMagnitude < 0.01f;
            }
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public bool SetDestination(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                if (!navMeshWarningShown)
                {
                    Debug.LogWarning(
                        $"{name} no está colocado sobre un NavMesh.",
                        this
                    );

                    navMeshWarningShown = true;
                }

                return false;
            }

            agent.isStopped = false;

            return agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!agent.isOnNavMesh)
            {
                return;
            }

            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}