using ExFillZone.Gameplay.Combat;
using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerAimController))]
    [RequireComponent(typeof(PlayerAmmo))]
    public sealed class PlayerShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private HitscanWeapon weapon;

        [SerializeField]
        private Transform muzzle;

        private PlayerInputReader inputReader;
        private PlayerAimController aimController;
        private PlayerAmmo ammo;

        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            aimController =
                GetComponent<PlayerAimController>();

            ammo =
                GetComponent<PlayerAmmo>();

            if (weapon == null)
            {
                weapon =
                    GetComponentInChildren<HitscanWeapon>();
            }

            if (muzzle == null && weapon != null)
            {
                muzzle = weapon.transform;
            }

            if (weapon == null || muzzle == null)
            {
                Debug.LogError(
                    "PlayerShooter necesita un arma y un Muzzle.",
                    this
                );

                enabled = false;
            }
        }

        private void Update()
        {
            if (!aimController.IsAiming)
            {
                return;
            }

            if (inputReader.FirePressed)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if (!ammo.UseAmmo())
            {
                Debug.Log("Sin munición.");
                return;
            }

            Vector3 direction =
                aimController.AimDirection;

            Vector3 origin =
                muzzle.position +
                direction * 0.05f;

            weapon.Fire(
                origin,
                direction
            );
        }
    }
}