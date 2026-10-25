using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class EquipmentSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Visual References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text durabilityText;

        private InventoryDragController dragController;

        public ItemInstance Item { get; private set; }
        public EquipmentSlotType SlotType { get; private set; }

        public void ConfigureDrag(InventoryDragController controller, EquipmentSlotType slot)
        {
            dragController = controller;
            SlotType = slot;
        }

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
                    ? $"{Mathf.RoundToInt(item.Durability / item.Definition.MaxDurability * 100f)}%"
                    : "";
            }
        }

        public bool ContainsScreenPoint(Vector2 position, Camera camera)
        {
            return isActiveAndEnabled &&
                   RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, position, camera);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || Item == null) return;
            dragController?.BeginDragFromSlot(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragController?.UpdateDragFromSlot(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragController?.EndDragFromSlot(this, eventData);
        }
    }
}