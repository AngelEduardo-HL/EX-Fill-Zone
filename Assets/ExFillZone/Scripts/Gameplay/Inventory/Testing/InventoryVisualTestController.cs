using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class InventoryVisualTestController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private PlayerInventoryPanelsUI playerPanels;
        [SerializeField] private InventoryGridView groundView;

        [Header("Test Items")]
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;
        [SerializeField] private ContainerDefinition rigDefinition;
        [SerializeField] private ContainerDefinition backpackDefinition;

        private CharacterInventory character;
        private InventoryGrid ground;
        private ItemInstance rigItem;
        private ItemInstance backpackItem;

        private void Start()
        {
            if (playerPanels == null || groundView == null || rifleDefinition == null ||
                medkitDefinition == null || rigDefinition == null || backpackDefinition == null)
            {
                Debug.LogError("[VisualTest] Faltan referencias.", this);
                return;
            }

            character = GetComponent<CharacterInventory>();
            ground = new InventoryGrid(7, 13);

            bool rifleAdded = character.Pockets.TryAutoPlace(new ItemInstance(rifleDefinition));
            bool medkitAdded = character.Pockets.TryAutoPlace(new ItemInstance(medkitDefinition));

            ground.TryAutoPlace(new ItemInstance(rifleDefinition));

            for (int i = 0; i < 4; i++)
                ground.TryAutoPlace(new ItemInstance(medkitDefinition));

            rigItem = new ItemInstance(rigDefinition);
            backpackItem = new ItemInstance(backpackDefinition);

            bool rigFilled = rigItem.Contents.TryAutoPlace(new ItemInstance(medkitDefinition));
            bool backpackFilled = backpackItem.Contents.TryAutoPlace(new ItemInstance(rifleDefinition));

            bool rigOnGround = ground.TryAutoPlace(rigItem);
            bool backpackOnGround = ground.TryAutoPlace(backpackItem);

            if (!rigOnGround || !backpackOnGround)
            {
                Debug.LogError("[VisualTest] No hay espacio para el equipo de prueba.", this);
                return;
            }

            bool rigEquipped = character.Equipment.TryEquip(ground, rigItem, EquipmentSlotType.TacticalRig);
            bool backpackEquipped = character.Equipment.TryEquip(ground, backpackItem, EquipmentSlotType.Backpack);

            playerPanels.Bind(character);
            groundView.ShowGrid(ground);

            Debug.Log($"[VisualTest] Hands items: {rifleAdded && medkitAdded}", this);
            Debug.Log($"[VisualTest] Rig filled: {rigFilled}, equipped: {rigEquipped}", this);
            Debug.Log($"[VisualTest] Backpack filled: {backpackFilled}, equipped: {backpackEquipped}", this);
            Debug.Log("[VisualTest] F7: Rig | F8: Backpack", this);
        }

        private void Update()
        {
            if (character == null || ground == null) return;

            if (Input.GetKeyDown(KeyCode.F7))
                ToggleEquipment(EquipmentSlotType.TacticalRig, rigItem);

            if (Input.GetKeyDown(KeyCode.F8))
                ToggleEquipment(EquipmentSlotType.Backpack, backpackItem);
        }

        private void ToggleEquipment(EquipmentSlotType slot, ItemInstance item)
        {
            bool equipped = ReferenceEquals(character.Equipment.GetItem(slot), item);

            bool success = equipped
                ? character.Equipment.TryUnequipAuto(slot, ground)
                : character.Equipment.TryEquip(ground, item, slot);

            if (!success)
            {
                Debug.LogWarning($"[VisualTest] No se pudo cambiar {slot}.", this);
                return;
            }

            playerPanels.Refresh();
            groundView.Refresh();

            bool nowEquipped = ReferenceEquals(character.Equipment.GetItem(slot), item);

            Debug.Log($"[VisualTest] {slot} equipped: {nowEquipped}", this);
            Debug.Log($"[VisualTest] {slot} contents preserved: {item.Contents.Placements.Count == 1}", this);
        }
    }
}