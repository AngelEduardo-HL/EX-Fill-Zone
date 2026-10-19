using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Armor_New", menuName = "ExFillZone/Inventory/Armor Definition")]
    public sealed class ArmorDefinition : ItemDefinition
    {
        [Header("Armor")]
        [SerializeField] private EquipmentTier tier = EquipmentTier.T1;
        [SerializeField, Range(0f, 100f)] private float mitigationPercent = 10f;

        public EquipmentTier Tier => tier;
        public float MitigationPercent => mitigationPercent;

        public float GetMitigation(float currentDurability)
        {
            if (MaxDurability <= 0f) return 0f;

            float condition = Mathf.Clamp01(currentDurability / MaxDurability);
            return (mitigationPercent / 100f) * condition;
        }
    }
}
