using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.Gameplay.Items
{
    public sealed class AmmoPickup : MonoBehaviour
    {
        [SerializeField, Min(1)] private int ammoAmount = 5;

        private bool pickedUp;

        private void OnTriggerEnter(Collider other)
        {
            if (pickedUp) return;

            PlayerAmmo playerAmmo = other.GetComponentInParent<PlayerAmmo>();
            if (playerAmmo == null) return;

            int addedAmmo = playerAmmo.AddAmmo(ammoAmount);

            if (addedAmmo <= 0)
            {
                Debug.Log("Munición llena.");
                return;
            }

            pickedUp = true;
            Destroy(gameObject);
        }
    }
}