using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerAimController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Camera aimCamera;

        [SerializeField]
        private Transform aimOrigin;

        [Header("Aim Configuration")]
        [SerializeField, Min(0f)]
        private float aimingRotationSpeed = 720f;

        [SerializeField, Min(0f)]
        private float movementRotationSpeed = 540f;

        [SerializeField, Min(1f)]
        private float maximumAimDistance = 100f;

        private PlayerInputReader inputReader;
        private PlayerMotor playerMotor;

        public Vector3 AimPoint { get; private set; }

        public Vector3 AimDirection { get; private set; }

        public bool IsAiming =>
            inputReader != null &&
            inputReader.AimHeld;

        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            playerMotor =
                GetComponent<PlayerMotor>();

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            if (aimOrigin == null)
            {
                aimOrigin = transform;
            }

            if (aimCamera == null)
            {
                Debug.LogError(
                    "PlayerAimController necesita una cámara.",
                    this
                );

                enabled = false;
                return;
            }

            AimDirection = transform.forward;

            AimPoint =
                aimOrigin.position +
                AimDirection * maximumAimDistance;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            UpdateAimPoint();
            UpdatePlayerRotation();
        }

        private void UpdateAimPoint()
        {
            Ray mouseRay =
                aimCamera.ScreenPointToRay(
                    inputReader.MouseScreenPosition
                );

            Plane aimPlane =
                new Plane(
                    Vector3.up,
                    new Vector3(
                        0f,
                        aimOrigin.position.y,
                        0f
                    )
                );

            if (!aimPlane.Raycast(
                mouseRay,
                out float distance
            ))
            {
                return;
            }

            Vector3 mouseWorldPoint =
                mouseRay.GetPoint(distance);

            Vector3 planarDirection =
                mouseWorldPoint -
                aimOrigin.position;

            planarDirection.y = 0f;

            if (planarDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            planarDirection =
                Vector3.ClampMagnitude(
                    planarDirection,
                    maximumAimDistance
                );

            AimDirection =
                planarDirection.normalized;

            AimPoint =
                aimOrigin.position +
                planarDirection;
        }

        private void UpdatePlayerRotation()
        {
            if (IsAiming)
            {
                RotateTowards(
                    AimDirection,
                    aimingRotationSpeed
                );

                return;
            }

            if (playerMotor.IsMoving)
            {
                RotateTowards(
                    playerMotor.MovementDirection,
                    movementRotationSpeed
                );
            }
        }

        private void RotateTowards(
            Vector3 direction,
            float rotationSpeed
        )
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.DrawLine(
                aimOrigin.position,
                AimPoint
            );

            Gizmos.DrawSphere(
                AimPoint,
                0.15f
            );
        }
    }
}