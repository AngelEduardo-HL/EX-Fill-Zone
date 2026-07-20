using ExFillZone.Gameplay.Combat;
using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Camera aimCamera;

        [SerializeField]
        private HitscanWeapon weapon;

        private PlayerInputReader inputReader;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();

            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>();
            }

            if (aimCamera == null || weapon == null)
            {
                Debug.LogError(
                    "Tampoco Jalo bro ocupa el arma y su camara.",
                    this
                );

                enabled = false;
            }
        }

        private void Update()
        {
            if (inputReader.FirePressed)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            Ray aimingRay = aimCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

            weapon.Fire(
                aimingRay.origin,
                aimingRay.direction
            );
        }
    }
}