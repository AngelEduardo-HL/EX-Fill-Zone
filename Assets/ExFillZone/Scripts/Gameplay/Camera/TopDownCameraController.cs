using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class TopDownCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform target;

        [SerializeField]
        private PlayerAimController playerAim;

        [Header("Fixed Camera")]
        [SerializeField]
        private Vector3 followOffset = new Vector3(0f, 14f, -10f);

        [SerializeField]
        private Vector3 fixedRotation = new Vector3(55f, 0f, 0f);

        [SerializeField, Min(0.01f)]
        private float positionSmoothTime = 0.12f;

        [Header("Aim Focus")]
        [SerializeField, Min(0f)]
        private float aimOffsetDistance = 3.5f;

        [Header("Perspective Zoom")]
        [SerializeField, Range(1f, 179f)]
        private float normalFieldOfView = 55f;

        [SerializeField, Range(1f, 179f)]
        private float aimingFieldOfView = 42f;

        [Header("Orthographic Zoom")]
        [SerializeField, Min(0.1f)]
        private float normalOrthographicSize = 8f;

        [SerializeField, Min(0.1f)]
        private float aimingOrthographicSize = 6f;

        [Header("Zoom Smoothing")]
        [SerializeField, Min(0.01f)]
        private float zoomSmoothTime = 0.1f;

        private Camera controlledCamera;

        private Vector3 positionVelocity;
        private float zoomVelocity;

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();

            if (target == null || playerAim == null)
            {
                Debug.LogError(
                    "TopDownCameraController necesita Target y Player Aim.",
                    this
                );

                enabled = false;
            }
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            FollowTarget();
            UpdateZoom();

            transform.rotation =
                Quaternion.Euler(fixedRotation);
        }

        private void FollowTarget()
        {
            Vector3 aimOffset = Vector3.zero;

            if (playerAim.IsAiming)
            {
                aimOffset =
                    playerAim.AimDirection *
                    aimOffsetDistance;
            }

            Vector3 desiredPosition =
                target.position +
                followOffset +
                aimOffset;

            transform.position =
                Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref positionVelocity,
                    positionSmoothTime
                );
        }

        private void UpdateZoom()
        {
            if (controlledCamera.orthographic)
            {
                float targetSize =
                    playerAim.IsAiming
                        ? aimingOrthographicSize
                        : normalOrthographicSize;

                controlledCamera.orthographicSize =
                    Mathf.SmoothDamp(
                        controlledCamera.orthographicSize,
                        targetSize,
                        ref zoomVelocity,
                        zoomSmoothTime
                    );

                return;
            }

            float targetFieldOfView =
                playerAim.IsAiming
                    ? aimingFieldOfView
                    : normalFieldOfView;

            controlledCamera.fieldOfView =
                Mathf.SmoothDamp(
                    controlledCamera.fieldOfView,
                    targetFieldOfView,
                    ref zoomVelocity,
                    zoomSmoothTime
                );
        }

        private void SnapToTarget()
        {
            transform.position =
                target.position + followOffset;

            transform.rotation =
                Quaternion.Euler(fixedRotation);

            if (controlledCamera.orthographic)
            {
                controlledCamera.orthographicSize =
                    normalOrthographicSize;
            }
            else
            {
                controlledCamera.fieldOfView =
                    normalFieldOfView;
            }
        }
    }
}