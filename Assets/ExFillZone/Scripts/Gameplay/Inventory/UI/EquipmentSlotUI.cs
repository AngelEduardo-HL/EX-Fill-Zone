using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class EquipmentSlotUI : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text durabilityText;

        public ItemInstance Item { get; private set; }

        public void ShowItem(ItemInstance item)
        {
            Item = item;
            bool hasItem = item?.Definition != null;

            if (itemIcon != null)
            {
                itemIcon.sprite = hasItem ? item.Definition.Icon : null;
                itemIcon.enabled = itemIcon.sprite != null;
            }

            if (itemName != null)
                itemName.text = hasItem ? item.Definition.DisplayName : "";

            if (durabilityText != null)
            {
                bool hasDurability = hasItem && item.Definition.MaxDurability > 0f;

                durabilityText.text = hasDurability
                    ? $"{item.Durability:0}/{item.Definition.MaxDurability:0}"
                    : "";
            }
        }
    }
}