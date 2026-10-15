using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryTestRunner : MonoBehaviour
    {
        [Header("Test Item Definitions")]
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;

        [ContextMenu("Run Inventory Test")]
        public void RunTest()
        {
            if (rifleDefinition == null || medkitDefinition == null)
            {
                Debug.LogWarning("Asigna Rifle y Medkit en InventoryTestRunner.", this);
                return;
            }

            InventoryGrid grid = new InventoryGrid(6, 4);
            ItemInstance rifle = new ItemInstance(rifleDefinition);
            ItemInstance medkit = new ItemInstance(medkitDefinition);

            bool rifleAdded = grid.TryAutoPlace(rifle);
            bool duplicateBlocked = !grid.TryAutoPlace(rifle);
            bool overlapBlocked = !grid.TryPlace(medkit, Vector2Int.zero);
            bool medkitAdded = grid.TryAutoPlace(medkit);

            Debug.Log($"[Inventory] Rifle placed: {rifleAdded}, duplicate blocked: {duplicateBlocked}, overlap blocked: {overlapBlocked}, medkit placed: {medkitAdded}", this);

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
