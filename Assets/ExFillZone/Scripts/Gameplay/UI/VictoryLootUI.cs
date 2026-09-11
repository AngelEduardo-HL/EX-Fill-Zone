using DG.Tweening;
using ExFillZone.Gameplay.Loot;
using TMPro;
using UnityEngine;

namespace ExFillZone.Gameplay.UI
{
    public sealed class VictoryLootUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;
        [SerializeField, Min(0.01f)] private float countDuration = 1.2f;

        private Tween countTween;

        private void Start()
        {
            if (moneyText == null) return;

            int displayedMoney = 0;
            int totalMoney = RunLoot.TotalMoney;

            moneyText.text = "MONEY EXTRACTED\n$0";

            countTween = DOTween.To(() => displayedMoney, value =>
            {
                displayedMoney = value;
                moneyText.text = $"MONEY EXTRACTED\n${displayedMoney:N0}";
            }, totalMoney, countDuration).SetEase(Ease.OutCubic);
        }

        private void OnDisable()
        {
            countTween?.Kill();
        }
    }
}