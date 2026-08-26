using DG.Tweening;
using ExFillZone.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerHealth playerHealth;

        [SerializeField]
        private Image healthFill;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float barDuration = 0.20f;

        [SerializeField, Min(0.01f)]
        private float fadeDuration = 0.20f;

        [SerializeField, Range(0f, 1f)]
        private float fullHealthThreshold = 0.999f;

        private Tween barTween;
        private Tween fadeTween;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth =
                    GetComponentInParent<PlayerHealth>();
            }

            if (canvasGroup == null)
            {
                canvasGroup =
                    GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            if (playerHealth == null ||
                healthFill == null ||
                canvasGroup == null)
            {
                Debug.LogError(
                    "PlayerHealthUI tiene referencias sin asignar.",
                    this
                );

                enabled = false;
                return;
            }

            healthFill.fillAmount =
                playerHealth.NormalizedHealth;

            if (playerHealth.NormalizedHealth >=
                fullHealthThreshold)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged +=
                    UpdateHealth;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -=
                    UpdateHealth;
            }

            barTween?.Kill();
            fadeTween?.Kill();
        }

        private void UpdateHealth(
            float currentHealth,
            float maxHealth
        )
        {
            if (maxHealth <= 0f)
            {
                return;
            }

            float normalized =
                currentHealth / maxHealth;

            barTween?.Kill();

            barTween =
                healthFill
                    .DOFillAmount(
                        normalized,
                        barDuration
                    )
                    .SetEase(Ease.OutQuad);

            if (normalized <
                fullHealthThreshold)
            {
                ShowBar();
            }
            else
            {
                HideBar();
            }
        }

        private void ShowBar()
        {
            fadeTween?.Kill();

            fadeTween =
                canvasGroup
                    .DOFade(
                        1f,
                        fadeDuration
                    )
                    .SetEase(Ease.OutQuad);
        }

        private void HideBar()
        {
            fadeTween?.Kill();

            fadeTween =
                canvasGroup
                    .DOFade(
                        0f,
                        fadeDuration
                    )
                    .SetEase(Ease.OutQuad);
        }
    }
}