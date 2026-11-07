
using DG.Tweening;
using ExFillZone.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerBackpackUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterInventory inventory;
        [SerializeField] private Image backpackFill;

        [Header("Weight")]
        [SerializeField, Min(1f)] private float maxWeight = 40f;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.15f;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float fillDuration = 0.20f;

        private float lastWeight = -1f;
        private float nextRefresh;
        private Tween fillTween;

        private void Start()
        {
            if (inventory == null || backpackFill == null)
            {
                Debug.LogError("[PlayerBackpackUI] Faltan referencias.", this);
                enabled = false;
                return;
            }

            lastWeight = inventory.TotalWeight;
            backpackFill.fillAmount = GetFillAmount();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;

            nextRefresh = Time.unscaledTime + refreshInterval;

            float weight = inventory.TotalWeight;
            if (Mathf.Abs(weight - lastWeight) < 0.001f) return;

            lastWeight = weight;
            UpdateBackpack();
        }

        private float GetFillAmount()
        {
            return Mathf.Clamp01(inventory.TotalWeight / maxWeight);
        }

        private void UpdateBackpack()
        {
            fillTween?.Kill();
            fillTween = backpackFill.DOFillAmount(GetFillAmount(), fillDuration).SetEase(Ease.OutQuad);
        }

        private void OnDisable()
        {
            fillTween?.Kill();
        }
    }
}
