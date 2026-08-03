using System.Collections.Generic;
using ExFillZone.AI.Enemy;
using ExFillZone.AI.Shared.Data;
using UnityEngine;
using UnityEngine.AI;

namespace ExFillZone.AI.Director
{
    public sealed class AIDirector : MonoBehaviour
    {
        [Header("Investigation Point Generation")]
        [SerializeField, Range(0f, 1f)]
        private float minimumRadiusFactor = 0.25f;

        [SerializeField, Min(0.1f)]
        private float navMeshSampleDistance = 1.5f;

        [SerializeField, Min(1)]
        private int attemptsPerEnemy = 12;

        [SerializeField, Min(0f)]
        private float minimumPointSeparation = 1.25f;

        [Header("Debug")]
        [SerializeField, Min(0f)]
        private float debugDuration = 4f;

        private readonly List<EnemyBrain> registeredEnemies =
            new List<EnemyBrain>();

        private readonly List<Vector3> debugInvestigationPoints =
            new List<Vector3>();

        private Vector3 lastSoundOrigin;
        private float lastHearingRadius;
        private float lastInvestigationRadius;
        private float debugExpirationTime;

        public void RegisterEnemy(EnemyBrain enemy)
        {
            if (enemy == null ||
                registeredEnemies.Contains(enemy))
            {
                return;
            }

            registeredEnemies.Add(enemy);
        }

        public void UnregisterEnemy(EnemyBrain enemy)
        {
            registeredEnemies.Remove(enemy);
        }

        public void ReportGunshot(
            Vector3 soundOrigin,
            float hearingRadius,
            float investigationRadius
        )
        {
            RemoveInvalidEnemies();

            List<EnemyBrain> alertedEnemies =
                FindEnemiesInsideHearingRadius(
                    soundOrigin,
                    hearingRadius
                );

            if (alertedEnemies.Count == 0)
            {
                SaveDebugInformation(
                    soundOrigin,
                    hearingRadius,
                    investigationRadius,
                    new List<Vector3>()
                );

                return;
            }

            Shuffle(alertedEnemies);

            List<Vector3> assignedPoints =
                new List<Vector3>();

            float randomRotation =
                Random.Range(0f, 360f);

            for (
                int index = 0;
                index < alertedEnemies.Count;
                index++
            )
            {
                EnemyBrain enemy =
                    alertedEnemies[index];

                if (!TryGenerateInvestigationPoint(
                    soundOrigin,
                    investigationRadius,
                    index,
                    alertedEnemies.Count,
                    randomRotation,
                    assignedPoints,
                    out Vector3 investigationPoint
                ))
                {
                    Debug.LogWarning(
                        $"No se encontró un punto de " +
                        $"investigación válido para {enemy.name}.",
                        enemy
                    );

                    continue;
                }

                assignedPoints.Add(
                    investigationPoint
                );

                GunshotClue clue =
                    new GunshotClue(
                        soundOrigin,
                        investigationPoint,
                        Time.time,
                        enemy.GunshotPriority
                    );

                enemy.ReceiveGunshotClue(clue);
            }

            SaveDebugInformation(
                soundOrigin,
                hearingRadius,
                investigationRadius,
                assignedPoints
            );
        }

        private List<EnemyBrain>
            FindEnemiesInsideHearingRadius(
                Vector3 soundOrigin,
                float hearingRadius
            )
        {
            List<EnemyBrain> result =
                new List<EnemyBrain>();

            float squaredRadius =
                hearingRadius * hearingRadius;

            foreach (EnemyBrain enemy in registeredEnemies)
            {
                Vector3 difference =
                    enemy.Position - soundOrigin;

                if (difference.sqrMagnitude <= squaredRadius)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }

        private bool TryGenerateInvestigationPoint(
            Vector3 center,
            float radius,
            int enemyIndex,
            int enemyCount,
            float randomRotation,
            List<Vector3> assignedPoints,
            out Vector3 result
        )
        {
            float sectorSize =
                360f / Mathf.Max(1, enemyCount);

            float sectorCenter =
                randomRotation +
                sectorSize * enemyIndex;

            float maximumAngleVariation =
                sectorSize * 0.35f;

            float minimumRadius =
                radius * minimumRadiusFactor;

            for (
                int attempt = 0;
                attempt < attemptsPerEnemy;
                attempt++
            )
            {
                float angle =
                    sectorCenter +
                    Random.Range(
                        -maximumAngleVariation,
                        maximumAngleVariation
                    );

                float distance =
                    Random.Range(
                        minimumRadius,
                        radius
                    );

                Vector3 direction =
                    Quaternion.Euler(
                        0f,
                        angle,
                        0f
                    ) * Vector3.forward;

                Vector3 candidate =
                    center +
                    direction * distance;

                if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleDistance,
                    NavMesh.AllAreas
                ))
                {
                    continue;
                }

                if (!IsSeparatedFromOtherPoints(
                    hit.position,
                    assignedPoints
                ))
                {
                    continue;
                }

                result = hit.position;
                return true;
            }

            result = default;
            return false;
        }

        private bool IsSeparatedFromOtherPoints(
            Vector3 candidate,
            List<Vector3> assignedPoints
        )
        {
            float squaredMinimumDistance =
                minimumPointSeparation *
                minimumPointSeparation;

            foreach (Vector3 assignedPoint in assignedPoints)
            {
                if (
                    (candidate - assignedPoint).sqrMagnitude <
                    squaredMinimumDistance
                )
                {
                    return false;
                }
            }

            return true;
        }

        private void RemoveInvalidEnemies()
        {
            registeredEnemies.RemoveAll(
                enemy => enemy == null
            );
        }

        private static void Shuffle(
            List<EnemyBrain> enemies
        )
        {
            for (
                int index = enemies.Count - 1;
                index > 0;
                index--
            )
            {
                int randomIndex =
                    Random.Range(0, index + 1);

                (
                    enemies[index],
                    enemies[randomIndex]
                ) =
                (
                    enemies[randomIndex],
                    enemies[index]
                );
            }
        }

        private void SaveDebugInformation(
            Vector3 soundOrigin,
            float hearingRadius,
            float investigationRadius,
            List<Vector3> assignedPoints
        )
        {
            lastSoundOrigin = soundOrigin;
            lastHearingRadius = hearingRadius;
            lastInvestigationRadius =
                investigationRadius;

            debugExpirationTime =
                Time.time + debugDuration;

            debugInvestigationPoints.Clear();
            debugInvestigationPoints.AddRange(
                assignedPoints
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying ||
                Time.time > debugExpirationTime)
            {
                return;
            }

            // Zona amplia que escuchan los NPC.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                lastSoundOrigin,
                lastHearingRadius
            );

            // Zona menor donde investigan.
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                lastSoundOrigin,
                lastInvestigationRadius
            );

            Gizmos.color = Color.magenta;

            foreach (
                Vector3 point in debugInvestigationPoints
            )
            {
                Gizmos.DrawSphere(point, 0.3f);

                Gizmos.DrawLine(
                    lastSoundOrigin,
                    point
                );
            }
        }
    }
}