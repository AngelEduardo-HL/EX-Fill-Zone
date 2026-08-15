using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    public sealed class PlayerAmmo : MonoBehaviour
    {
        [Header("Ammo")]
        [SerializeField, Min(0)]
        private int currentAmmo = 10;

        [SerializeField, Min(1)]
        private int maxAmmo = 30;

        public int CurrentAmmo => currentAmmo;

        public int MaxAmmo => maxAmmo;

        public bool HasAmmo => currentAmmo > 0;

        public bool UseAmmo()
        {
            if (!HasAmmo)
            {
                return false;
            }

            currentAmmo--;

            Debug.Log(
                $"Munición: {currentAmmo}/{maxAmmo}"
            );

            return true;
        }

        public void AddAmmo(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentAmmo = Mathf.Min(
                currentAmmo + amount,
                maxAmmo
            );

            Debug.Log(
                $"Munición recogida. {currentAmmo}/{maxAmmo}"
            );
        }
    }
}