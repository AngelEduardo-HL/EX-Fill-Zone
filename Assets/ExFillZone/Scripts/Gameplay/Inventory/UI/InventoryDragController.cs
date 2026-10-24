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

        [Header("Other UI")]
        [SerializeField] private PlayerInventoryPanelsUI playerPanels;
        [SerializeField] private PlayerEquipmentPanelUI equipmentPanel;
        [SerializeField] private RectTransform dragLayer;

        private InventoryItemUI moving;
        private InventoryGridView source;
        private RectTransform movingRect;
        private Image background;
        private Vector2Int grabOffset;
        private bool rotated;
        private Vector2 pointerPosition;
        private Camera pointerCamera;

        private void Update()
        {
            if (moving == null || !Input.GetKeyDown(KeyCode.R)) return;

            rotated = !rotated;
            RefreshPreview();
        }

        public void BeginDrag(InventoryItemUI visual, InventoryGridView origin, PointerEventData eventData)
        {
            if (moving != null || visual?.Item == null || origin?.Grid == null || dragLayer == null)
                return;

            InventoryPlacement placement = null;

            foreach (InventoryPlacement current in origin.Grid.Placements)
            {
                if (!ReferenceEquals(current.Item, visual.Item)) continue;

                placement = current;
                break;
            }

            if (placement == null) return;

            moving = visual;
            source = origin;
            rotated = placement.Rotated;
            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;

            grabOffset = Vector2Int.zero;

            if (origin.TryGetCell(eventData.position, pointerCamera, out Vector2Int clicked))
                grabOffset = clicked - placement.Position;

            movingRect = visual.GetComponent<RectTransform>();
            movingRect.SetParent(dragLayer, false);
            movingRect.SetAsLastSibling();
            movingRect.anchorMin = new Vector2(0.5f, 0.5f);
            movingRect.anchorMax = new Vector2(0.5f, 0.5f);
            movingRect.pivot = new Vector2(0f, 1f);
            movingRect.localScale = Vector3.one;

            CanvasGroup group = visual.GetComponent<CanvasGroup>();
            if (group == null) group = visual.gameObject.AddComponent<CanvasGroup>();

            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 0.85f;

            background = visual.GetComponent<Image>();
            RefreshPreview();
        }

        public void UpdateDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(moving, visual)) return;

            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;
            RefreshPreview();
        }

        public void EndDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(moving, visual)) return;

            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;

            bool success = false;
            InventoryGridView target = GetHoveredGrid(out Vector2Int hoveredCell);

            if (target != null)
            {
                Vector2Int position = GetPlacementPosition(hoveredCell);

                success = ReferenceEquals(source.Grid, target.Grid)
                    ? source.Grid.TryMove(moving.Item, position, rotated)
                    : InventoryTransfer.TryTransfer(source.Grid, target.Grid, moving.Item, position, rotated);
            }

            string itemName = moving.Item.Definition.DisplayName;

            Destroy(moving.gameObject);
            moving = null;
            movingRect = null;
            background = null;
            source = null;

            playerPanels?.Refresh();
            groundView?.Refresh();
            equipmentPanel?.Refresh();

            Debug.Log($"[Drag] {itemName}: {(success ? "Moved" : "Cancelled")}", this);
        }

        private void RefreshPreview()
        {
            if (moving == null || movingRect == null) return;

            InventoryGridView target = GetHoveredGrid(out Vector2Int hoveredCell);

            if (target == null)
            {
                movingRect.sizeDelta = source.GetItemVisualSize(moving.Item, rotated);

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    dragLayer, pointerPosition, pointerCamera, out Vector3 world))
                    movingRect.position = world;

                SetPreviewColor(false);
                return;
            }

            Vector2Int position = GetPlacementPosition(hoveredCell);

            movingRect.sizeDelta = target.GetItemVisualSize(moving.Item, rotated);
            movingRect.position = target.GetCellWorldPosition(position);

            bool valid = ReferenceEquals(source.Grid, target.Grid)
                ? source.Grid.CanMove(moving.Item, position, rotated)
                : target.Grid.CanPlace(moving.Item, position, rotated) &&
                  !ReferenceEquals(moving.Item.Contents, target.Grid);

            SetPreviewColor(valid);
        }

        private Vector2Int GetPlacementPosition(Vector2Int hoveredCell)
        {
            Vector2Int size = moving.Item.Definition.GridSize;
            if (rotated) size = new Vector2Int(size.y, size.x);

            Vector2Int offset = new Vector2Int(
                Mathf.Clamp(grabOffset.x, 0, size.x - 1),
                Mathf.Clamp(grabOffset.y, 0, size.y - 1)
            );

            return hoveredCell - offset;
        }

        private InventoryGridView GetHoveredGrid(out Vector2Int cell)
        {
            if (handsView != null && handsView.TryGetCell(pointerPosition, pointerCamera, out cell)) return handsView;
            if (rigView != null && rigView.TryGetCell(pointerPosition, pointerCamera, out cell)) return rigView;
            if (backpackView != null && backpackView.TryGetCell(pointerPosition, pointerCamera, out cell)) return backpackView;
            if (groundView != null && groundView.TryGetCell(pointerPosition, pointerCamera, out cell)) return groundView;

            cell = Vector2Int.zero;
            return null;
        }

        private void SetPreviewColor(bool valid)
        {
            if (background == null) return;

            background.color = valid
                ? new Color(0.2f, 0.8f, 0.4f, 0.9f)
                : new Color(0.9f, 0.25f, 0.25f, 0.9f);
        }
    }
}