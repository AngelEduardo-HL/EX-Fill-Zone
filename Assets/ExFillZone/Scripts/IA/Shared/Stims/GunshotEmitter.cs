using ExFillZone.AI.Director;
using UnityEngine;

namespace ExFillZone.AI.Shared.Stimuli
{
    public sealed class GunshotEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private AIDirector director;

        [Header("Gunshot Areas")]
        [SerializeField, Min(0.1f)]
        private float hearingRadius = 18f;

        [SerializeField, Min(0.1f)]
        private float investigationRadius = 5f;

        private void Awake()
        {
            if (director == null)
            {
                director =
                    FindFirstObjectByType<AIDirector>();
            }

            if (director == null)
            {
                Debug.LogError(
                    "GunshotEmitter no encontró un AIDirector.",
                    this
                );
            }
        }

        public void Emit(Vector3 position)
        {
            if (director == null)
            {
                return;
            }

            director.ReportGunshot(
                position,
                hearingRadius,
                investigationRadius
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                transform.position,
                hearingRadius
            );

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                transform.position,
                investigationRadius
            );
        }
    }
}