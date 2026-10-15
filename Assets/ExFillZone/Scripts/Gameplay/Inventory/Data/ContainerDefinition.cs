using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Container_New", menuName = "ExFillZone/Inventory/Container Definition")]
    public sealed class ContainerDefinition : ItemDefinition
    {
        [Header("Container")]
        [SerializeField] private EquipmentTier tier = EquipmentTier.T1;

        [Header("Storage Capacity")]
        [SerializeField, Min(1)] private int columns = 5;
        [SerializeField, Min(1)] private int rows = 5;

        public EquipmentTier Tier => tier;
        public int Columns => Mathf.Max(1, columns);
        public int Rows => Mathf.Max(1, rows);
        public bool SupportsQuickAccess => Category == ItemCategory.TacticalRig;
    }
}
