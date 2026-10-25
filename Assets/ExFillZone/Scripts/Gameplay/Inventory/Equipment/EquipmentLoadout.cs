using System;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [Serializable]
    public sealed class EquipmentLoadout
    {
        [SerializeReference] private ItemInstance helmet;
        [SerializeReference] private ItemInstance bodyArmor;
        [SerializeReference] private ItemInstance tacticalRig;
        [SerializeReference] private ItemInstance backpack;
        [SerializeReference] private ItemInstance primaryWeapon;
        [SerializeReference] private ItemInstance secondaryWeapon;

        public InventoryGrid RigInventory => tacticalRig?.Contents;
        public InventoryGrid BackpackInventory => backpack?.Contents;

        [Header("Armor Balance")]
        [SerializeField, Range(0f, 1f)] private float maxArmorMitigation = 0.8f;

        public float ArmorMitigation
        {
            get
            {
                float total = GetArmorMitigation(helmet) + GetArmorMitigation(bodyArmor);
                return Mathf.Min(total, maxArmorMitigation);
            }
        }

        private float GetArmorMitigation(ItemInstance item)
        {
            if (item?.Definition is not ArmorDefinition armor) return 0f;
            return armor.GetMitigation(item.Durability);
        }

        public float TotalWeight =>
            GetWeight(helmet) + GetWeight(bodyArmor) + GetWeight(tacticalRig) +
            GetWeight(backpack) + GetWeight(primaryWeapon) + GetWeight(secondaryWeapon);

        public int TotalValue =>
            GetValue(helmet) + GetValue(bodyArmor) + GetValue(tacticalRig) +
            GetValue(backpack) + GetValue(primaryWeapon) + GetValue(secondaryWeapon);

        private float GetWeight(ItemInstance item) => item?.TotalWeight ?? 0f;
        private int GetValue(ItemInstance item) => item?.TotalValue ?? 0;

        public ItemInstance GetItem(EquipmentSlotType slot)
        {
            return slot switch
            {
                EquipmentSlotType.Helmet => helmet,
                EquipmentSlotType.BodyArmor => bodyArmor,
                EquipmentSlotType.TacticalRig => tacticalRig,
                EquipmentSlotType.Backpack => backpack,
                EquipmentSlotType.PrimaryWeapon => primaryWeapon,
                EquipmentSlotType.SecondaryWeapon => secondaryWeapon,
                _ => null
            };
        }

        private void SetItem(EquipmentSlotType slot, ItemInstance item)
        {
            switch (slot)
            {
                case EquipmentSlotType.Helmet: helmet = item; break;
                case EquipmentSlotType.BodyArmor: bodyArmor = item; break;
                case EquipmentSlotType.TacticalRig: tacticalRig = item; break;
                case EquipmentSlotType.Backpack: backpack = item; break;
                case EquipmentSlotType.PrimaryWeapon: primaryWeapon = item; break;
                case EquipmentSlotType.SecondaryWeapon: secondaryWeapon = item; break;
            }
        }

        public bool IsValidForSlot(ItemDefinition definition, EquipmentSlotType slot)
        {
            if (definition == null) return false;

            return slot switch
            {
                EquipmentSlotType.Helmet => definition.Category == ItemCategory.Helmet,
                EquipmentSlotType.BodyArmor => definition.Category == ItemCategory.BodyArmor,
                EquipmentSlotType.TacticalRig => definition.Category == ItemCategory.TacticalRig && definition is ContainerDefinition,
                EquipmentSlotType.Backpack => definition.Category == ItemCategory.Backpack && definition is ContainerDefinition,
                EquipmentSlotType.PrimaryWeapon => definition.Category == ItemCategory.Weapon,
                EquipmentSlotType.SecondaryWeapon => definition.Category == ItemCategory.Weapon,
                _ => false
            };
        }

        public bool TryEquip(InventoryGrid source, ItemInstance item, EquipmentSlotType slot)
        {
            if (source == null || item?.Definition == null) return false;
            if (!source.Contains(item) || GetItem(slot) != null) return false;
            if (item.Quantity != 1 || !IsValidForSlot(item.Definition, slot)) return false;

            InventoryPlacement original = null;

            foreach (InventoryPlacement placement in source.Placements)
            {
                if (ReferenceEquals(placement.Item, item))
                {
                    original = placement;
                    break;
                }
            }

            if (original == null) return false;

            Vector2Int previousPosition = original.Position;
            bool previousRotation = original.Rotated;

            if (!source.TryRemove(item)) return false;

            if (!item.TryClaim(this))
            {
                source.TryPlace(item, previousPosition, previousRotation);
                return false;
            }

            SetItem(slot, item);
            return true;
        }

        public bool TryUnequip(EquipmentSlotType slot, InventoryGrid destination, Vector2Int position, bool rotated = false)
        {
            ItemInstance item = GetItem(slot);

            if (item == null || destination == null) return false;
            if (ReferenceEquals(item.Contents, destination)) return false;
            if (!item.IsOwnedBy(this) || !destination.CanPlace(item, position, rotated)) return false;

            if (!item.TryRelease(this)) return false;

            if (!destination.TryPlace(item, position, rotated))
            {
                item.TryClaim(this);
                return false;
            }

            SetItem(slot, null);
            return true;
        }

        public bool TryUnequipAuto(EquipmentSlotType slot, InventoryGrid destination)
        {
            ItemInstance item = GetItem(slot);

            if (item?.Definition == null || destination == null) return false;
            if (ReferenceEquals(item.Contents, destination)) return false;

            for (int orientation = 0; orientation < 2; orientation++)
            {
                bool rotated = orientation == 1;

                if (rotated && item.Definition.GridSize.x == item.Definition.GridSize.y) break;

                for (int y = 0; y < destination.Height; y++)
                {
                    for (int x = 0; x < destination.Width; x++)
                    {
                        Vector2Int position = new Vector2Int(x, y);

                        if (destination.CanPlace(item, position, rotated))
                            return TryUnequip(slot, destination, position, rotated);
                    }
                }
            }

            return false;
        }
        public bool CanEquipOrSwap(InventoryGrid source, ItemInstance incoming, EquipmentSlotType slot)
        {
            if (source == null || incoming?.Definition == null || incoming.Quantity != 1)
                return false;

            if (!source.Contains(incoming) || !incoming.IsOwnedBy(source) ||
                !IsValidForSlot(incoming.Definition, slot))
                return false;

            ItemInstance old = GetItem(slot);

            if (old == null) return true;

            if (!old.IsOwnedBy(this) || ReferenceEquals(source, old.Contents))
                return false;

            return TryFindSwapSpace(source, incoming, old, out _, out _);
        }

        public bool TryEquipOrSwap(InventoryGrid source, ItemInstance incoming, EquipmentSlotType slot)
        {
            if (!CanEquipOrSwap(source, incoming, slot)) return false;

            ItemInstance old = GetItem(slot);

            if (old == null)
                return TryEquip(source, incoming, slot);

            InventoryPlacement original = null;

            foreach (InventoryPlacement placement in source.Placements)
            {
                if (!ReferenceEquals(placement.Item, incoming)) continue;
                original = placement;
                break;
            }

            if (original == null ||
                !TryFindSwapSpace(source, incoming, old, out Vector2Int position, out bool rotated))
                return false;

            Vector2Int previousPosition = original.Position;
            bool previousRotation = original.Rotated;

            if (!source.TryRemove(incoming)) return false;

            if (!old.TryRelease(this))
            {
                source.TryPlace(incoming, previousPosition, previousRotation);
                return false;
            }

            if (!source.TryPlace(old, position, rotated))
            {
                old.TryClaim(this);
                source.TryPlace(incoming, previousPosition, previousRotation);
                return false;
            }

            if (!incoming.TryClaim(this))
            {
                source.TryRemove(old);
                old.TryClaim(this);
                source.TryPlace(incoming, previousPosition, previousRotation);
                return false;
            }

            SetItem(slot, incoming);
            return true;
        }

        private bool TryFindSwapSpace(InventoryGrid source, ItemInstance incoming, ItemInstance old,
            out Vector2Int position, out bool rotated)
        {
            position = Vector2Int.zero;
            rotated = false;

            if (source == null || old?.Definition == null) return false;

            for (int orientation = 0; orientation < 2; orientation++)
            {
                bool testRotation = orientation == 1;

                if (testRotation && old.Definition.GridSize.x == old.Definition.GridSize.y)
                    break;

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Vector2Int candidate = new Vector2Int(x, y);

                        if (!source.CanPlaceReplacing(old, incoming, candidate, testRotation))
                            continue;

                        position = candidate;
                        rotated = testRotation;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanMoveBetweenSlots(EquipmentSlotType from, EquipmentSlotType to)
        {
            if (from == to) return false;

            ItemInstance first = GetItem(from);
            ItemInstance second = GetItem(to);

            if (first?.Definition == null || !first.IsOwnedBy(this) ||
                !IsValidForSlot(first.Definition, to))
                return false;

            return second == null ||
                   (second.Definition != null && second.IsOwnedBy(this) &&
                    IsValidForSlot(second.Definition, from));
        }

        public bool TryMoveBetweenSlots(EquipmentSlotType from, EquipmentSlotType to)
        {
            if (!CanMoveBetweenSlots(from, to)) return false;

            ItemInstance first = GetItem(from);
            ItemInstance second = GetItem(to);

            SetItem(from, second);
            SetItem(to, first);
            return true;
        }
    }
}