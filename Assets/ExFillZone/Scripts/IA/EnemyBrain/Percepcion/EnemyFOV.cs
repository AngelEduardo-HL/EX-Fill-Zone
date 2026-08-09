using UnityEngine;

namespace ExFillZone.AI.Enemy.Perception
{
    public sealed class EnemyFOV : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform player;

        [SerializeField]
        private Transform eyes;

        [Header("Field Of View")]
        [SerializeField, Min(0f)]
        private float viewDistance = 12f;

        [SerializeField, Range(0f, 360f)]
        private float viewAngle = 100f;

        [SerializeField]
        private LayerMask visionMask = ~0;

        public bool CanSeePlayer { get; private set; }

        public Transform Player => player;

        private void Awake()
        {
            if (player == null)
            {
                GameObject playerObject =
                    GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }

            if (eyes == null)
            {
                eyes = transform;
            }
        }

        private void Update()
        {
            CheckVision();
        }

        private void CheckVision()
        {
            CanSeePlayer = false;

            if (player == null)
            {
                return;
            }

            Vector3 targetPosition =
                player.position + Vector3.up * 0.5f;

            Vector3 direction =
                targetPosition - eyes.position;

            float distance = direction.magnitude;

            //Distancia
            if (distance > viewDistance)
            {
                return;
            }

            //Angulo horizontal
            Vector3 flatDirection =
                new Vector3(
                    direction.x,
                    0f,
                    direction.z
                );

            Vector3 flatForward =
                new Vector3(
                    transform.forward.x,
                    0f,
                    transform.forward.z
                );

            float angle =
                Vector3.Angle(
                    flatForward,
                    flatDirection
                );

            if (angle > viewAngle * 0.5f)
            {
                return;
            }

            //Línea de vision
            Ray ray =
                new Ray(
                    eyes.position,
                    direction.normalized
                );

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                distance + 0.2f,
                visionMask,
                QueryTriggerInteraction.Ignore
            ))
            {
                Transform hitObject =
                    hit.transform;

                if (hitObject == player ||
                    hitObject.IsChildOf(player))
                {
                    CanSeePlayer = true;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Transform eyePoint =
                eyes != null
                    ? eyes
                    : transform;

            Gizmos.color = Color.yellow;

            //Gizmos.DrawWireSphere(
            //    eyePoint.position,
            //    viewDistance
            //);

            Vector3 leftLimit =
                Quaternion.Euler(
                    0f,
                    -viewAngle * 0.5f,
                    0f
                ) * transform.forward;

            Vector3 rightLimit =
                Quaternion.Euler(
                    0f,
                    viewAngle * 0.5f,
                    0f
                ) * transform.forward;

            Gizmos.DrawLine(
                eyePoint.position,
                eyePoint.position +
                leftLimit * viewDistance
            );

            Gizmos.DrawLine(
                eyePoint.position,
                eyePoint.position +
                rightLimit * viewDistance
            );

            if (!Application.isPlaying ||
                player == null)
            {
                return;
            }

            Gizmos.color =
                CanSeePlayer
                    ? Color.green
                    : Color.red;

            Gizmos.DrawLine(
                eyePoint.position,
                player.position +
                Vector3.up * 0.5f
            );
        }
    }
}