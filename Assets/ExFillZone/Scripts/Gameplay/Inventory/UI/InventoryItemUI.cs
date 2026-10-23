using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text quantityText;

        public ItemInstance Item { get; private set; }

        public void Setup(ItemInstance item)
        {
            Item = item;
            if (item?.Definition == null) return;

            if (icon != null)
            {
                icon.sprite = item.Definition.Icon;
                icon.enabled = icon.sprite != null;
            }

            if (itemName != null)
                itemName.text = item.Definition.DisplayName;

            if (quantityText != null)
                quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
        }
    }
}