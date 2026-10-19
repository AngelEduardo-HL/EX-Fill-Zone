using System;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public static class InventoryTransfer
    {
        public static bool TryTransfer(InventoryGrid source, InventoryGrid destination, ItemInstance item, Vector2Int position, bool rotated = false)
        {
            if (source == null || destination == null || item == null || ReferenceEquals(source, destination)) return false;
            if (!source.Contains(item) || destination.Contains(item) || ReferenceEquals(item.Contents, destination)) return false;
            if (!destination.CanPlace(item, position, rotated)) return false;

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
            if (destination.TryPlace(item, position, rotated)) return true;

            if (!source.TryPlace(item, previousPosition, previousRotation))
                Debug.LogError("InventoryTransfer: no se pudo restaurar el objeto a su inventario original.");

            return false;
        }

        public static bool TryAutoTransfer(InventoryGrid source, InventoryGrid destination, ItemInstance item)
        {
            if (source == null || destination == null || item?.Definition == null) return false;

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
                            return TryTransfer(source, destination, item, position, rotated);
                    }
                }
            }

            return false;
        }
    }
}