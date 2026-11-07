
using TMPro;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class PlayerLootValueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterInventory inventory;
        [SerializeField] private TMP_Text lootValueText;

        [Header("Update")]
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.15f;

        private int lastValue = -1;
        private float nextRefresh;

        private void Start()
        {
            if (inventory == null || lootValueText == null)
            {
                Debug.LogError("[LootValueUI] Faltan referencias.", this);
                enabled = false;
                return;
            }

            RefreshValue();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;

            nextRefresh = Time.unscaledTime + refreshInterval;
            RefreshValue();
        }

        private void RefreshValue()
        {
            int value = inventory.TotalValue;
            if (value == lastValue) return;

            lastValue = value;
            lootValueText.text = $"${value:N0}";
        }
    }
}
