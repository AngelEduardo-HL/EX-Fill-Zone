using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Item_New", menuName = "ExFillZone/Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private ItemCategory category = ItemCategory.Miscellaneous;
        [SerializeField, TextArea(2, 4)] private string description;

        [Header("Inventory")]
        [SerializeField] private Vector2Int gridSize = Vector2Int.one;
        [SerializeField, Min(0f)] private float weight = 0.1f;
        [SerializeField, Min(0)] private int value;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField, Min(0f)] private float maxDurability;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private GameObject equippedPrefab;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public Vector2Int GridSize => gridSize;
        public float Weight => weight;
        public int Value => value;
        public int MaxStack => maxStack;
        public float MaxDurability => maxDurability;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;
        public GameObject EquippedPrefab => equippedPrefab;

        private void OnValidate()
        {
            gridSize.x = Mathf.Max(1, gridSize.x);
            gridSize.y = Mathf.Max(1, gridSize.y);
            weight = Mathf.Max(0f, weight);
            value = Mathf.Max(0, value);
            maxStack = Mathf.Max(1, maxStack);
            maxDurability = Mathf.Max(0f, maxDurability);
        }
    }
}
