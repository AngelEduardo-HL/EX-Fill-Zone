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

        public Vector3 MouseScreenPosition => Input.mousePosition;

        public bool FirePressed => Input.GetMouseButtonDown(0);

        public bool AimHeld => Input.GetMouseButton(1);
    }
}