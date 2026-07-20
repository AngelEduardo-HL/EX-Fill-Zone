using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform cameraPivot;

        [Header("Configuration")]
        [SerializeField, Min(0f)]
        private float mouseSensitivity = 2f;

        [SerializeField]
        private float minimumPitch = -85f;

        [SerializeField]
        private float maximumPitch = 85f;

        private PlayerInputReader inputReader;
        private float currentPitch;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();

            if (cameraPivot == null)
            {
                Debug.LogError(
                    "No jalo bro Checa eso",
                    this
                );

                enabled = false;
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            RotateView();
        }

        private void RotateView()
        {
            Vector2 lookInput = inputReader.LookInput;

            float yaw = lookInput.x * mouseSensitivity;
            float pitchChange = lookInput.y * mouseSensitivity;

            currentPitch -= pitchChange;
            currentPitch = Mathf.Clamp(
                currentPitch,
                minimumPitch,
                maximumPitch
            );

            cameraPivot.localRotation =
                Quaternion.Euler(currentPitch, 0f, 0f);

            transform.Rotate(Vector3.up * yaw);
        }
    }
}