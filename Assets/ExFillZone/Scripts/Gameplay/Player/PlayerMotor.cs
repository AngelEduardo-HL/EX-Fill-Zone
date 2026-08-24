using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStamina))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform movementReference;

        [Header("Movement Speed")]
        [SerializeField, Min(0f)]
        private float walkSpeed = 5f;

        [SerializeField, Min(0f)]
        private float sprintSpeed = 8.5f;

        [Header("Acceleration")]
        [SerializeField, Min(0f)]
        private float walkAcceleration = 40f;

        [SerializeField, Min(0f)]
        private float sprintAcceleration = 14f;

        [Header("Deceleration")]
        [SerializeField, Min(0f)]
        private float walkDeceleration = 60f;

        [SerializeField, Min(0f)]
        private float sprintDeceleration = 16f;

        [Header("Sprint Rules")]
        [SerializeField]
        private bool canSprintWhileAiming = false;

        [Header("Gravity")]
        [SerializeField]
        private float gravity = -20f;

        private CharacterController characterController;
        private PlayerInputReader inputReader;
        private PlayerStamina playerStamina;

        private Vector3 currentMoveDirection;
        private float currentMovementSpeed;
        private float verticalVelocity;

        public Vector3 MovementDirection { get; private set; }

        public Vector3 HorizontalVelocity =>
            currentMoveDirection * currentMovementSpeed;

        public float CurrentSpeed =>
            currentMovementSpeed;

        public bool IsMoving =>
            MovementDirection.sqrMagnitude > 0.001f;

        public bool IsPhysicallyMoving =>
            currentMovementSpeed > 0.01f;

        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            inputReader =
                GetComponent<PlayerInputReader>();

            playerStamina =
                GetComponent<PlayerStamina>();

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
                Vector2.ClampMagnitude(
                    inputReader.MovementInput,
                    1f
                );

            CalculateMovementDirection(input);

            bool hasMovementInput =
                MovementDirection.sqrMagnitude > 0.001f;

            bool wasSprinting =
                IsSprinting;

            bool sprintRequested =
                inputReader.SprintHeld &&
                hasMovementInput &&
                CanSprintInCurrentState();

            IsSprinting =
                sprintRequested &&
                playerStamina.CanSprint;

            UpdateMovementSpeed(
                hasMovementInput,
                wasSprinting
            );

            UpdateMovementDirection(
                hasMovementInput
            );

            ApplyGravity();

            Vector3 horizontalVelocity =
                currentMoveDirection *
                currentMovementSpeed;

            Vector3 finalVelocity =
                horizontalVelocity;

            finalVelocity.y =
                verticalVelocity;

            characterController.Move(finalVelocity * Time.deltaTime);

            playerStamina.Tick(
                IsSprinting,
                inputReader.SprintHeld,
                Time.deltaTime
            );
        }

        private void CalculateMovementDirection(Vector2 input)
        {
            float cameraYaw =
                movementReference.eulerAngles.y;

            Quaternion horizontalRotation =
                Quaternion.Euler(
                    0f,
                    cameraYaw,
                    0f
                );

            Vector3 cameraForward =
                horizontalRotation *
                Vector3.forward;

            Vector3 cameraRight =
                horizontalRotation *
                Vector3.right;

            Vector3 desiredDirection =
                cameraRight * input.x +
                cameraForward * input.y;

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                desiredDirection.Normalize();
            }

            MovementDirection =
                desiredDirection;
        }

        private void UpdateMovementSpeed(bool hasMovementInput, bool wasSprinting)
        {
            if (!hasMovementInput)
            {
                bool stoppingFromSprint =
                    wasSprinting ||
                    currentMovementSpeed >
                    walkSpeed + 0.1f;

                float stoppingRate =
                    stoppingFromSprint
                        ? sprintDeceleration
                        : walkDeceleration;

                currentMovementSpeed =
                    Mathf.MoveTowards(
                        currentMovementSpeed,
                        0f,
                        stoppingRate *
                        Time.deltaTime
                    );

                return;
            }

            float targetSpeed =
                IsSprinting
                    ? sprintSpeed
                    : walkSpeed;

            float accelerationRate;

            if (currentMovementSpeed >
                targetSpeed)
            {
                accelerationRate =
                    sprintDeceleration;
            }
            else
            {
                accelerationRate =
                    IsSprinting
                        ? sprintAcceleration
                        : walkAcceleration;
            }

            currentMovementSpeed =
                Mathf.MoveTowards(
                    currentMovementSpeed,
                    targetSpeed,
                    accelerationRate *
                    Time.deltaTime
                );
        }

        private void UpdateMovementDirection(bool hasMovementInput)
        {
            if (!hasMovementInput)
            {
                return;
            }

            currentMoveDirection =
                MovementDirection;
        }

        private bool CanSprintInCurrentState()
        {
            if (canSprintWhileAiming)
            {
                return true;
            }

            return !inputReader.AimHeld;
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity +=
                gravity * Time.deltaTime;
        }
    }
}