using System;
using ExFillZone.Gameplay.Inventory;
using UnityEngine;

namespace ExFillZone.Gameplay.Loot
{
    [Serializable]
    public sealed class WeightedItemPool
    {
        [Serializable]
        public struct Entry
        {
            public ItemDefinition item;
            [Min(1)] public int weight;
        }

        [SerializeField, Range(0, 100)] private int rollChance = 100;
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public ItemDefinition Roll()
        {
            if (entries == null || entries.Length == 0 || UnityEngine.Random.Range(0, 100) >= rollChance)
                return null;

            int totalWeight = 0;

            foreach (Entry entry in entries)
            {
                if (entry.item != null && entry.weight > 0)
                    totalWeight += entry.weight;
            }

            if (totalWeight <= 0) return null;

            int roll = UnityEngine.Random.Range(0, totalWeight);

            foreach (Entry entry in entries)
            {
                if (entry.item == null || entry.weight <= 0) continue;

                if (roll < entry.weight) return entry.item;

                roll -= entry.weight;
            }

            return null;
        }
    }

    [CreateAssetMenu(fileName = "EnemyLoot_New", menuName = "ExFillZone/Loot/Enemy Loot Table")]
    public sealed class EnemyLootTable : ScriptableObject
    {
        [Header("Equipment Pools")]
        [SerializeField] private WeightedItemPool helmets = new WeightedItemPool();
        [SerializeField] private WeightedItemPool bodyArmors = new WeightedItemPool();
        [SerializeField] private WeightedItemPool tacticalRigs = new WeightedItemPool();
        [SerializeField] private WeightedItemPool backpacks = new WeightedItemPool();
        [SerializeField] private WeightedItemPool primaryWeapons = new WeightedItemPool();
        [SerializeField] private WeightedItemPool secondaryWeapons = new WeightedItemPool();

        [Header("Additional Loot")]
        [SerializeField, Min(0)] private int minExtraItems = 0;
        [SerializeField, Min(0)] private int maxExtraItems = 2;
        [SerializeField] private WeightedItemPool extraItems = new WeightedItemPool();

        public ItemDefinition RollHelmet() => helmets.Roll();
        public ItemDefinition RollBodyArmor() => bodyArmors.Roll();
        public ItemDefinition RollTacticalRig() => tacticalRigs.Roll();
        public ItemDefinition RollBackpack() => backpacks.Roll();
        public ItemDefinition RollPrimaryWeapon() => primaryWeapons.Roll();
        public ItemDefinition RollSecondaryWeapon() => secondaryWeapons.Roll();
        public ItemDefinition RollExtraItem() => extraItems.Roll();

        public int RollExtraCount() => UnityEngine.Random.Range(minExtraItems, Mathf.Max(minExtraItems, maxExtraItems) + 1);
    }
}
