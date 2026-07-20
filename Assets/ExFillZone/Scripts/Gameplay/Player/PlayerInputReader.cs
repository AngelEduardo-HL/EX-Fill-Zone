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

        public Vector2 LookInput
        {
            get
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                return new Vector2(mouseX, mouseY);
            }
        }

        public bool FirePressed => Input.GetButtonDown("Fire1");
    }
}