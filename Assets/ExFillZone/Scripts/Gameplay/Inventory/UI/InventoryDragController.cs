using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        [Header("Grid Views")]
        [SerializeField] private InventoryGridView handsView;
        [SerializeField] private InventoryGridView rigView;
        [SerializeField] private InventoryGridView backpackView;
        [SerializeField] private InventoryGridView groundView;

        [Header("Quick Transfer")]
        [SerializeField] private InventoryGridView externalLootView;
        [SerializeField] private bool groundAsTemporaryLoot = true;

        [Header("Equipment")]
        [SerializeField] private PlayerInventoryPanelsUI playerPanels;
        [SerializeField] private PlayerEquipmentPanelUI equipmentPanel;

        [Header("Drag Preview")]
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private InventoryItemUI itemPrefab;
        [SerializeField] private InventoryDiscardZoneUI discardZone;

        [Header("Enemy Loot")]
        [SerializeField] private EnemyLootUIController enemyLootUI;


        private readonly EquipmentSlotType[] equipmentPriority =
        {
            EquipmentSlotType.Helmet,
            EquipmentSlotType.BodyArmor,
            EquipmentSlotType.TacticalRig,
            EquipmentSlotType.Backpack,
            EquipmentSlotType.PrimaryWeapon,
            EquipmentSlotType.SecondaryWeapon
        };

        private CharacterInventory character;

        private InventoryItemUI sourceItemUI;
        private InventoryGridView sourceGrid;
        private EquipmentSlotUI sourceSlot;

        private ItemInstance movingItem;
        private InventoryItemUI preview;
        private RectTransform previewRect;
        private Image previewBackground;

        private CanvasGroup sourceCanvasGroup;
        private float originalAlpha;

        private Vector2 pointerPosition;
        private Camera pointerCamera;
        private bool rotated;

        public bool IsDragging => movingItem != null;

        public void Bind(CharacterInventory inventory)
        {
            character = inventory;
            discardZone?.SetVisible(false);
            enemyLootUI?.ConfigureDrag(this);
        }

        private void Update()
        {
            if (movingItem != null)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    rotated = !rotated;
                    RefreshPreview();
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.X))
                DropHoveredItem();
        }

        // SHORTCUTS

        public void OnItemClicked(InventoryItemUI visual)
        {
            if (movingItem != null || visual?.Item == null || visual.SourceGrid?.Grid == null)
                return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool alt = Input.GetKey(KeyCode.LeftAlt);

            if (!ctrl && !alt) return;

            bool success = ctrl
                ? TryQuickTransfer(visual)
                : TryQuickEquip(visual);

            if (success) RefreshAll();

            Debug.Log($"[Shortcut] {visual.Item.Definition.DisplayName}: {(success ? "Success" : "Cancelled")}", this);
        }

        public void OnEquipmentClicked(EquipmentSlotUI slot)
        {
            if (movingItem != null || slot?.Item == null) return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl) return;

            EquipmentLoadout owner = GetEquipmentOwner(slot);

            if (owner == null || !ReferenceEquals(owner.GetItem(slot.SlotType), slot.Item))
                return;

            bool success = TryUnequipTo(owner, slot, handsView) ||
                           TryUnequipTo(owner, slot, rigView) ||
                           TryUnequipTo(owner, slot, backpackView);

            if (!success && ReferenceEquals(owner, character?.Equipment))
            {
                InventoryGridView external = GetExternalInventory();
                if (external != null) success = TryUnequipTo(owner, slot, external);
            }

            if (success) RefreshAll();

            Debug.Log($"[Shortcut] Unequip {slot.SlotType}: {(success ? "Success" : "Cancelled")}", this);
        }

        private bool TryUnequipTo(EquipmentLoadout owner, EquipmentSlotUI slot, InventoryGridView destination)
        {
            if (owner == null || destination?.Grid == null || !destination.gameObject.activeInHierarchy)
                return false;

            return owner.TryUnequipAuto(slot.SlotType, destination.Grid);
        }


        private InventoryGridView GetExternalInventory()
        {
            if (enemyLootUI != null && enemyLootUI.IsLootOpen) return null;

            if (externalLootView != null && externalLootView.Grid != null &&
                externalLootView.gameObject.activeInHierarchy)
                return externalLootView;

            return groundAsTemporaryLoot && groundView != null &&
                   groundView.gameObject.activeInHierarchy ? groundView : null;
        }


        private bool TryQuickTransfer(InventoryItemUI visual)
        {
            InventoryGrid source = visual.SourceGrid.Grid;

            if (enemyLootUI != null && enemyLootUI.IsLootOpen)
            {
                if (IsEnemyGrid(source))
                    return TryTransferTo(visual, handsView) ||
                           TryTransferTo(visual, rigView) ||
                           TryTransferTo(visual, backpackView);

                if (IsPlayerGrid(source))
                    return TryTransferTo(visual, enemyLootUI.EnemyHandsView) ||
                           TryTransferTo(visual, enemyLootUI.EnemyRigView) ||
                           TryTransferTo(visual, enemyLootUI.EnemyBackpackView);

                return false;
            }

            InventoryGridView external = GetExternalInventory();

            if (external?.Grid == null) return false;

            if (ReferenceEquals(source, external.Grid))
                return TryTransferTo(visual, handsView) ||
                       TryTransferTo(visual, rigView) ||
                       TryTransferTo(visual, backpackView);

            if (!IsPlayerGrid(source)) return false;

            return InventoryTransfer.TryAutoTransfer(source, external.Grid, visual.Item);
        }

        private bool TryTransferTo(InventoryItemUI visual, InventoryGridView destination)
        {
            if (destination?.Grid == null || !destination.gameObject.activeInHierarchy)
                return false;

            return InventoryTransfer.TryAutoTransfer(
                visual.SourceGrid.Grid, destination.Grid, visual.Item);
        }

        private bool TryQuickEquip(InventoryItemUI visual)
        {
            if (character == null) return false;

            InventoryGrid source = visual.SourceGrid.Grid;
            ItemInstance item = visual.Item;
            EquipmentLoadout equipment = character.Equipment;

            // Primero buscamos un slot compatible y vacío.
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (EquipmentSlotType slot in equipmentPriority)
                {
                    if (!equipment.IsValidForSlot(item.Definition, slot)) continue;

                    bool occupied = equipment.GetItem(slot) != null;

                    if (pass == 0 && occupied) continue;
                    if (pass == 1 && !occupied) continue;

                    if (equipment.TryEquipOrSwap(source, item, slot))
                        return true;
                }
            }

            return false;
        }

        private void DropHoveredItem()
        {
            if (EventSystem.current == null) return;

            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            foreach (RaycastResult result in results)
            {
                InventoryItemUI itemUI = result.gameObject.GetComponentInParent<InventoryItemUI>();

                if (itemUI != null && itemUI.Item != null && itemUI.SourceGrid != null)
                {
                    bool success = TryDropFromGrid(itemUI.SourceGrid, itemUI.Item);
                    if (success) RefreshAll();

                    Debug.Log($"[Drop X] {(success ? "Dropped" : "Cancelled")}", this);
                    return;
                }

                EquipmentSlotUI slotUI = result.gameObject.GetComponentInParent<EquipmentSlotUI>();

                if (slotUI != null && slotUI.Item != null)
                {
                    bool success = TryDropFromSlot(slotUI);
                    if (success) RefreshAll();

                    Debug.Log($"[Drop X] {(success ? "Dropped" : "Cancelled")}", this);
                    return;
                }
            }
        }

        private bool IsPlayerGrid(InventoryGrid grid)
        {
            return character != null && grid != null &&
                   (ReferenceEquals(grid, character.Pockets) ||
                    ReferenceEquals(grid, character.RigInventory) ||
                    ReferenceEquals(grid, character.BackpackInventory));
        }

        private bool IsEnemyGrid(InventoryGrid grid)
        {
            CharacterInventory enemy = enemyLootUI?.ActiveInventory;

            return enemy != null && grid != null &&
                   (ReferenceEquals(grid, enemy.Pockets) ||
                    ReferenceEquals(grid, enemy.RigInventory) ||
                    ReferenceEquals(grid, enemy.BackpackInventory));
        }


        private EquipmentLoadout GetEquipmentOwner(EquipmentSlotUI slot)
        {
            if (slot == null) return null;

            if (equipmentPanel != null && character != null &&
                slot.transform.IsChildOf(equipmentPanel.transform))
                return character.Equipment;

            if (enemyLootUI != null && enemyLootUI.IsLootOpen &&
                enemyLootUI.EnemyEquipmentPanel != null &&
                slot.transform.IsChildOf(enemyLootUI.EnemyEquipmentPanel.transform))
                return enemyLootUI.ActiveInventory?.Equipment;

            return null;
        }

        private bool TryDropFromGrid(InventoryGridView origin, ItemInstance item)
        {
            if (groundView?.Grid == null || origin?.Grid == null || !IsPlayerGrid(origin.Grid))
                return false;

            return InventoryTransfer.TryAutoTransfer(origin.Grid, groundView.Grid, item);
        }

        private bool TryDropFromSlot(EquipmentSlotUI slot)
        {
            if (character == null || groundView?.Grid == null || slot?.Item == null)
                return false;

            if (!ReferenceEquals(GetEquipmentOwner(slot), character.Equipment))
                return false;

            return character.Equipment.TryUnequipAuto(slot.SlotType, groundView.Grid);
        }

        private EquipmentSlotUI GetHoveredSlot()
        {
            EquipmentSlotUI enemySlot = enemyLootUI?.EnemyEquipmentPanel?.GetSlotAt(pointerPosition, pointerCamera);

            return enemySlot != null ? enemySlot : equipmentPanel?.GetSlotAt(pointerPosition, pointerCamera);
        }

        // DRAG & DROP

        public void BeginDrag(InventoryItemUI visual, InventoryGridView origin, PointerEventData eventData)
        {
            if (movingItem != null || visual?.Item == null || origin?.Grid == null) return;

            InventoryPlacement placement = null;

            foreach (InventoryPlacement current in origin.Grid.Placements)
            {
                if (!ReferenceEquals(current.Item, visual.Item)) continue;

                placement = current;
                break;
            }

            if (placement == null || !CreatePreview(visual.Item, visual.gameObject, eventData))
                return;

            sourceItemUI = visual;
            sourceGrid = origin;
            sourceSlot = null;
            rotated = placement.Rotated;

            discardZone?.SetVisible(IsPlayerGrid(origin.Grid));
            RefreshPreview();
        }

        public void BeginDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (movingItem != null || slot?.Item == null) return;

            EquipmentLoadout owner = GetEquipmentOwner(slot);

            if (owner == null || !ReferenceEquals(owner.GetItem(slot.SlotType), slot.Item))
                return;

            if (!CreatePreview(slot.Item, slot.gameObject, eventData)) return;

            sourceItemUI = null;
            sourceGrid = null;
            sourceSlot = slot;
            rotated = false;

            discardZone?.SetVisible(ReferenceEquals(owner, character?.Equipment));
            RefreshPreview();
        }

        private bool CreatePreview(ItemInstance item, GameObject sourceObject, PointerEventData eventData)
        {
            if (item?.Definition == null || dragLayer == null || itemPrefab == null) return false;

            movingItem = item;
            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;

            preview = Instantiate(itemPrefab, dragLayer, false);
            preview.Setup(item, null, this);

            previewRect = preview.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);
            previewRect.localScale = Vector3.one;
            previewRect.SetAsLastSibling();

            CanvasGroup previewGroup = preview.GetComponent<CanvasGroup>();
            if (previewGroup == null) previewGroup = preview.gameObject.AddComponent<CanvasGroup>();

            previewGroup.blocksRaycasts = false;
            previewGroup.interactable = false;
            previewGroup.alpha = 0.85f;

            previewBackground = preview.GetComponent<Image>();

            sourceCanvasGroup = sourceObject.GetComponent<CanvasGroup>();
            if (sourceCanvasGroup == null) sourceCanvasGroup = sourceObject.AddComponent<CanvasGroup>();

            originalAlpha = sourceCanvasGroup.alpha;
            sourceCanvasGroup.alpha = 0.3f;

            return true;
        }

        public void UpdateDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceItemUI, visual)) return;
            UpdatePointer(eventData);
        }

        public void UpdateDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceSlot, slot)) return;
            UpdatePointer(eventData);
        }

        private void UpdatePointer(PointerEventData eventData)
        {
            pointerPosition = eventData.position;
            pointerCamera = eventData.pressEventCamera;
            RefreshPreview();
        }

        public void EndDrag(InventoryItemUI visual, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceItemUI, visual)) return;
            CompleteDrag(eventData);
        }

        public void EndDragFromSlot(EquipmentSlotUI slot, PointerEventData eventData)
        {
            if (!ReferenceEquals(sourceSlot, slot)) return;
            CompleteDrag(eventData);
        }


        private void CompleteDrag(PointerEventData eventData)
        {
            UpdatePointer(eventData);

            bool success = false;

            if (discardZone != null && discardZone.Contains(pointerPosition, pointerCamera))
            {
                if (sourceGrid != null)
                    success = TryDropFromGrid(sourceGrid, movingItem);
                else if (sourceSlot != null)
                    success = TryDropFromSlot(sourceSlot);
            }
            else
            {
                EquipmentSlotUI targetSlot = GetHoveredSlot();

                if (targetSlot != null)
                {
                    if (CanDropOnSlot(targetSlot))
                    {
                        EquipmentLoadout destination = GetEquipmentOwner(targetSlot);

                        if (sourceGrid != null)
                        {
                            success = destination.TryEquipOrSwap(sourceGrid.Grid, movingItem, targetSlot.SlotType);
                        }
                        else
                        {
                            EquipmentLoadout source = GetEquipmentOwner(sourceSlot);

                            success = ReferenceEquals(source, destination)
                                ? source.TryMoveBetweenSlots(sourceSlot.SlotType, targetSlot.SlotType)
                                : source.TryTransferEquippedTo(destination, sourceSlot.SlotType, targetSlot.SlotType);
                        }
                    }
                }
                else
                {
                    InventoryGridView targetGrid = GetHoveredGrid(out Vector2Int position);

                    if (targetGrid != null && CanDropOnGrid(targetGrid, position))
                    {
                        if (sourceGrid != null)
                        {
                            success = ReferenceEquals(sourceGrid.Grid, targetGrid.Grid)
                                ? sourceGrid.Grid.TryMove(movingItem, position, rotated)
                                : InventoryTransfer.TryTransfer(sourceGrid.Grid, targetGrid.Grid, movingItem, position, rotated);

                            if (!success && targetGrid == groundView && !ReferenceEquals(sourceGrid.Grid, targetGrid.Grid))
                                success = InventoryTransfer.TryAutoTransfer(sourceGrid.Grid, targetGrid.Grid, movingItem);
                        }
                        else
                        {
                            EquipmentLoadout source = GetEquipmentOwner(sourceSlot);

                            if (source != null)
                            {
                                success = source.TryUnequip(sourceSlot.SlotType, targetGrid.Grid, position, rotated);

                                if (!success && targetGrid == groundView)
                                    success = source.TryUnequipAuto(sourceSlot.SlotType, targetGrid.Grid);
                            }
                        }
                    }
                }
            }

            FinishDrag(success);
        }

        private bool CanDropOnSlot(EquipmentSlotUI slot)
        {
            if (movingItem == null || slot == null) return false;

            EquipmentLoadout destination = GetEquipmentOwner(slot);
            if (destination == null) return false;

            if (sourceGrid != null)
                return destination.CanEquipOrSwap(sourceGrid.Grid, movingItem, slot.SlotType);

            EquipmentLoadout source = GetEquipmentOwner(sourceSlot);
            if (source == null) return false;

            return ReferenceEquals(source, destination)
                ? source.CanMoveBetweenSlots(sourceSlot.SlotType, slot.SlotType)
                : source.CanTransferEquippedTo(destination, sourceSlot.SlotType, slot.SlotType);
        }


        private bool CanDropOnGrid(InventoryGridView grid, Vector2Int position)
        {
            if (grid?.Grid == null || movingItem == null) return false;

            EquipmentLoadout sourceEquipment = GetEquipmentOwner(sourceSlot);

            if (sourceGrid != null && ReferenceEquals(sourceGrid.Grid, grid.Grid))
                return grid.Grid.CanMove(movingItem, position, rotated);

            if (grid == groundView && grid.Grid.AutoExpandRows)
            {
                Vector2Int size = movingItem.Definition.GridSize;

                if (size.x > grid.Grid.Width && size.y > grid.Grid.Width)
                    return false;

                if (ReferenceEquals(movingItem.Contents, grid.Grid)) return false;

                return sourceGrid != null
                    ? sourceGrid.Grid.Contains(movingItem)
                    : sourceEquipment != null && ReferenceEquals(sourceEquipment.GetItem(sourceSlot.SlotType), movingItem);
            }

            if (ReferenceEquals(movingItem.Contents, grid.Grid) ||
                !grid.Grid.CanPlace(movingItem, position, rotated))
                return false;

            if (sourceGrid != null)
                return sourceGrid.Grid.Contains(movingItem);

            return sourceEquipment != null &&
                   ReferenceEquals(sourceEquipment.GetItem(sourceSlot.SlotType), movingItem);
        }

        private void RefreshPreview()
        {
            if (movingItem == null || previewRect == null) return;

            bool overDiscard = discardZone != null &&
                               discardZone.Contains(pointerPosition, pointerCamera);

            if (overDiscard)
            {
                previewRect.sizeDelta = sourceGrid != null
                    ? sourceGrid.GetItemVisualSize(movingItem, rotated)
                    : ((RectTransform)sourceSlot.transform).rect.size;

                if (previewBackground != null)
                    previewBackground.color = new Color(0.9f, 0.3f, 0.15f, 0.9f);
            }
            else
            {
                EquipmentSlotUI targetSlot = GetHoveredSlot();
                bool valid;

                if (targetSlot != null)
                {
                    previewRect.sizeDelta = ((RectTransform)targetSlot.transform).rect.size;
                    valid = CanDropOnSlot(targetSlot);
                }
                else
                {
                    InventoryGridView targetGrid = GetHoveredGrid(out Vector2Int position);

                    if (targetGrid != null)
                    {
                        previewRect.sizeDelta = targetGrid.GetItemVisualSize(movingItem, rotated);
                        valid = CanDropOnGrid(targetGrid, position);
                    }
                    else
                    {
                        previewRect.sizeDelta = sourceGrid != null
                            ? sourceGrid.GetItemVisualSize(movingItem, rotated)
                            : ((RectTransform)sourceSlot.transform).rect.size;

                        valid = false;
                    }
                }

                if (previewBackground != null)
                {
                    previewBackground.color = valid
                        ? new Color(0.2f, 0.8f, 0.4f, 0.9f)
                        : new Color(0.9f, 0.25f, 0.25f, 0.9f);
                }
            }

            preview?.SetIconRotation(rotated);

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragLayer, pointerPosition, pointerCamera, out Vector3 worldPosition))
            {
                previewRect.position = worldPosition;
            }
        }


        private InventoryGridView GetHoveredGrid(out Vector2Int position)
        {
            if (movingItem != null)
            {
                if (handsView != null && handsView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return handsView;
                if (rigView != null && rigView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return rigView;
                if (backpackView != null && backpackView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return backpackView;

                if (enemyLootUI != null && enemyLootUI.IsLootOpen)
                {
                    InventoryGridView hands = enemyLootUI.EnemyHandsView;
                    InventoryGridView rig = enemyLootUI.EnemyRigView;
                    InventoryGridView backpack = enemyLootUI.EnemyBackpackView;

                    if (hands != null && hands.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return hands;
                    if (rig != null && rig.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return rig;
                    if (backpack != null && backpack.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return backpack;
                }

                if (groundView != null && groundView.TryGetDropPosition(pointerPosition, pointerCamera, movingItem, rotated, out position)) return groundView;
            }

            position = Vector2Int.zero;
            return null;
        }


        private void RefreshAll()
        {
            playerPanels?.Refresh();
            groundView?.Refresh();
            equipmentPanel?.Refresh();
            externalLootView?.Refresh();
            enemyLootUI?.RefreshActiveLootUI();
        }

        private void FinishDrag(bool success)
        {
            string itemName = movingItem?.Definition?.DisplayName ?? "Item";

            if (sourceCanvasGroup != null)
                sourceCanvasGroup.alpha = originalAlpha;

            if (preview != null)
                Destroy(preview.gameObject);

            discardZone?.SetVisible(false);

            movingItem = null;
            sourceItemUI = null;
            sourceGrid = null;
            sourceSlot = null;
            preview = null;
            previewRect = null;
            previewBackground = null;
            sourceCanvasGroup = null;

            RefreshAll();

            Debug.Log($"[Drag] {itemName}: {(success ? "Moved" : "Cancelled")}", this);
        }
    }
}