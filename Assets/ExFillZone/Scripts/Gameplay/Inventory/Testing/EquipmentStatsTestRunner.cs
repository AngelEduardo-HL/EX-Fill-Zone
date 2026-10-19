using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class EquipmentStatsTestRunner : MonoBehaviour
    {
        [Header("Armor")]
        [SerializeField] private ArmorDefinition helmetDefinition;
        [SerializeField] private ArmorDefinition bodyArmorDefinition;

        [Header("Containers")]
        [SerializeField] private ContainerDefinition rigDefinition;
        [SerializeField] private ContainerDefinition backpackDefinition;

        private CharacterInventory inventory;

        private void Awake()
        {
            inventory = GetComponent<CharacterInventory>();
        }

        private void Start()
        {
            RunTest();
        }

        [ContextMenu("Run Equipment Stats Test")]
        public void RunTest()
        {
            if (helmetDefinition == null || bodyArmorDefinition == null || rigDefinition == null || backpackDefinition == null)
            {
                Debug.LogWarning("[Stats] Faltan definiciones.", this);
                return;
            }

            if (inventory == null) inventory = GetComponent<CharacterInventory>();
            if (inventory.Pockets == null || inventory.Pockets.Placements.Count > 0) return;

            InventoryGrid pockets = inventory.Pockets;
            EquipmentLoadout equipment = inventory.Equipment;

            ItemInstance helmet = new ItemInstance(helmetDefinition);
            ItemInstance bodyArmor = new ItemInstance(bodyArmorDefinition);
            ItemInstance rig = new ItemInstance(rigDefinition);
            ItemInstance backpack = new ItemInstance(backpackDefinition);

            bool stored = pockets.TryAutoPlace(helmet) && pockets.TryAutoPlace(bodyArmor) &&
                          pockets.TryAutoPlace(rig) && pockets.TryAutoPlace(backpack);

            if (!stored)
            {
                Debug.LogError("[Stats] Los objetos no caben en Pockets.", this);
                return;
            }

            bool helmetEquipped = equipment.TryEquip(pockets, helmet, EquipmentSlotType.Helmet);
            bool armorEquipped = equipment.TryEquip(pockets, bodyArmor, EquipmentSlotType.BodyArmor);
            bool rigEquipped = equipment.TryEquip(pockets, rig, EquipmentSlotType.TacticalRig);
            bool backpackEquipped = equipment.TryEquip(pockets, backpack, EquipmentSlotType.Backpack);

            Debug.Log($"[Stats] Helmet equipped: {helmetEquipped}", this);
            Debug.Log($"[Stats] Armor equipped: {armorEquipped}", this);
            Debug.Log($"[Stats] Rig equipped: {rigEquipped}", this);
            Debug.Log($"[Stats] Backpack equipped: {backpackEquipped}", this);

            Debug.Log($"[Stats] Total mitigation: {inventory.ArmorMitigation * 100f:0.0}%", this);
            Debug.Log($"[Stats] Rig quick access: {rigDefinition.SupportsQuickAccess}", this);
            Debug.Log($"[Stats] Backpack quick access blocked: {!backpackDefinition.SupportsQuickAccess}", this);

            if (inventory.RigInventory != null)
                Debug.Log($"[Stats] Rig grid: {inventory.RigInventory.Width}x{inventory.RigInventory.Height}", this);

            if (inventory.BackpackInventory != null)
                Debug.Log($"[Stats] Backpack grid: {inventory.BackpackInventory.Width}x{inventory.BackpackInventory.Height}", this);

            helmet.SetDurability(helmetDefinition.MaxDurability * 0.5f);
            Debug.Log($"[Stats] Mitigation after helmet damage: {inventory.ArmorMitigation * 100f:0.0}%", this);

            Debug.Log($"[Stats] Total weight: {inventory.TotalWeight:0.##} kg", this);
            Debug.Log($"[Stats] Total value: ${inventory.TotalValue:N0}", this);
        }
    }
}
