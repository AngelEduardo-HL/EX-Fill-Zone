using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class InventoryVisualTestController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private PlayerInventoryPanelsUI playerPanels;
        [SerializeField] private PlayerEquipmentPanelUI equipmentPanel;
        [SerializeField] private InventoryGridView groundView;
        [SerializeField] private InventoryDragController dragController;

        [Header("Test Items")]
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;
        [SerializeField] private ContainerDefinition rigDefinition;
        [SerializeField] private ContainerDefinition backpackDefinition;
        [SerializeField] private ArmorDefinition helmetDefinition;
        [SerializeField] private ArmorDefinition bodyArmorDefinition;

        private CharacterInventory character;
        private InventoryGrid ground;
        private ItemInstance rigItem;
        private ItemInstance backpackItem;
        private ItemInstance helmetItem;
        private ItemInstance bodyArmorItem;

        private void Start()
        {
            if (playerPanels == null || equipmentPanel == null || groundView == null ||
                dragController == null || rifleDefinition == null || medkitDefinition == null ||
                rigDefinition == null || backpackDefinition == null ||
                helmetDefinition == null || bodyArmorDefinition == null)
            {
                Debug.LogError("[VisualTest] Faltan referencias.", this);
                return;
            }

            character = GetComponent<CharacterInventory>();
            dragController.Bind(character);
            equipmentPanel.ConfigureDrag(dragController);
            ground = new InventoryGrid(7, 13);

            // Objetos independientes para Hands y Ground.
            bool handsReady = character.Pockets.TryAutoPlace(new ItemInstance(rifleDefinition)) &&
                              character.Pockets.TryAutoPlace(new ItemInstance(medkitDefinition));

            bool groundReady = ground.TryAutoPlace(new ItemInstance(rifleDefinition));

            for (int i = 0; i < 4; i++)
                groundReady &= ground.TryAutoPlace(new ItemInstance(medkitDefinition));

            // Contenedores y equipo de prueba.
            rigItem = new ItemInstance(rigDefinition);
            backpackItem = new ItemInstance(backpackDefinition);
            helmetItem = new ItemInstance(helmetDefinition);
            bodyArmorItem = new ItemInstance(bodyArmorDefinition);

            ItemInstance primary = new ItemInstance(rifleDefinition);
            ItemInstance secondary = new ItemInstance(rifleDefinition);

            bool rigFilled = rigItem.Contents.TryAutoPlace(new ItemInstance(medkitDefinition));
            bool backpackFilled = backpackItem.Contents.TryAutoPlace(new ItemInstance(rifleDefinition));

            groundReady &= ground.TryAutoPlace(rigItem);
            groundReady &= ground.TryAutoPlace(backpackItem);
            groundReady &= ground.TryAutoPlace(helmetItem);
            groundReady &= ground.TryAutoPlace(bodyArmorItem);
            groundReady &= ground.TryAutoPlace(primary);
            groundReady &= ground.TryAutoPlace(secondary);

            if (!handsReady || !groundReady || !rigFilled || !backpackFilled)
            {
                Debug.LogError("[VisualTest] No se pudo preparar el inventario de prueba.", this);
                return;
            }

            EquipmentLoadout equipment = character.Equipment;

            bool rigEquipped = equipment.TryEquip(ground, rigItem, EquipmentSlotType.TacticalRig);
            bool backpackEquipped = equipment.TryEquip(ground, backpackItem, EquipmentSlotType.Backpack);
            bool helmetEquipped = equipment.TryEquip(ground, helmetItem, EquipmentSlotType.Helmet);
            bool armorEquipped = equipment.TryEquip(ground, bodyArmorItem, EquipmentSlotType.BodyArmor);
            bool primaryEquipped = equipment.TryEquip(ground, primary, EquipmentSlotType.PrimaryWeapon);
            bool secondaryEquipped = equipment.TryEquip(ground, secondary, EquipmentSlotType.SecondaryWeapon);

            playerPanels.Bind(character);
            equipmentPanel.Bind(character);
            groundView.ShowGrid(ground);

            Debug.Log($"[VisualTest] Rig: {rigEquipped}, Backpack: {backpackEquipped}", this);
            Debug.Log($"[VisualTest] Helmet: {helmetEquipped}, BodyArmor: {armorEquipped}", this);
            Debug.Log($"[VisualTest] Primary: {primaryEquipped}, Secondary: {secondaryEquipped}", this);
            Debug.Log($"[VisualTest] Weight: {character.TotalWeight:0.0} KG", this);
            Debug.Log("[VisualTest] F7: Rig | F8: Backpack | F9: Helmet | F10: BodyArmor", this);
        }

        private void Update()
        {
            if (character == null || ground == null) return;

            if (Input.GetKeyDown(KeyCode.F7))
                ToggleEquipment(EquipmentSlotType.TacticalRig, rigItem);

            if (Input.GetKeyDown(KeyCode.F8))
                ToggleEquipment(EquipmentSlotType.Backpack, backpackItem);

            if (Input.GetKeyDown(KeyCode.F9))
                ToggleEquipment(EquipmentSlotType.Helmet, helmetItem);

            if (Input.GetKeyDown(KeyCode.F10))
                ToggleEquipment(EquipmentSlotType.BodyArmor, bodyArmorItem);
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
            equipmentPanel.Refresh();
            groundView.Refresh();

            bool nowEquipped = ReferenceEquals(character.Equipment.GetItem(slot), item);

            Debug.Log($"[VisualTest] {slot} equipped: {nowEquipped}", this);
            Debug.Log($"[VisualTest] Weight: {character.TotalWeight:0.0} KG", this);
        }
    }
}