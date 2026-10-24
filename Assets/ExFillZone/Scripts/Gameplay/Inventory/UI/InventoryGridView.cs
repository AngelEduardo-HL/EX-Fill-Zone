using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryGridView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform cellsRoot;
        [SerializeField] private RectTransform itemsRoot;
        [SerializeField] private GridLayoutGroup cellsLayout;
        [SerializeField] private LayoutElement layoutElement;

        [Header("Prefabs")]
        [SerializeField] private InventoryCellUI cellPrefab;
        [SerializeField] private InventoryItemUI itemPrefab;

        [Header("Grid Appearance")]
        [SerializeField] private Vector2 cellSize = new Vector2(56f, 56f);
        [SerializeField] private Vector2 spacing = new Vector2(4f, 4f);
        [SerializeField, Min(0)] private int padding = 6;

        [Header("Drag & Drop")]
        [SerializeField] private InventoryDragController dragController;

        public InventoryGrid Grid => inventory;

        private InventoryGrid inventory;

        public void ShowGrid(InventoryGrid grid)
        {
            inventory = grid;
            gameObject.SetActive(grid != null);

            if (grid != null) Refresh();
        }

        public void Refresh()
        {
            if (inventory == null) return;

            if (cellsRoot == null || itemsRoot == null || cellsLayout == null ||
                layoutElement == null || cellPrefab == null || itemPrefab == null)
            {
                Debug.LogError("[InventoryGridView] Faltan referencias en el Inspector.", this);
                return;
            }

            ClearChildren(cellsRoot);
            ClearChildren(itemsRoot);

            cellsLayout.cellSize = cellSize;
            cellsLayout.spacing = spacing;
            cellsLayout.padding = new RectOffset(padding, padding, padding, padding);
            cellsLayout.childAlignment = TextAnchor.UpperLeft;
            cellsLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            cellsLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            cellsLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cellsLayout.constraintCount = inventory.Width;

            layoutElement.preferredHeight = inventory.Height * cellSize.y +
                                            (inventory.Height - 1) * spacing.y + padding * 2;

            for (int i = 0; i < inventory.Width * inventory.Height; i++)
                Instantiate(cellPrefab, cellsRoot, false);

            foreach (InventoryPlacement placement in inventory.Placements)
            {
                InventoryItemUI visual = Instantiate(itemPrefab, itemsRoot, false);
                visual.Setup(placement.Item, this, dragController);

                RectTransform rect = visual.GetComponent<RectTransform>();
                Vector2Int size = placement.Size;
                Vector2Int position = placement.Position;

                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);

                rect.sizeDelta = new Vector2(
                    size.x * cellSize.x + (size.x - 1) * spacing.x,
                    size.y * cellSize.y + (size.y - 1) * spacing.y
                );

                rect.anchoredPosition = new Vector2(
                    padding + position.x * (cellSize.x + spacing.x),
                    -padding - position.y * (cellSize.y + spacing.y)
                );
            }

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }

        private void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        public bool TryGetCell(Vector2 screenPosition, Camera camera, out Vector2Int cell)
        {
            cell = Vector2Int.zero;

            if (!isActiveAndEnabled || inventory == null || cellsRoot == null) return false;

            ScrollRect scroll = GetComponentInParent<ScrollRect>();

            if (scroll != null && scroll.viewport != null &&
                !RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, screenPosition, camera))
                return false;

            if (!RectTransformUtility.RectangleContainsScreenPoint(cellsRoot, screenPosition, camera))
                return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(cellsRoot, screenPosition, camera, out Vector2 local))
                return false;

            Rect rect = cellsRoot.rect;
            float x = local.x - rect.xMin - padding;
            float y = rect.yMax - local.y - padding;

            if (x < 0f || y < 0f) return false;

            float stepX = cellSize.x + spacing.x;
            float stepY = cellSize.y + spacing.y;

            int column = Mathf.FloorToInt(x / stepX);
            int row = Mathf.FloorToInt(y / stepY);

            if (column < 0 || row < 0 || column >= inventory.Width || row >= inventory.Height)
                return false;

            if (x - column * stepX >= cellSize.x || y - row * stepY >= cellSize.y)
                return false;

            cell = new Vector2Int(column, row);
            return true;
        }

        public Vector2 GetItemVisualSize(ItemInstance item, bool rotated)
        {
            Vector2Int size = item.Definition.GridSize;
            if (rotated) size = new Vector2Int(size.y, size.x);

            return new Vector2(
                size.x * cellSize.x + (size.x - 1) * spacing.x,
                size.y * cellSize.y + (size.y - 1) * spacing.y
            );
        }
        public Vector3 GetCellWorldPosition(Vector2Int cell)
        {
            Rect rect = cellsRoot.rect;

            Vector3 local = new Vector3(
                rect.xMin + padding + cell.x * (cellSize.x + spacing.x),
                rect.yMax - padding - cell.y * (cellSize.y + spacing.y),
                0f
            );

            return cellsRoot.TransformPoint(local);
        }
    }
}