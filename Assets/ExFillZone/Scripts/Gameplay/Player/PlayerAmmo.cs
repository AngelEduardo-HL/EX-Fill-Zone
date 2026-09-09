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

        public int AddAmmo(int amount)
        {
            if (amount <= 0) return 0;

            int previousAmmo = currentAmmo;
            currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);

            int addedAmmo = currentAmmo - previousAmmo;

            if (addedAmmo > 0) Debug.Log($"Munición: {currentAmmo}/{maxAmmo}");

            return addedAmmo;
        }
    }
}