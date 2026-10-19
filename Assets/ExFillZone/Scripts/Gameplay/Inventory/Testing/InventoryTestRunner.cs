using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryTestRunner : MonoBehaviour
    {
        [Header("Test Item Definitions")]
        [SerializeField] private ItemDefinition FirstItem;
        [SerializeField] private ItemDefinition SecItem;

        [ContextMenu("Run Inventory Test")]
        public void RunTest()
        {
            if (FirstItem == null || SecItem == null)
            {
                Debug.LogWarning("Asigna los objetos en InventoryTestRunner.", this);
                return;
            }

            InventoryGrid grid = new InventoryGrid(6, 4);
            ItemInstance item_1 = new ItemInstance(FirstItem);
            ItemInstance item_2 = new ItemInstance(SecItem);

            bool rifleAdded = grid.TryAutoPlace(item_1);
            bool duplicateBlocked = !grid.TryAutoPlace(item_1);
            bool overlapBlocked = !grid.TryPlace(item_2, Vector2Int.zero);
            bool medkitAdded = grid.TryAutoPlace(item_2);

            Debug.Log($"[Inventory] Item1 placed: {rifleAdded}, duplicate blocked: {duplicateBlocked}, overlap blocked: {overlapBlocked}, Item2 placed: {medkitAdded}", this);

            foreach (InventoryPlacement placement in grid.Placements)
            {
                Debug.Log($"[Inventory] {placement.Item.Definition.DisplayName}: {placement.Position}, size {placement.Size}, rotated {placement.Rotated}", this);
            }

            Debug.Log($"[Inventory] Items: {grid.Placements.Count}, weight: {grid.TotalWeight:0.##} kg, value: ${grid.TotalValue:N0}", this);
        }

        private void Start()
        {
            RunTest();
        }
    }
}
