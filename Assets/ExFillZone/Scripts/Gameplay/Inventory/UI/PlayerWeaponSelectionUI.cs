
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    [DisallowMultipleComponent]
    public sealed class PlayerWeaponSelectionUI : MonoBehaviour
    {
        [Header("Weapon Panels")]
        [SerializeField] private RectTransform primaryWeaponPanel;
        [SerializeField] private RectTransform secondaryWeaponPanel;

        [Header("Primary Weapon UI")]
        [SerializeField] private Image primaryWeaponIcon;
        [SerializeField] private TMP_Text primaryWeaponName;

        [Header("Secondary Weapon UI")]
        [SerializeField] private Image secondaryWeaponIcon;
        [SerializeField] private TMP_Text secondaryWeaponName;

        [Header("Position")]
        [SerializeField] private Vector2 backOffset = new Vector2(35f, 35f);
        [SerializeField, Range(0.5f, 1f)] private float backScale = 0.94f;
        [SerializeField, Range(0f, 1f)] private float backAlpha = 0.5f;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private CharacterInventory inventory;
        private CanvasGroup primaryGroup;
        private CanvasGroup secondaryGroup;

        private Vector2 frontPosition;
        private Vector3 primaryOriginalScale;
        private Vector3 secondaryOriginalScale;
        private int selectedSlot = 1;

        public int SelectedSlot => selectedSlot;

        private void Awake()
        {
            if (primaryWeaponPanel == null || secondaryWeaponPanel == null)
            {
                Debug.LogError("[WeaponSelectionUI] Faltan referencias de paneles.", this);
                enabled = false;
                return;
            }

            frontPosition = primaryWeaponPanel.anchoredPosition;
            primaryOriginalScale = primaryWeaponPanel.localScale;
            secondaryOriginalScale = secondaryWeaponPanel.localScale;

            primaryGroup = GetOrAddCanvasGroup(primaryWeaponPanel);
            secondaryGroup = GetOrAddCanvasGroup(secondaryWeaponPanel);

            SetImmediate(1);
        }

        public void Bind(CharacterInventory character)
        {
            inventory = character;
            RefreshWeapons();
        }

        public void RefreshWeapons()
        {
            if (inventory == null) return;

            SetWeaponVisual(inventory.Equipment.GetItem(EquipmentSlotType.PrimaryWeapon), primaryWeaponIcon, primaryWeaponName);
            SetWeaponVisual(inventory.Equipment.GetItem(EquipmentSlotType.SecondaryWeapon), secondaryWeaponIcon, secondaryWeaponName);
        }

        private void SetWeaponVisual(ItemInstance item, Image icon, TMP_Text weaponName)
        {
            ItemDefinition definition = item?.Definition;

            if (icon != null)
            {
                icon.sprite = definition != null ? definition.Icon : null;
                icon.enabled = icon.sprite != null;
                icon.preserveAspect = true;
            }

            if (weaponName != null)
                weaponName.text = definition != null ? definition.DisplayName : "NO WEAPON";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                SelectWeapon(1);

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                SelectWeapon(2);
        }

        public void SelectWeapon(int slot)
        {
            if (slot != 1 && slot != 2 || selectedSlot == slot) return;

            selectedSlot = slot;
            KillTweens();

            bool primaryActive = slot == 1;

            (primaryActive ? primaryWeaponPanel : secondaryWeaponPanel).SetAsLastSibling();

            AnimatePanel(primaryWeaponPanel, primaryGroup, primaryOriginalScale, primaryActive);
            AnimatePanel(secondaryWeaponPanel, secondaryGroup, secondaryOriginalScale, !primaryActive);
        }

        private void AnimatePanel(RectTransform panel, CanvasGroup group, Vector3 originalScale, bool active)
        {
            Vector2 position = active ? frontPosition : frontPosition + backOffset;
            Vector3 scale = active ? originalScale : originalScale * backScale;
            float alpha = active ? 1f : backAlpha;

            panel.DOAnchorPos(position, duration).SetEase(ease).SetUpdate(true);
            panel.DOScale(scale, duration).SetEase(ease).SetUpdate(true);
            group.DOFade(alpha, duration).SetEase(ease).SetUpdate(true);
        }

        private void SetImmediate(int slot)
        {
            selectedSlot = slot;
            bool primaryActive = slot == 1;

            primaryWeaponPanel.anchoredPosition = primaryActive ? frontPosition : frontPosition + backOffset;
            secondaryWeaponPanel.anchoredPosition = primaryActive ? frontPosition + backOffset : frontPosition;

            primaryWeaponPanel.localScale = primaryOriginalScale * (primaryActive ? 1f : backScale);
            secondaryWeaponPanel.localScale = secondaryOriginalScale * (primaryActive ? backScale : 1f);

            primaryGroup.alpha = primaryActive ? 1f : backAlpha;
            secondaryGroup.alpha = primaryActive ? backAlpha : 1f;

            (primaryActive ? primaryWeaponPanel : secondaryWeaponPanel).SetAsLastSibling();
        }

        private CanvasGroup GetOrAddCanvasGroup(RectTransform panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            return group != null ? group : panel.gameObject.AddComponent<CanvasGroup>();
        }

        private void KillTweens()
        {
            primaryWeaponPanel?.DOKill();
            secondaryWeaponPanel?.DOKill();
            primaryGroup?.DOKill();
            secondaryGroup?.DOKill();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
