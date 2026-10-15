using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class EquipmentTestRunner : MonoBehaviour
    {
        [Header("Definitions")]
        [SerializeField] private ContainerDefinition backpackDefinition;
        [SerializeField] private ContainerDefinition rigDefinition;
        [SerializeField] private ItemDefinition helmetDefinition;
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;

        private CharacterInventory inventory;

        private void Awake()
        {
            inventory = GetComponent<CharacterInventory>();
        }

        private void Start()
        {
            RunTest();
        }

        [ContextMenu("Run Equipment Test")]
        public void RunTest()
        {
            if (backpackDefinition == null || rigDefinition == null || helmetDefinition == null ||
                rifleDefinition == null || medkitDefinition == null)
            {
                Debug.LogWarning("[Equipment] Faltan definiciones.", this);
                return;
            }

            InventoryGrid pockets = inventory.Pockets;
            EquipmentLoadout equipment = inventory.Equipment;

            if (pockets.Placements.Count > 0)
            {
                Debug.LogWarning("[Equipment] La prueba ya se ejecutó.", this);
                return;
            }

            ItemInstance backpack = new ItemInstance(backpackDefinition);
            ItemInstance rig = new ItemInstance(rigDefinition);
            ItemInstance helmet = new ItemInstance(helmetDefinition);
            ItemInstance rifle = new ItemInstance(rifleDefinition);
            ItemInstance medkit = new ItemInstance(medkitDefinition);

            bool stored = pockets.TryAutoPlace(backpack) && pockets.TryAutoPlace(rig) &&
                          pockets.TryAutoPlace(helmet) && pockets.TryAutoPlace(rifle) &&
                          pockets.TryAutoPlace(medkit);

            bool invalidEquipmentBlocked = !equipment.TryEquip(pockets, medkit, EquipmentSlotType.Helmet);

            bool backpackEquipped = equipment.TryEquip(pockets, backpack, EquipmentSlotType.Backpack);
            bool rigEquipped = equipment.TryEquip(pockets, rig, EquipmentSlotType.TacticalRig);
            bool helmetEquipped = equipment.TryEquip(pockets, helmet, EquipmentSlotType.Helmet);
            bool rifleEquipped = equipment.TryEquip(pockets, rifle, EquipmentSlotType.PrimaryWeapon);

            bool medkitStored = InventoryTransfer.TryAutoTransfer(pockets, equipment.RigInventory, medkit);
            bool medkitTransferred = InventoryTransfer.TryAutoTransfer(equipment.RigInventory, equipment.BackpackInventory, medkit);

            bool ownInventoryBlocked = !equipment.TryUnequipAuto(EquipmentSlotType.Backpack, backpack.Contents);

            float weightBefore = inventory.TotalWeight;
            int valueBefore = inventory.TotalValue;

            bool backpackUnequipped = equipment.TryUnequipAuto(EquipmentSlotType.Backpack, pockets);
            bool contentsPreserved = backpack.Contents.Contains(medkit);
            bool weightPreserved = Mathf.Approximately(weightBefore, inventory.TotalWeight);
            bool valuePreserved = valueBefore == inventory.TotalValue;

            InventoryGrid ground = new InventoryGrid(8, 6);
            bool duplicateBlocked = !ground.TryAutoPlace(backpack);

            ItemInstance anotherBackpack = new ItemInstance(backpackDefinition);
            bool nestingBlocked = !equipment.RigInventory.TryAutoPlace(anotherBackpack);

            bool ownershipPreserved = pockets.Contains(backpack) && !ground.Contains(backpack);

            Debug.Log($"[Equipment] Initial items stored: {stored}", this);
            Debug.Log($"[Equipment] Invalid equipment blocked: {invalidEquipmentBlocked}", this);
            Debug.Log($"[Equipment] Backpack equipped: {backpackEquipped}", this);
            Debug.Log($"[Equipment] Rig equipped: {rigEquipped}", this);
            Debug.Log($"[Equipment] Helmet equipped: {helmetEquipped}", this);
            Debug.Log($"[Equipment] Rifle equipped: {rifleEquipped}", this);
            Debug.Log($"[Equipment] Medkit stored in rig: {medkitStored}", this);
            Debug.Log($"[Equipment] Medkit transferred to backpack: {medkitTransferred}", this);
            Debug.Log($"[Equipment] Self-container placement blocked: {ownInventoryBlocked}", this);
            Debug.Log($"[Equipment] Backpack unequipped: {backpackUnequipped}", this);
            Debug.Log($"[Equipment] Contents preserved: {contentsPreserved}", this);
            Debug.Log($"[Equipment] Weight preserved: {weightPreserved}", this);
            Debug.Log($"[Equipment] Value preserved: {valuePreserved}", this);
            Debug.Log($"[Equipment] Final weight: {inventory.TotalWeight:0.##} kg, value: ${inventory.TotalValue:N0}", this);
            Debug.Log($"[Safety] Duplicate blocked: {duplicateBlocked}", this);
            Debug.Log($"[Safety] Container nesting blocked: {nestingBlocked}", this);
            Debug.Log($"[Safety] Ownership preserved: {ownershipPreserved}", this);
        }
    }
}