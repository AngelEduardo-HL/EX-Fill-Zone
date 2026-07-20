using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float movementSpeed = 5f;

        [SerializeField]
        private float gravity = -20f;

        private CharacterController characterController;
        private PlayerInputReader inputReader;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 input = inputReader.MovementInput;

            Vector3 movement =
                transform.right * input.x +
                transform.forward * input.y;

            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 finalVelocity = movement * movementSpeed;
            finalVelocity.y = verticalVelocity;

            characterController.Move(finalVelocity * Time.deltaTime);
        }
    }
}