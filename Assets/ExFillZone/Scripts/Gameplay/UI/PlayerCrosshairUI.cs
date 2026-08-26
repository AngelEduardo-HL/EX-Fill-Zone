using DG.Tweening;
using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerCrosshairUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerInputReader input;

        [SerializeField]
        private PlayerAimController playerAim;

        [SerializeField]
        private RectTransform canvasRect;

        [Header("Crosshair")]
        [SerializeField]
        private RectTransform normalCrosshair;

        [SerializeField]
        private CanvasGroup normalCrosshairGroup;

        [SerializeField]
        private RectTransform aimCrosshair;

        [SerializeField]
        private CanvasGroup aimCrosshairGroup;

        [Header("Aim Overlay")]
        [SerializeField]
        private CanvasGroup aimOverlay;

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float crosshairDuration = 0.12f;

        [SerializeField, Min(0.01f)]
        private float overlayDuration = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float overlayAlpha = 1f;

        [SerializeField, Range(0.1f, 1f)]
        private float hiddenScale = 0.75f;

        private bool lastAimState;

        private void Start()
        {
            FindPlayer();

            if (input == null ||
                playerAim == null ||
                canvasRect == null ||
                normalCrosshair == null ||
                normalCrosshairGroup == null ||
                aimCrosshair == null ||
                aimCrosshairGroup == null ||
                aimOverlay == null)
            {
                Debug.LogError(
                    "PlayerCrosshairUI tiene referencias sin asignar.",
                    this
                );

                enabled = false;
                return;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;

            lastAimState =
                playerAim.IsAiming;

            SetInitialState();
        }

        private void Update()
        {
            UpdateCrosshairPosition();

            bool aiming =
                playerAim.IsAiming;

            if (aiming != lastAimState)
            {
                lastAimState = aiming;

                UpdateAimState(
                    aiming
                );
            }
        }

        private void FindPlayer()
        {
            if (input != null &&
                playerAim != null)
            {
                return;
            }

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                return;
            }

            if (input == null)
            {
                input =
                    player.GetComponent<PlayerInputReader>();
            }

            if (playerAim == null)
            {
                playerAim =
                    player.GetComponent<PlayerAimController>();
            }
        }

        private void UpdateCrosshairPosition()
        {
            Vector2 localPosition;

            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    input.MouseScreenPosition,
                    null,
                    out localPosition
                );

            normalCrosshair.anchoredPosition =
                localPosition;

            aimCrosshair.anchoredPosition =
                localPosition;
        }

        private void SetInitialState()
        {
            normalCrosshair.DOKill();
            aimCrosshair.DOKill();

            normalCrosshairGroup.DOKill();
            aimCrosshairGroup.DOKill();
            aimOverlay.DOKill();

            if (playerAim.IsAiming)
            {
                normalCrosshairGroup.alpha = 0f;
                normalCrosshair.localScale =
                    Vector3.one * hiddenScale;

                aimCrosshairGroup.alpha = 1f;
                aimCrosshair.localScale =
                    Vector3.one;

                aimOverlay.alpha =
                    overlayAlpha;
            }
            else
            {
                normalCrosshairGroup.alpha = 1f;
                normalCrosshair.localScale =
                    Vector3.one;

                aimCrosshairGroup.alpha = 0f;
                aimCrosshair.localScale =
                    Vector3.one * hiddenScale;

                aimOverlay.alpha = 0f;
            }
        }

        private void UpdateAimState(
            bool aiming
        )
        {
            normalCrosshair.DOKill();
            aimCrosshair.DOKill();

            normalCrosshairGroup.DOKill();
            aimCrosshairGroup.DOKill();
            aimOverlay.DOKill();

            if (aiming)
            {
                normalCrosshairGroup
                    .DOFade(
                        0f,
                        crosshairDuration
                    );

                normalCrosshair
                    .DOScale(
                        hiddenScale,
                        crosshairDuration
                    )
                    .SetEase(Ease.OutQuad);

                aimCrosshairGroup
                    .DOFade(
                        1f,
                        crosshairDuration
                    );

                aimCrosshair
                    .DOScale(
                        1f,
                        crosshairDuration
                    )
                    .SetEase(Ease.OutBack);

                aimOverlay
                    .DOFade(
                        overlayAlpha,
                        overlayDuration
                    )
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                aimCrosshairGroup
                    .DOFade(
                        0f,
                        crosshairDuration
                    );

                aimCrosshair
                    .DOScale(
                        hiddenScale,
                        crosshairDuration
                    )
                    .SetEase(Ease.OutQuad);

                normalCrosshairGroup
                    .DOFade(
                        1f,
                        crosshairDuration
                    );

                normalCrosshair
                    .DOScale(
                        1f,
                        crosshairDuration
                    )
                    .SetEase(Ease.OutBack);

                aimOverlay
                    .DOFade(
                        0f,
                        overlayDuration
                    )
                    .SetEase(Ease.OutQuad);
            }
        }

        private void OnDisable()
        {
            if (normalCrosshair != null)
            {
                normalCrosshair.DOKill();
            }

            if (aimCrosshair != null)
            {
                aimCrosshair.DOKill();
            }

            if (normalCrosshairGroup != null)
            {
                normalCrosshairGroup.DOKill();
            }

            if (aimCrosshairGroup != null)
            {
                aimCrosshairGroup.DOKill();
            }

            if (aimOverlay != null)
            {
                aimOverlay.DOKill();
            }

            Cursor.visible = true;
        }
    }
}