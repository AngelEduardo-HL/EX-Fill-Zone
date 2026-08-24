using ExFillZone.Gameplay.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineFollow))]
    public sealed class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerInputReader input;

        [SerializeField]
        private PlayerAimController playerAim;

        [Header("Camera")]
        [SerializeField]
        private Vector3 baseOffset =
            new Vector3(0f, 14f, -10f);

        [SerializeField]
        private float cameraPitch = 55f;

        [Header("Orbit")]
        [SerializeField, Min(0f)]
        private float orbitSpeed = 90f;

        [SerializeField, Min(0.01f)]
        private float orbitSmoothTime = 0.08f;

        [Header("Mouse Follow")]
        [SerializeField, Min(0f)]
        private float normalMouseDistance = 1.25f;

        [SerializeField, Min(0f)]
        private float aimMouseDistance = 3.5f;

        [SerializeField, Min(0.01f)]
        private float mouseSmoothTime = 0.10f;

        [Header("Zoom")]
        [SerializeField, Range(1f, 179f)]
        private float normalFOV = 55f;

        [SerializeField, Range(1f, 179f)]
        private float aimFOV = 42f;

        [SerializeField, Min(0.01f)]
        private float zoomSmoothTime = 0.10f;

        private CinemachineCamera playerCamera;
        private CinemachineFollow cameraFollow;

        private float targetYaw;
        private float currentYaw;
        private float yawVelocity;

        private Vector3 mouseOffset;
        private Vector3 mouseOffsetVelocity;

        private float currentFOV;
        private float fovVelocity;

        private void Awake()
        {
            playerCamera =
                GetComponent<CinemachineCamera>();

            cameraFollow =
                GetComponent<CinemachineFollow>();

            FindPlayerReferences();

            currentYaw =
                transform.eulerAngles.y;

            targetYaw =
                currentYaw;

            currentFOV =
                playerCamera.Lens.FieldOfView;
        }

        private void Update()
        {
            if (!playerCamera.IsLive)
            {
                return;
            }

            UpdateOrbit();
            UpdateMouseFollow();
            UpdateZoom();
            UpdateCamera();
        }

        private void FindPlayerReferences()
        {
            if (input != null &&
                playerAim != null)
            {
                return;
            }

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError(
                    "PlayerCameraController no encontró al Player.",
                    this
                );

                enabled = false;
                return;
            }

            if (input == null)
            {
                input =
                    player.GetComponent<PlayerInputReader>();
            }

            if (playerAim == null)
            {
                playerAim =
                    player.GetComponent<PlayerAimController>();
            }
        }

        private void UpdateOrbit()
        {
            targetYaw +=
                input.CameraOrbitInput *
                orbitSpeed *
                Time.deltaTime;

            currentYaw =
                Mathf.SmoothDampAngle(
                    currentYaw,
                    targetYaw,
                    ref yawVelocity,
                    orbitSmoothTime
                );
        }

        private void UpdateMouseFollow()
        {
            Vector2 mousePosition =
                input.MouseScreenPosition;

            Vector2 screenCenter =
                new Vector2(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f
                );

            Vector2 screenHalf =
                new Vector2(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f
                );

            Vector2 mouseDirection =
                new Vector2(
                    (mousePosition.x - screenCenter.x) /
                    screenHalf.x,

                    (mousePosition.y - screenCenter.y) /
                    screenHalf.y
                );

            mouseDirection =
                Vector2.ClampMagnitude(
                    mouseDirection,
                    1f
                );

            float distance =
                playerAim.IsAiming
                    ? aimMouseDistance
                    : normalMouseDistance;

            Quaternion cameraRotation =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f
                );

            Vector3 cameraRight =
                cameraRotation *
                Vector3.right;

            Vector3 cameraForward =
                cameraRotation *
                Vector3.forward;

            Vector3 targetOffset =
                (
                    cameraRight *
                    mouseDirection.x
                    +
                    cameraForward *
                    mouseDirection.y
                )
                * distance;

            mouseOffset =
                Vector3.SmoothDamp(
                    mouseOffset,
                    targetOffset,
                    ref mouseOffsetVelocity,
                    mouseSmoothTime
                );
        }

        private void UpdateZoom()
        {
            float targetFOV =
                playerAim.IsAiming
                    ? aimFOV
                    : normalFOV;

            currentFOV =
                Mathf.SmoothDamp(
                    currentFOV,
                    targetFOV,
                    ref fovVelocity,
                    zoomSmoothTime
                );

            LensSettings lens =
                playerCamera.Lens;

            lens.FieldOfView =
                currentFOV;

            playerCamera.Lens =
                lens;
        }

        private void UpdateCamera()
        {
            Quaternion orbitRotation =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f
                );

            Vector3 orbitOffset =
                orbitRotation *
                baseOffset;

            cameraFollow.FollowOffset =
                orbitOffset +
                mouseOffset;

            transform.rotation =
                Quaternion.Euler(
                    cameraPitch,
                    currentYaw,
                    0f
                );
        }
    }
}