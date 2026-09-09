using DG.Tweening;
using ExFillZone.AI.Enemy;
using ExFillZone.AI.Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class EnemyStateUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyBrain enemyBrain;
        [SerializeField] private Image stateIcon;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Sprites")]
        [SerializeField] private Sprite investigationSprite;
        [SerializeField] private Sprite detectionSprite;
        [SerializeField] private Sprite attackSprite;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float animationDuration = 0.15f;
        [SerializeField, Range(0.1f, 1f)] private float hiddenScale = 0.7f;

        private void Awake()
        {
            if (enemyBrain == null) enemyBrain = GetComponentInParent<EnemyBrain>();
        }

        private void OnEnable()
        {
            if (enemyBrain != null) enemyBrain.StateChanged += UpdateState;
        }

        private void Start()
        {
            if (enemyBrain == null || stateIcon == null || canvasGroup == null)
            {
                Debug.LogError("EnemyStateUI tiene referencias sin asignar.", this);
                enabled = false;
                return;
            }

            UpdateState(enemyBrain.CurrentState, true);
        }

        private void UpdateState(EnemyState state)
        {
            UpdateState(state, false);
        }

        private void UpdateState(EnemyState state, bool instant)
        {
            Sprite sprite = GetSprite(state);

            if (sprite == null)
            {
                Hide(instant);
                return;
            }

            stateIcon.sprite = sprite;
            Show(instant);
        }

        private Sprite GetSprite(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.MovingToInvestigation:
                case EnemyState.Investigating:
                    return investigationSprite;

                case EnemyState.Chasing:
                    return detectionSprite;

                case EnemyState.Attacking:
                    return attackSprite;

                default:
                    return null;
            }
        }

        private void Show(bool instant)
        {
            transform.DOKill();
            canvasGroup.DOKill();

            if (instant)
            {
                transform.localScale = Vector3.one;
                canvasGroup.alpha = 1f;
                return;
            }

            transform.localScale = Vector3.one * hiddenScale;
            canvasGroup.alpha = 0f;

            transform.DOScale(1f, animationDuration).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1f, animationDuration);
        }

        private void Hide(bool instant)
        {
            transform.DOKill();
            canvasGroup.DOKill();

            if (instant)
            {
                transform.localScale = Vector3.one * hiddenScale;
                canvasGroup.alpha = 0f;
                return;
            }

            transform.DOScale(hiddenScale, animationDuration).SetEase(Ease.InQuad);
            canvasGroup.DOFade(0f, animationDuration);
        }

        private void OnDisable()
        {
            if (enemyBrain != null) enemyBrain.StateChanged -= UpdateState;

            transform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
        }
    }
}