using System;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [Serializable]
    public sealed class ItemInstance
    {
        [SerializeField] private string instanceId;
        [SerializeField] private ItemDefinition definition;
        [SerializeField, Min(1)] private int quantity;
        [SerializeField, Min(0f)] private float durability;
        [SerializeField] private InventoryGrid contents;

        public string InstanceId => instanceId;
        public ItemDefinition Definition => definition;
        public int Quantity => quantity;
        public float Durability => durability;
        public InventoryGrid Contents => contents;

        public float TotalWeight => definition == null ? 0f : definition.Weight * quantity + (contents?.TotalWeight ?? 0f);
        public int TotalValue => definition == null ? 0 : definition.Value * quantity + (contents?.TotalValue ?? 0);

        public ItemInstance(ItemDefinition definition, int quantity = 1)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            this.definition = definition;
            this.quantity = definition is ContainerDefinition ? 1 : Mathf.Clamp(quantity, 1, definition.MaxStack);
            instanceId = Guid.NewGuid().ToString("N");
            durability = definition.MaxDurability;

            if (definition is ContainerDefinition container)
                contents = new InventoryGrid(container.Columns, container.Rows);
        }

        public void SetDurability(float value)
        {
            if (definition == null) return;
            durability = Mathf.Clamp(value, 0f, definition.MaxDurability);
        }

        public int AddQuantity(int amount)
        {
            if (definition == null || amount <= 0) return 0;

            int added = Mathf.Min(amount, definition.MaxStack - quantity);
            quantity += added;
            return added;
        }

        public int RemoveQuantity(int amount)
        {
            if (amount <= 0) return 0;

            int removed = Mathf.Min(amount, quantity);
            quantity -= removed;
            return removed;
        }
    }
}