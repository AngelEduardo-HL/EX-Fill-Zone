using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        [Header("Grid Views")]
        [SerializeField] private InventoryGridView handsView;
        [SerializeField] private InventoryGridView rigView;
        [SerializeField] private InventoryGridView backpackView;
        [SerializeField] private InventoryGridView groundView;

        [Header("Equipment")]
        [SerializeField] private PlayerInventoryPanelsUI playerPanels;
        [SerializeField] private PlayerEquipmentPanelUI equipmentPanel;


        [Header("Drag Preview")]
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private InventoryItemUI itemPrefab;


        private CharacterInventory character;

        private InventoryItemUI sourceItemUI;
        private InventoryGridView sourceGrid;
        private EquipmentSlotUI sourceSlot;

        private ItemInstance movingItem;
        private InventoryItemUI preview;
        private RectTransform previewRect;
        private Image previewBackground;

        private CanvasGroup sourceCanvasGroup;
        private float originalAlpha;

        private Vector2 pointerPosition;
        private Camera pointerCamera;
        private bool rotated;

        public void Bind(CharacterInventory inventory)
        {
            character = inventory;
        }

        private void Update()
        {
            if (movingItem == null || !Input.GetKeyDown(KeyCode.R)) return;

            rotated = !rotated;
            RefreshPreview();
        }

        public void BeginDrag(InventoryItemUI visual, InventoryGridView origin, PointerEventData eventData)
        {
            if (visual?.Item == null || origin?.Grid == null || movingItem != null) return;

            InventoryPlacement placement = null;

            foreach (InventoryPlacement current in origin.Grid.Placements)
            {
                if (!ReferenceEquals(current.Item, visual.Item)) continue;
                placement = current;
                break;
            }

            if (placement == null || !CreatePreview(visual.Item, visual.gameObject, eventData)) return;

            sourceItemUI = visual;
            sourceGrid = origin;
            sourceSlot = null;
            rotated = placement.Rotated;

            RefreshPreview();
        }

        public void BeginDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (slot?.Item == null || character == null || movingItem != null) return;
            if (!ReferenceEquals(character.Equipment.GetItem(slot.SlotType), slot.Item)) return;
            if (!CreatePreview(slot.Item, slot.gameObject, eventData)) return;

            sourceItemUI = null;
            sourceGrid = null;
            sourceSlot = slot;
            rotated = false;

            RefreshPreview();
        }

        private bool CreatePreview(ItemInstance item, GameObject sourceObject, PointerEventData eventData)
        {
            if (item?.Definition == null || dragLayer == null || itemPrefab == null) return false;

            movingItem = item;
            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;

            preview = Instantiate(itemPrefab, dragLayer, false);
            preview.Setup(item, null, this);

            previewRect = preview.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);
            previewRect.localScale = Vector3.one;
            previewRect.SetAsLastSibling();

            CanvasGroup previewGroup = preview.GetComponent<CanvasGroup>();
            if (previewGroup == null) previewGroup = preview.gameObject.AddComponent<CanvasGroup>();

            previewGroup.blocksRaycasts = false;
            previewGroup.interactable = false;
            previewGroup.alpha = 0.85f;

            previewBackground = preview.GetComponent<Image>();

            sourceCanvasGroup = sourceObject.GetComponent<CanvasGroup>();
            if (sourceCanvasGroup == null) sourceCanvasGroup = sourceObject.AddComponent<CanvasGroup>();

            originalAlpha = sourceCanvasGroup.alpha;
            sourceCanvasGroup.alpha = 0.3f;

            return true;
        }

        public void UpdateDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceItemUI, visual)) return;
            UpdatePointer(eventData);
        }

        public void UpdateDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceSlot, slot)) return;
            UpdatePointer(eventData);
        }

        private void UpdatePointer(PointerEventData eventData)
        {
            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;
            RefreshPreview();
        }

        public void EndDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceItemUI, visual)) return;
            CompleteDrag(eventData);
        }

        public void EndDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceSlot, slot)) return;
            CompleteDrag(eventData);
        }

        private void CompleteDrag(PointerEventData eventData)
        {
            UpdatePointer(eventData);

            bool success = false;
            EquipmentSlotUI targetSlot = equipmentPanel != null
                ? equipmentPanel.GetSlotAt(pointerPosition, pointerCamera)
                : null;

            if (targetSlot != null)
            {
                if (CanDropOnSlot(targetSlot))
                    success = character.Equipment.TryEquip(sourceGrid.Grid, movingItem, targetSlot.SlotType);
            }
            else
            {
                InventoryGridView targetGrid = GetHoveredGrid(out Vector2Int position);

                if (targetGrid != null && CanDropOnGrid(targetGrid, position))
                {
                    if (sourceGrid != null)
                    {
                        success = ReferenceEquals(sourceGrid.Grid, targetGrid.Grid)
                            ? sourceGrid.Grid.TryMove(movingItem, position, rotated)
                            : InventoryTransfer.TryTransfer(sourceGrid.Grid, targetGrid.Grid, movingItem, position, rotated);
                    }
                    else if (sourceSlot != null)
                    {
                        success = character.Equipment.TryUnequip(sourceSlot.SlotType, targetGrid.Grid, position, rotated);
                    }
                }
            }

            FinishDrag(success);
        }

        private bool CanDropOnSlot(EquipmentSlotUI slot)
        {
            if (sourceGrid == null || character == null || movingItem == null || slot.Item != null)
                return false;

            return sourceGrid.Grid.Contains(movingItem) &&
                   movingItem.Quantity == 1 &&
                   character.Equipment.IsValidForSlot(movingItem.Definition, slot.SlotType);
        }

        private bool CanDropOnGrid(InventoryGridView grid, Vector2Int position)
        {
            if (grid?.Grid == null || movingItem == null) return false;

            if (sourceGrid != null && ReferenceEquals(sourceGrid.Grid, grid.Grid))
                return grid.Grid.CanMove(movingItem, position, rotated);

            if (ReferenceEquals(movingItem.Contents, grid.Grid) ||
                !grid.Grid.CanPlace(movingItem, position, rotated))
                return false;

            if (sourceGrid != null)
                return sourceGrid.Grid.Contains(movingItem);

            return sourceSlot != null && character != null &&
                   ReferenceEquals(character.Equipment.GetItem(sourceSlot.SlotType), movingItem);
        }

        private void RefreshPreview()
        {
            if (movingItem == null || previewRect == null) return;

            EquipmentSlotUI targetSlot = equipmentPanel != null
                ? equipmentPanel.GetSlotAt(pointerPosition, pointerCamera)
                : null;

            bool valid;

            if (targetSlot != null)
            {
                previewRect.sizeDelta = ((RectTransform)targetSlot.transform).rect.size;
                valid = CanDropOnSlot(targetSlot);
            }
            else
            {
                InventoryGridView targetGrid = GetHoveredGrid(out Vector2Int position);

                if (targetGrid != null)
                {
                    previewRect.sizeDelta = targetGrid.GetItemVisualSize(movingItem, rotated);
                    valid = CanDropOnGrid(targetGrid, position);
                }
                else
                {
                    previewRect.sizeDelta = sourceGrid != null
                        ? sourceGrid.GetItemVisualSize(movingItem, rotated)
                        : ((RectTransform)sourceSlot.transform).rect.size;

                    valid = false;
                }
            }

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragLayer, pointerPosition, pointerCamera, out Vector3 worldPosition))
            {
                previewRect.position = worldPosition;
            }

            if (previewBackground != null)
            {
                previewBackground.color = valid
                    ? new Color(0.2f, 0.8f, 0.4f, 0.9f)
                    : new Color(0.9f, 0.25f, 0.25f, 0.9f);
            }
        }

        private InventoryGridView GetHoveredGrid(out Vector2Int position)
        {
            if (movingItem != null)
            {
                if (handsView != null && handsView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return handsView;
                if (rigView != null && rigView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return rigView;
                if (backpackView != null && backpackView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return backpackView;
                if (groundView != null && groundView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return groundView;
            }

            position = Vector2Int.zero;
            return null;
        }

        private void FinishDrag(bool success)
        {
            string itemName = movingItem?.Definition?.DisplayName ?? "Item";

            if (sourceCanvasGroup != null)
                sourceCanvasGroup.alpha = originalAlpha;

            if (preview != null)
                Destroy(preview.gameObject);

            movingItem = null;
            sourceItemUI = null;
            sourceGrid = null;
            sourceSlot = null;
            preview = null;
            previewRect = null;
            previewBackground = null;
            sourceCanvasGroup = null;

            playerPanels?.Refresh();
            groundView?.Refresh();
            equipmentPanel?.Refresh();

            Debug.Log($"[Drag] {itemName}: {(success ? "Moved" : "Cancelled")}", this);
        }
    }
}