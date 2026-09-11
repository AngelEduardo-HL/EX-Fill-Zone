using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MovementInput
        {
            get
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                return new Vector2(horizontal, vertical);
            }
        }
        public float CameraOrbitInput
        {
            get
            {
                float input = 0f;

                if (Input.GetKey(KeyCode.Q))
                {
                    input -= 1f;
                }

                if (Input.GetKey(KeyCode.E))
                {
                    input += 1f;
                }

                return input;
            }
        }

        public Vector3 MouseScreenPosition => Input.mousePosition;

        public bool FirePressed => Input.GetMouseButtonDown(0);
        public bool AimHeld => Input.GetMouseButton(1);
        public bool SprintHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public bool InteractPressed => Input.GetKeyDown(KeyCode.F);

    }
}