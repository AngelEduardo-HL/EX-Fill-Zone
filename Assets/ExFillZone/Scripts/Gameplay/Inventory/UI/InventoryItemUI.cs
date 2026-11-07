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

        public void SetIconRotation(bool rotated)
        {
            if (icon == null) return;

            RectTransform iconRect = icon.rectTransform;
            RectTransform itemRect = (RectTransform)transform;

            float padding = 4f;
            float topMargin = 18f;
            float bottomMargin = 4f;

            float width = Mathf.Max(1f, itemRect.rect.width - padding * 2f);
            float height = Mathf.Max(1f, itemRect.rect.height - topMargin - bottomMargin);

            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);

            iconRect.anchoredPosition = new Vector2(0f, (bottomMargin - topMargin) * 0.5f);

            iconRect.sizeDelta = rotated
                ? new Vector2(height, width)
                : new Vector2(width, height);

            iconRect.localRotation = Quaternion.Euler(0f, 0f, rotated ? -90f : 0f);
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