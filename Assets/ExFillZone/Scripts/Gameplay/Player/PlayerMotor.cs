using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform movementReference;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float movementSpeed = 5f;

        [SerializeField]
        private float gravity = -20f;

        private CharacterController characterController;
        private PlayerInputReader inputReader;

        private float verticalVelocity;

        public Vector3 MovementDirection { get; private set; }

        public bool IsMoving =>
            MovementDirection.sqrMagnitude > 0.001f;

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            inputReader =
                GetComponent<PlayerInputReader>();

            if (movementReference == null &&
                Camera.main != null)
            {
                movementReference =
                    Camera.main.transform;
            }

            if (movementReference == null)
            {
                Debug.LogError(
                    "PlayerMotor necesita una referencia de cámara.",
                    this
                );

                enabled = false;
            }
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 input =
                inputReader.MovementInput;

            Vector3 cameraForward =
                movementReference.forward;

            Vector3 cameraRight =
                movementReference.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            MovementDirection =
                cameraRight * input.x +
                cameraForward * input.y;

            if (MovementDirection.sqrMagnitude > 1f)
            {
                MovementDirection =
                    MovementDirection.normalized;
            }

            if (characterController.isGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity +=
                gravity * Time.deltaTime;

            Vector3 finalVelocity =
                MovementDirection * movementSpeed;

            finalVelocity.y = verticalVelocity;

            characterController.Move(
                finalVelocity * Time.deltaTime
            );
        }
    }
}