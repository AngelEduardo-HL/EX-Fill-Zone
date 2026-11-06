using ExFillZone.Gameplay.Loot;
using UnityEngine;

namespace ExFillZone.Gameplay.Enemies
{
    public enum EnemyArchetype
    {
        Militant,
        Marksman,
        Assault,
        Hunter
    }

    [CreateAssetMenu(fileName = "EnemyProfile_New", menuName = "ExFillZone/Enemies/Enemy Profile")]
    public sealed class EnemyProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Militant;

        [Header("Default Equipment")]
        [SerializeField] private EnemyLootTable defaultLootTable;

        public EnemyArchetype Archetype => archetype;
        public EnemyLootTable DefaultLootTable => defaultLootTable;
    }
}
