using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class CharacterInventory : MonoBehaviour
    {
        [Header("Pockets")]
        [SerializeField, Min(1)] private int pocketColumns = 4;
        [SerializeField, Min(1)] private int pocketRows = 2;

        [Header("Equipment")]
        [SerializeReference] private EquipmentLoadout equipment = new EquipmentLoadout();

        [Header("Runtime Inventory")]
        [SerializeReference] private InventoryGrid pockets;

        public InventoryGrid Pockets => pockets;
        public EquipmentLoadout Equipment => equipment;

        public InventoryGrid RigInventory => equipment.RigInventory;
        public InventoryGrid BackpackInventory => equipment.BackpackInventory;

        public float TotalWeight => (pockets?.TotalWeight ?? 0f) + equipment.TotalWeight;
        public int TotalValue => (pockets?.TotalValue ?? 0) + equipment.TotalValue;
        public float ArmorMitigation => equipment?.ArmorMitigation ?? 0f;

        private void Awake()
        {
            pockets = new InventoryGrid(pocketColumns, pocketRows);
            if (equipment == null) equipment = new EquipmentLoadout();
        }
    }
}