using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.Gameplay.Items
{
    public sealed class AmmoPickup : MonoBehaviour
    {
        [SerializeField, Min(1)]
        private int ammoAmount = 5;

        private bool pickedUp;

        private void OnTriggerEnter(Collider other)
        {
            if (pickedUp)
            {
                return;
            }

            PlayerAmmo playerAmmo =
                other.GetComponentInParent<PlayerAmmo>();

            if (playerAmmo == null)
            {
                return;
            }

            pickedUp = true;

            playerAmmo.AddAmmo(ammoAmount);

            Destroy(gameObject);
        }
    }
}