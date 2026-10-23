using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class PlayerEquipmentPanelUI : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentSlotUI helmetSlot;
        [SerializeField] private EquipmentSlotUI bodyArmorSlot;
        [SerializeField] private EquipmentSlotUI tacticalRigSlot;
        [SerializeField] private EquipmentSlotUI backpackSlot;
        [SerializeField] private EquipmentSlotUI primaryWeaponSlot;
        [SerializeField] private EquipmentSlotUI secondaryWeaponSlot;

        [Header("Weight")]
        [SerializeField] private Slider weightSlider;
        [SerializeField] private TMP_Text weightText;
        [SerializeField] private TMP_Text weightStateText;
        [SerializeField, Min(1f)] private float displayMaxWeight = 40f;

        private CharacterInventory inventory;

        public void Bind(CharacterInventory character)
        {
            inventory = character;

            if (weightSlider != null)
            {
                weightSlider.minValue = 0f;
                weightSlider.maxValue = 1f;
                weightSlider.interactable = false;
            }

            Refresh();
        }

        public void Refresh()
        {
            if (inventory == null) return;

            EquipmentLoadout equipment = inventory.Equipment;

            helmetSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.Helmet));
            bodyArmorSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.BodyArmor));
            tacticalRigSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.TacticalRig));
            backpackSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.Backpack));
            primaryWeaponSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.PrimaryWeapon));
            secondaryWeaponSlot?.ShowItem(equipment.GetItem(EquipmentSlotType.SecondaryWeapon));

            RefreshWeight();
        }

        private void RefreshWeight()
        {
            float weight = inventory.TotalWeight;
            float ratio = weight / Mathf.Max(1f, displayMaxWeight);

            if (weightSlider != null)
                weightSlider.SetValueWithoutNotify(Mathf.Clamp01(ratio));

            if (weightText != null)
                weightText.text = $"{weight:0.0} KG";

            if (weightStateText != null)
            {
                weightStateText.text = ratio < 0.25f ? "LIGHTEST" :
                                       ratio < 0.50f ? "LIGHT" :
                                       ratio < 0.75f ? "MEDIUM" :
                                       ratio < 1f ? "HEAVY" : "OVERLOADED";
            }
        }
    }
}