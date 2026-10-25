using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryItemUI : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text quantityText;

        private InventoryGridView source;
        private InventoryDragController dragController;

        public ItemInstance Item { get; private set; }
        public InventoryGridView SourceGrid => source;

        public void Setup(ItemInstance item, InventoryGridView grid, InventoryDragController controller)
        {
            Item = item;
            source = grid;
            dragController = controller;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            dragController?.OnItemClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            dragController?.BeginDrag(this, source, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragController?.UpdateDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragController?.EndDrag(this, eventData);
        }
    }
}