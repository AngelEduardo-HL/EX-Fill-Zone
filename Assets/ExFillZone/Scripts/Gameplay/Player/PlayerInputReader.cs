using ExFillZone.Gameplay.Inventory;
using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MovementInput
        {
            get
            {
                if (InventoryMenuController.IsOpen) return Vector2.zero;

                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                return new Vector2(horizontal, vertical);
            }
        }
        public float CameraOrbitInput
        {
            get
            {
                if (InventoryMenuController.IsOpen) return 0f;

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

        public bool FirePressed => !InventoryMenuController.IsOpen && Input.GetMouseButtonDown(0);
        public bool AimHeld => !InventoryMenuController.IsOpen && Input.GetMouseButton(1);
        public bool SprintHeld => !InventoryMenuController.IsOpen && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        public bool InteractPressed => !InventoryMenuController.IsOpen && Input.GetKeyDown(KeyCode.F);

    }
}