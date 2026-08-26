using DG.Tweening;
using ExFillZone.Gameplay.Player;
using TMPro;
using UnityEngine;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerAmmoUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerAmmo playerAmmo;

        [SerializeField]
        private TMP_Text ammoText;

        [SerializeField]
        private RectTransform ammoPanel;

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float animationDuration = 0.12f;

        [SerializeField, Min(1f)]
        private float animationScale = 1.12f;

        private int lastAmmo = -1;

        private void Start()
        {
            if (playerAmmo == null)
            {
                GameObject player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    playerAmmo =
                        player.GetComponent<PlayerAmmo>();
                }
            }

            if (ammoPanel == null)
            {
                ammoPanel =
                    transform as RectTransform;
            }

            if (playerAmmo == null ||
                ammoText == null)
            {
                Debug.LogError(
                    "PlayerAmmoUI tiene referencias sin asignar.",
                    this
                );

                enabled = false;
                return;
            }

            UpdateAmmoText();

            lastAmmo =
                playerAmmo.CurrentAmmo;
        }

        private void Update()
        {
            if (playerAmmo.CurrentAmmo ==
                lastAmmo)
            {
                return;
            }

            lastAmmo =
                playerAmmo.CurrentAmmo;

            UpdateAmmoText();

            AnimateAmmo();
        }

        private void UpdateAmmoText()
        {
            ammoText.text =
                $"{playerAmmo.CurrentAmmo} / " +
                $"{playerAmmo.MaxAmmo}";
        }

        private void AnimateAmmo()
        {
            if (ammoPanel == null)
            {
                return;
            }

            ammoPanel.DOKill();

            ammoPanel.localScale =
                Vector3.one;

            ammoPanel
                .DOScale(
                    animationScale,
                    animationDuration
                )
                .SetLoops(
                    2,
                    LoopType.Yoyo
                )
                .SetEase(Ease.OutQuad);
        }

        private void OnDisable()
        {
            if (ammoPanel != null)
            {
                ammoPanel.DOKill();
            }
        }
    }
}