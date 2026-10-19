using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryContainerTestRunner : MonoBehaviour
    {
        [Header("Definitions")]
        [SerializeField] private ContainerDefinition backpackDefinition;
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;

        [ContextMenu("Run Container Test")]
        public void RunTest()
        {
            if (backpackDefinition == null || rifleDefinition == null || medkitDefinition == null)
            {
                Debug.LogWarning("Faltan definiciones en InventoryContainerTestRunner.", this);
                return;
            }

            InventoryGrid playerInventory = new InventoryGrid(6, 4);
            InventoryGrid groundInventory = new InventoryGrid(6, 4);

            ItemInstance backpack = new ItemInstance(backpackDefinition);
            ItemInstance rifle = new ItemInstance(rifleDefinition);
            ItemInstance medkit = new ItemInstance(medkitDefinition);

            bool backpackPlaced = playerInventory.TryAutoPlace(backpack);
            bool rifleStored = backpack.Contents.TryAutoPlace(rifle);
            bool medkitStored = backpack.Contents.TryAutoPlace(medkit);

            Debug.Log($"[Container] Backpack placed: {backpackPlaced}, rifle stored: {rifleStored}, medkit stored: {medkitStored}", this);
            Debug.Log($"[Container] Backpack contents: {backpack.Contents.Placements.Count}", this);
            Debug.Log($"[Container] Total player weight: {playerInventory.TotalWeight:0.##} kg", this);

            bool transferred = InventoryTransfer.TryAutoTransfer(backpack.Contents, groundInventory, medkit);

            Debug.Log($"[Transfer] Medkit transferred to ground: {transferred}", this);
            Debug.Log($"[Transfer] Medkit removed from backpack: {!backpack.Contents.Contains(medkit)}", this);
            Debug.Log($"[Transfer] Medkit now on ground: {groundInventory.Contains(medkit)}", this);
            Debug.Log($"[Container] Weight after transfer: {playerInventory.TotalWeight:0.##} kg", this);

            InventoryGrid tinyInventory = new InventoryGrid(1, 1);
            bool invalidTransferBlocked = !InventoryTransfer.TryAutoTransfer(backpack.Contents, tinyInventory, rifle);

            Debug.Log($"[Transfer] Invalid transfer blocked: {invalidTransferBlocked}", this);
            Debug.Log($"[Transfer] Rifle still in backpack: {backpack.Contents.Contains(rifle)}", this);
        }

        private void Start()
        {
            RunTest();
        }
    }
}