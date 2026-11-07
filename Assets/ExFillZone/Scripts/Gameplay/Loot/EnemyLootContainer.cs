using System;
using ExFillZone.Gameplay.Inventory;
using UnityEngine;

namespace ExFillZone.Gameplay.Loot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class EnemyLootContainer : MonoBehaviour
    {
        private CharacterInventory inventory;
        private bool initialized;

        public CharacterInventory Inventory => inventory;
        public bool Initialized => initialized;

        private void Awake()
        {
            inventory = GetComponent<CharacterInventory>();
        }

        public bool Initialize(CharacterInventory source)
        {
            if (initialized || source == null || inventory == null || source == inventory)
                return false;

            if (source.Pockets == null || inventory.Pockets == null ||
                source.Pockets.Width != inventory.Pockets.Width ||
                source.Pockets.Height != inventory.Pockets.Height)
            {
                Debug.LogError("[EnemyLoot] Las dimensiones de los bolsillos no coinciden.", this);
                return false;
            }

            bool success = true;

            foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                if (source.Equipment.GetItem(slot) == null) continue;

                if (!source.Equipment.TryTransferEquippedTo(inventory.Equipment, slot))
                {
                    Debug.LogWarning($"[EnemyLoot] No se pudo transferir {slot}.", this);
                    success = false;
                }
            }

            for (int i = source.Pockets.Placements.Count - 1; i >= 0; i--)
            {
                InventoryPlacement placement = source.Pockets.Placements[i];

                if (!InventoryTransfer.TryTransfer(source.Pockets, inventory.Pockets,
                    placement.Item, placement.Position, placement.Rotated))
                {
                    Debug.LogWarning($"[EnemyLoot] No se pudo transferir {placement.Item.Definition.DisplayName}.", this);
                    success = false;
                }
            }

            initialized = true;

            Debug.Log($"[EnemyLoot] Caja preparada | Valor: {inventory.TotalValue} | Peso: {inventory.TotalWeight:0.0} kg", this);
            return success;
        }

        public bool TryOpenInventory()
        {
            if (!initialized) return false;

            EnemyLootUIController controller = FindFirstObjectByType<EnemyLootUIController>();

            if (controller == null)
            {
                Debug.LogWarning("[EnemyLoot] No se encontró EnemyLootUIController.", this);
                return false;
            }

            return controller.OpenLoot(this);
        }
    }
}
