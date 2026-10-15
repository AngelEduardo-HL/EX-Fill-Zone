using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExFillZone.Gameplay.Inventory
{
    [Serializable]
    public sealed class InventoryPlacement
    {
        [SerializeReference] private ItemInstance item;
        [SerializeField] private Vector2Int position;
        [SerializeField] private bool rotated;
        
        public ItemInstance Item => item;
        public Vector2Int Position => position;
        public bool Rotated => rotated;

        public Vector2Int Size
        {
            get
            {
                if (item?.Definition == null) return Vector2Int.zero;
                Vector2Int size = item.Definition.GridSize;
                return rotated ? new Vector2Int(size.y, size.x) : size;
            }
        }

        public InventoryPlacement(ItemInstance item, Vector2Int position, bool rotated)
        {
            this.item = item;
            this.position = position;
            this.rotated = rotated;
        }

        internal void SetPosition(Vector2Int position, bool rotated)
        {
            this.position = position;
            this.rotated = rotated;
        }
    }

    [Serializable]
    public sealed class InventoryGrid
    {
        [SerializeField, Min(1)] private int width = 1;
        [SerializeField, Min(1)] private int height = 1;
        [SerializeField] private List<InventoryPlacement> placements = new List<InventoryPlacement>();
        [SerializeField] private bool acceptsContainers = true;

        public bool AcceptsContainers => acceptsContainers;
        public int Width => width;
        public int Height => height;
        public IReadOnlyList<InventoryPlacement> Placements => placements;

        public InventoryGrid(int width, int height, bool acceptsContainers = true)
        {
            this.width = Mathf.Max(1, width);
            this.height = Mathf.Max(1, height);
            this.acceptsContainers = acceptsContainers;
        }

        public bool CanPlace(ItemInstance item, Vector2Int position, bool rotated = false)
        {
            return CanPlaceInternal(item, position, rotated, null);
        }

        private bool CanPlaceInternal(ItemInstance item, Vector2Int position, bool rotated, InventoryPlacement ignored)
        {
            if (item?.Definition == null || item.Quantity <= 0) return false;
            if (!acceptsContainers && item.Contents != null) return false;

            Vector2Int size = item.Definition.GridSize;
            if (rotated) size = new Vector2Int(size.y, size.x);

            if (position.x < 0 || position.y < 0 || position.x + size.x > width || position.y + size.y > height) return false;

            RectInt area = new RectInt(position, size);
            foreach (InventoryPlacement placement in placements)
            {
                if (placement == ignored) continue;
                if (area.Overlaps(new RectInt(placement.Position, placement.Size))) return false;
            }

            return true;
        }

        public bool TryPlace(ItemInstance item, Vector2Int position, bool rotated = false)
        {
            if (Contains(item) || !CanPlace(item, position, rotated)) return false;
            if (!item.TryClaim(this)) return false;

            placements.Add(new InventoryPlacement(item, position, rotated));
            return true;
        }

        public bool TryAutoPlace(ItemInstance item)
        {
            if (item?.Definition == null || Contains(item)) return false;

            for (int orientation = 0; orientation < 2; orientation++)
            {
                bool rotated = orientation == 1;
                if (rotated && item.Definition.GridSize.x == item.Definition.GridSize.y) break;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (TryPlace(item, new Vector2Int(x, y), rotated)) return true;
                    }
                }
            }

            return false;
        }

        public bool TryMove(ItemInstance item, Vector2Int position, bool rotated)
        {
            InventoryPlacement current = placements.Find(p => ReferenceEquals(p.Item, item));
            if (current == null || !item.IsOwnedBy(this) || !CanPlaceInternal(item, position, rotated, current)) return false;

            current.SetPosition(position, rotated);
            return true;
        }

        public bool TryRemove(ItemInstance item)
        {
            InventoryPlacement current = placements.Find(p => ReferenceEquals(p.Item, item));
            if (current == null || !item.IsOwnedBy(this)) return false;
            if (!item.TryRelease(this)) return false;

            placements.Remove(current);
            return true;
        }

        public bool Contains(ItemInstance item)
        {
            return item != null && placements.Exists(p => ReferenceEquals(p.Item, item));
        }

        public ItemInstance GetItemAt(Vector2Int position)
        {
            foreach (InventoryPlacement placement in placements)
            {
                RectInt area = new RectInt(placement.Position, placement.Size);
                if (area.Contains(position)) return placement.Item;
            }

            return null;
        }

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (InventoryPlacement placement in placements) total += placement.Item.TotalWeight;
                return total;
            }
        }

        public int TotalValue
        {
            get
            {
                int total = 0;
                foreach (InventoryPlacement placement in placements) total += placement.Item.TotalValue;
                return total;
            }
        }
    }
}
