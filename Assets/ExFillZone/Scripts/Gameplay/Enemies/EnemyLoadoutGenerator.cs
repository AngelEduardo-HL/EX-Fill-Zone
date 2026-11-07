using ExFillZone.Gameplay.Inventory;
using ExFillZone.Gameplay.Loot;
using UnityEngine;

namespace ExFillZone.Gameplay.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class EnemyLoadoutGenerator : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private EnemyProfile profile;
        [SerializeField] private EnemyLootTable lootTableOverride;

        private CharacterInventory inventory;
        private bool generated;

        public EnemyProfile Profile => profile;
        public CharacterInventory Inventory => inventory;

        private void Awake()
        {
            inventory = GetComponent<CharacterInventory>();
        }

        private void Start()
        {
            EnemyLootTable table = lootTableOverride != null ? lootTableOverride : profile != null ? profile.DefaultLootTable : null;

            if (table == null)
            {
                Debug.LogWarning($"[EnemyLoadout] {name} no tiene una tabla de loot asignada.", this);
                return;
            }

            Generate(table);
        }

        public void SetLootTable(EnemyLootTable table)
        {
            if (generated)
            {
                Debug.LogWarning("[EnemyLoadout] El equipamiento ya fue generado.", this);
                return;
            }

            lootTableOverride = table;
        }

        public void Generate(EnemyLootTable table)
        {
            if (generated || table == null) return;

            if (inventory == null) inventory = GetComponent<CharacterInventory>();
            if (inventory == null) return;

            generated = true;

            Equip(table.RollHelmet(), EquipmentSlotType.Helmet);
            Equip(table.RollBodyArmor(), EquipmentSlotType.BodyArmor);
            Equip(table.RollTacticalRig(), EquipmentSlotType.TacticalRig);
            Equip(table.RollBackpack(), EquipmentSlotType.Backpack);
            Equip(table.RollPrimaryWeapon(), EquipmentSlotType.PrimaryWeapon);
            Equip(table.RollSecondaryWeapon(), EquipmentSlotType.SecondaryWeapon);

            int extraCount = table.RollExtraCount();

            for (int i = 0; i < extraCount; i++)
            {
                ItemDefinition definition = table.RollExtraItem();
                if (definition == null) continue;

                ItemInstance item = new ItemInstance(definition);

                bool placed =
                    (inventory.RigInventory != null && inventory.RigInventory.TryAutoPlace(item)) ||
                    inventory.Pockets.TryAutoPlace(item) ||
                    (inventory.BackpackInventory != null && inventory.BackpackInventory.TryAutoPlace(item));

                if (!placed)
                    Debug.LogWarning($"[EnemyLoadout] Sin espacio para {definition.DisplayName} en {name}.", this);
            }

            Debug.Log($"[EnemyLoadout] {name} | Tipo: {profile?.Archetype} | Valor: {inventory.TotalValue} | Peso: {inventory.TotalWeight:0.0} kg | Protección: {inventory.ArmorMitigation:P0}", this);
        }

        private void Equip(ItemDefinition definition, EquipmentSlotType slot)
        {
            if (definition == null) return;

            if (!inventory.Equipment.TryEquipNew(definition, slot))
                Debug.LogWarning($"[EnemyLoadout] No se pudo equipar {definition.DisplayName} en {slot}.", this);
        }
    }
}
