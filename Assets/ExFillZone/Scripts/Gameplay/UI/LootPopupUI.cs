using DG.Tweening;
using ExFillZone.Gameplay.Loot;
using TMPro;
using UnityEngine;

namespace ExFillZone.Gameplay.UI
{
    public sealed class LootPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform popupRect;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Position")]
        [SerializeField] private Vector2 popupOffset = new Vector2(80f, 30f);

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float countDuration = 0.45f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.15f;
        [SerializeField, Min(0f)] private float stayDuration = 0.65f;
        [SerializeField, Min(0f)] private float moveUpDistance = 30f;

        private Tween countTween;
        private Sequence popupSequence;

        private void OnEnable()
        {
            RunLoot.MoneyAdded += ShowMoney;
        }

        private void OnDisable()
        {
            RunLoot.MoneyAdded -= ShowMoney;
            countTween?.Kill();
            popupSequence?.Kill();
        }

        private void Start()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void ShowMoney(int amount, int total)
        {
            if (canvasRect == null || popupRect == null || moneyText == null || canvasGroup == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out Vector2 mousePosition);

            popupRect.anchoredPosition = mousePosition + popupOffset;
            popupRect.localScale = Vector3.one * 0.8f;
            canvasGroup.alpha = 0f;

            int displayedMoney = 0;

            countTween?.Kill();
            popupSequence?.Kill();

            countTween = DOTween.To(() => displayedMoney, value =>
            {
                displayedMoney = value;
                moneyText.text = $"+${displayedMoney:N0}";
            }, amount, countDuration).SetEase(Ease.OutCubic);

            popupSequence = DOTween.Sequence();
            popupSequence.Join(canvasGroup.DOFade(1f, fadeDuration));
            popupSequence.Join(popupRect.DOScale(1f, fadeDuration).SetEase(Ease.OutBack));
            popupSequence.AppendInterval(stayDuration);
            popupSequence.Append(popupRect.DOAnchorPosY(popupRect.anchoredPosition.y + moveUpDistance, fadeDuration).SetEase(Ease.InQuad));
            popupSequence.Join(canvasGroup.DOFade(0f, fadeDuration));
        }
    }
}