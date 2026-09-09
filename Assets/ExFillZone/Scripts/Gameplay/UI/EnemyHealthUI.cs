using DG.Tweening;
using ExFillZone.Gameplay.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class EnemyHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Image healthFill;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform healthBar;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float barDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.15f;
        [SerializeField, Min(0f)] private float punchStrength = 0.08f;

        private Tween fillTween;
        private Tween fadeTween;

        private void Awake()
        {
            if (enemyHealth == null) enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (enemyHealth == null) return;

            enemyHealth.HealthChanged += UpdateHealth;
            enemyHealth.Damaged += DamageFeedback;
        }

        private void Start()
        {
            if (enemyHealth == null || healthFill == null || canvasGroup == null)
            {
                Debug.LogError("EnemyHealthUI tiene referencias sin asignar.", this);
                enabled = false;
                return;
            }

            healthFill.fillAmount = enemyHealth.NormalizedHealth;
            canvasGroup.alpha = enemyHealth.NormalizedHealth >= 0.999f ? 0f : 1f;
        }

        private void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0f) return;

            float normalized = currentHealth / maxHealth;

            fillTween?.Kill();
            fadeTween?.Kill();

            fillTween = healthFill.DOFillAmount(normalized, barDuration).SetEase(Ease.OutQuad);
            fadeTween = canvasGroup.DOFade(normalized < 0.999f ? 1f : 0f, fadeDuration);
        }

        private void DamageFeedback()
        {
            if (healthBar == null) return;

            healthBar.DOKill();
            healthBar.localScale = Vector3.one;
            healthBar.DOPunchScale(Vector3.one * punchStrength, 0.15f, 5, 0.5f);
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.HealthChanged -= UpdateHealth;
                enemyHealth.Damaged -= DamageFeedback;
            }

            fillTween?.Kill();
            fadeTween?.Kill();

            if (healthBar != null) healthBar.DOKill();
        }
    }
}