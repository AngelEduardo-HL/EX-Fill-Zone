
using DG.Tweening;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [DisallowMultipleComponent]
    public sealed class PlayerWeaponSelectionUI : MonoBehaviour
    {
        [Header("Weapon Panels")]
        [SerializeField] private RectTransform primaryWeaponPanel;
        [SerializeField] private RectTransform secondaryWeaponPanel;

        [Header("Position")]
        [SerializeField] private Vector2 backOffset = new Vector2(35f, 35f);
        [SerializeField, Range(0.5f, 1f)] private float backScale = 0.94f;
        [SerializeField, Range(0f, 1f)] private float backAlpha = 0.5f;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private CanvasGroup primaryGroup;
        private CanvasGroup secondaryGroup;

        private Vector2 frontPosition;
        private int selectedSlot;

        public int SelectedSlot => selectedSlot;

        private void Awake()
        {
            if (primaryWeaponPanel == null || secondaryWeaponPanel == null)
            {
                Debug.LogError("[WeaponSelectionUI] Faltan referencias.", this);
                enabled = false;
                return;
            }

            frontPosition = primaryWeaponPanel.anchoredPosition;

            primaryGroup = GetOrAddCanvasGroup(primaryWeaponPanel);
            secondaryGroup = GetOrAddCanvasGroup(secondaryWeaponPanel);

            SetImmediate(1);
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
            if (slot != 1 && slot != 2) return;
            if (selectedSlot == slot) return;

            selectedSlot = slot;

            primaryWeaponPanel.DOKill();
            secondaryWeaponPanel.DOKill();
            primaryGroup.DOKill();
            secondaryGroup.DOKill();

            bool primaryActive = slot == 1;

            RectTransform activePanel = primaryActive ? primaryWeaponPanel : secondaryWeaponPanel;
            activePanel.SetAsLastSibling();

            AnimatePanel(primaryWeaponPanel, primaryGroup, primaryActive);
            AnimatePanel(secondaryWeaponPanel, secondaryGroup, !primaryActive);
        }

        private void AnimatePanel(RectTransform panel, CanvasGroup group, bool active)
        {
            Vector2 position = active ? frontPosition : frontPosition + backOffset;
            float scale = active ? 1f : backScale;
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

            primaryWeaponPanel.localScale = Vector3.one * (primaryActive ? 1f : backScale);
            secondaryWeaponPanel.localScale = Vector3.one * (primaryActive ? backScale : 1f);

            primaryGroup.alpha = primaryActive ? 1f : backAlpha;
            secondaryGroup.alpha = primaryActive ? backAlpha : 1f;

            (primaryActive ? primaryWeaponPanel : secondaryWeaponPanel).SetAsLastSibling();
        }

        private CanvasGroup GetOrAddCanvasGroup(RectTransform panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            return group != null ? group : panel.gameObject.AddComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            if (primaryWeaponPanel != null) primaryWeaponPanel.DOKill();
            if (secondaryWeaponPanel != null) secondaryWeaponPanel.DOKill();

            primaryGroup?.DOKill();
            secondaryGroup?.DOKill();
        }
    }
}
