using ExFillZone.AI.Shared.Stimuli;
using ExFillZone.Gameplay.Combat;
using Unity.Cinemachine;
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
        private ProjectileWeapon weapon;

        [SerializeField]
        private Transform muzzle;

        [SerializeField]
        private GunshotEmitter gunshotEmitter;

        private PlayerInputReader inputReader;
        private PlayerAimController aimController;
        private PlayerAmmo ammo;

        [Header("Camera Feedback")]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField, Min(0f)] private float shakeForce = 0.15f;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();

            aimController = GetComponent<PlayerAimController>();

            ammo = GetComponent<PlayerAmmo>();

            if (weapon == null)
            {
                weapon = GetComponentInChildren<ProjectileWeapon>();
            }

            if (impulseSource == null)
            {
                impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
            }

            if (muzzle == null && weapon != null)
            {
                muzzle = weapon.transform;
            }

            if (gunshotEmitter == null)
            {
                gunshotEmitter = GetComponentInChildren<GunshotEmitter>();
            }

            if (weapon == null || muzzle == null || gunshotEmitter == null)
            {
                Debug.LogError("PlayerShooter necesita Weapon, Muzzle y GunshotEmitter.", this);
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

            // Disparo físico.
            weapon.Fire(origin, direction);
            // Emitir estímulo de disparo.
            gunshotEmitter?.Emit(muzzle.position);
            // Agitar la cámara.
            impulseSource?.GenerateImpulseWithForce(shakeForce);
        }
    }
}