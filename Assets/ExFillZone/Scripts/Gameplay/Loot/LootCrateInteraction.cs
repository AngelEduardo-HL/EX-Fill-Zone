using DG.Tweening;
using ExFillZone.Gameplay.Player;
using UnityEngine;

namespace ExFillZone.Gameplay.Loot
{
    public sealed class LootCrateInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LootCrate lootCrate;
        [SerializeField] private CanvasGroup interactionUI;
        [SerializeField] private RectTransform prompt;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float showDuration = 0.15f;
        [SerializeField, Min(0.01f)] private float hideDuration = 0.12f;
        [SerializeField, Range(0.1f, 1f)] private float hiddenScale = 0.7f;
        [SerializeField, Min(0f)] private float punchStrength = 0.15f;

        private PlayerInputReader playerInput;
        private bool playerInside;
        private bool interactionUsed;

        private void Start()
        {
            if (lootCrate == null) lootCrate = GetComponentInParent<LootCrate>();

            if (interactionUI != null) interactionUI.alpha = 0f;
            if (prompt != null) prompt.localScale = Vector3.one * hiddenScale;
        }

        private void Update()
        {
            if (!playerInside || interactionUsed || playerInput == null || lootCrate == null || lootCrate.IsOpened) return;
            if (playerInput.InteractPressed) Interact();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (interactionUsed || lootCrate == null || lootCrate.IsOpened) return;

            PlayerInputReader input = other.GetComponentInParent<PlayerInputReader>();
            if (input == null) return;

            playerInput = input;
            playerInside = true;

            ShowPrompt();
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerInputReader input = other.GetComponentInParent<PlayerInputReader>();
            if (input == null || input != playerInput) return;

            playerInside = false;
            playerInput = null;

            HidePrompt();
        }

        private void Interact()
        {
            if (!lootCrate.TryOpen()) return;

            interactionUsed = true;

            interactionUI?.DOKill();
            prompt?.DOKill();

            if (prompt != null)
            {
                prompt.localScale = Vector3.one;
                prompt.DOPunchScale(Vector3.one * punchStrength, 0.18f, 5, 0.5f);
            }

            if (interactionUI != null) interactionUI.DOFade(0f, 0.18f).SetDelay(0.08f);
        }

        private void ShowPrompt()
        {
            if (interactionUI == null || prompt == null) return;

            interactionUI.DOKill();
            prompt.DOKill();

            interactionUI.alpha = 0f;
            prompt.localScale = Vector3.one * hiddenScale;

            interactionUI.DOFade(1f, showDuration);
            prompt.DOScale(1f, showDuration).SetEase(Ease.OutBack);
        }

        private void HidePrompt()
        {
            if (interactionUI == null || prompt == null) return;

            interactionUI.DOKill();
            prompt.DOKill();

            interactionUI.DOFade(0f, hideDuration);
            prompt.DOScale(hiddenScale, hideDuration).SetEase(Ease.InQuad);
        }

        private void OnDisable()
        {
            interactionUI?.DOKill();
            prompt?.DOKill();
        }
    }
}