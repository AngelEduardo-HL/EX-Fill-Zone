using UnityEngine;

namespace ExFillZone.AI.Enemy.Perception
{
    public sealed class EnemyFOV : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform eyes;

        [Header("Field Of View")]
        [SerializeField, Min(0f)] private float viewDistance = 25f;
        [SerializeField, Range(0f, 360f)] private float viewAngle = 110f;
        [SerializeField] private LayerMask visionMask = ~0;

        [Header("Optimization")]
        [SerializeField, Min(0.02f)] private float checkInterval = 0.15f;

        private float nextCheckTime;

        public bool CanSeePlayer { get; private set; }
        public Transform Player => player;

        private void Awake()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }

            if (eyes == null) eyes = transform;

            nextCheckTime = Time.time + Random.Range(0f, checkInterval);
        }

        private void Update()
        {
            if (Time.time < nextCheckTime) return;

            nextCheckTime = Time.time + checkInterval;
            CheckVision();
        }

        private void CheckVision()
        {
            CanSeePlayer = false;
            if (player == null) return;

            Vector3 targetPosition = player.position + Vector3.up * 0.5f;
            Vector3 direction = targetPosition - eyes.position;

            float sqrDistance = direction.sqrMagnitude;
            if (sqrDistance > viewDistance * viewDistance) return;

            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);

            if (flatDirection.sqrMagnitude <= 0.001f || flatForward.sqrMagnitude <= 0.001f) return;

            flatDirection.Normalize();
            flatForward.Normalize();

            float minDot = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);
            if (Vector3.Dot(flatForward, flatDirection) < minDot) return;

            float distance = Mathf.Sqrt(sqrDistance);

            if (!Physics.Raycast(eyes.position, direction.normalized, out RaycastHit hit, distance + 0.2f, visionMask, QueryTriggerInteraction.Ignore)) return;

            Transform hitObject = hit.transform;
            CanSeePlayer = hitObject == player || hitObject.IsChildOf(player);
        }

        private void OnDrawGizmos()
        {
            Transform eyePoint = eyes != null ? eyes : transform;

            Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

            Vector3 leftLimit = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
            Vector3 rightLimit = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;

            Gizmos.DrawLine(eyePoint.position, eyePoint.position + leftLimit * viewDistance);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + rightLimit * viewDistance);

            if (!Application.isPlaying || player == null) return;

            Gizmos.color = CanSeePlayer ? Color.green : Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 0.5f);
        }
    }
}