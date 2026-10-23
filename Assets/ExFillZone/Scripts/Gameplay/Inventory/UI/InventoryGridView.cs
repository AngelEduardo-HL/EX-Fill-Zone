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
                visual.Setup(placement.Item);

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
    }
}