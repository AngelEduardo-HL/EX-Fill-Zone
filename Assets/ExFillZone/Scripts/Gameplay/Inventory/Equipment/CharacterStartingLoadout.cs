
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class CharacterStartingLoadout : MonoBehaviour
    {
        [Header("Armor")]
        [SerializeField] private ArmorDefinition helmet;
        [SerializeField] private ArmorDefinition bodyArmor;

        [Header("Containers")]
        [SerializeField] private ContainerDefinition tacticalRig;
        [SerializeField] private ContainerDefinition backpack;

        [Header("Weapons")]
        [SerializeField] private ItemDefinition primaryWeapon;
        [SerializeField] private ItemDefinition secondaryWeapon;

        [Header("Pocket Items")]
        [SerializeField] private ItemDefinition[] pocketItems;

        private void Start()
        {
            CharacterInventory inventory = GetComponent<CharacterInventory>();

            Equip(inventory, helmet, EquipmentSlotType.Helmet);
            Equip(inventory, bodyArmor, EquipmentSlotType.BodyArmor);
            Equip(inventory, tacticalRig, EquipmentSlotType.TacticalRig);
            Equip(inventory, backpack, EquipmentSlotType.Backpack);
            Equip(inventory, primaryWeapon, EquipmentSlotType.PrimaryWeapon);
            Equip(inventory, secondaryWeapon, EquipmentSlotType.SecondaryWeapon);

            if (pocketItems != null)
            {
                foreach (ItemDefinition definition in pocketItems)
                {
                    if (definition == null) continue;

                    if (!inventory.Pockets.TryAutoPlace(new ItemInstance(definition)))
                        Debug.LogWarning($"[Loadout] Sin espacio para {definition.DisplayName} en {name}.", this);
                }
            }

            Debug.Log($"[Loadout] {name} equipado. Valor: {inventory.TotalValue} | Peso: {inventory.TotalWeight:0.0} kg | Mitigación: {inventory.ArmorMitigation:P0}", this);
        }

        private void Equip(CharacterInventory inventory, ItemDefinition definition, EquipmentSlotType slot)
        {
            if (definition == null) return;

            if (!inventory.Equipment.TryEquipNew(definition, slot))
                Debug.LogWarning($"[Loadout] No se pudo equipar {definition.DisplayName} en {slot}.", this);
        }
    }
}
