using DG.Tweening;
using ExFillZone.Gameplay.Inventory;
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

        private EnemyLootContainer enemyLoot;
        private PlayerInputReader playerInput;
        private bool playerInside;
        private bool interactionUsed;
        private bool promptVisible;
        private bool waitingForOpen;

        private void Start()
        {
            if (lootCrate == null) lootCrate = GetComponentInParent<LootCrate>();
            enemyLoot = GetComponentInParent<EnemyLootContainer>();

            if (interactionUI != null) interactionUI.alpha = 0f;
            if (prompt != null) prompt.localScale = Vector3.one * hiddenScale;
        }

        private void Update()
        {
            if (!playerInside || playerInput == null || lootCrate == null) return;

            if (enemyLoot != null)
            {
                if (waitingForOpen) return;

                if (InventoryMenuController.IsOpen)
                {
                    if (promptVisible) HidePrompt();
                    return;
                }

                if (!promptVisible) ShowPrompt();

                if (playerInput.InteractPressed) Interact();
                return;
            }

            if (interactionUsed || lootCrate.IsOpened) return;
            if (playerInput.InteractPressed) Interact();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (lootCrate == null) return;
            if (enemyLoot == null && (interactionUsed || lootCrate.IsOpened)) return;

            PlayerInputReader input = other.GetComponentInParent<PlayerInputReader>();
            if (input == null) return;

            playerInput = input;
            playerInside = true;

            if (!InventoryMenuController.IsOpen) ShowPrompt();
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
            if (enemyLoot != null)
            {
                if (!lootCrate.IsOpened)
                {
                    lootCrate.OpenCompleted += HandleOpenCompleted;
                    waitingForOpen = true;

                    if (!lootCrate.TryOpen())
                    {
                        lootCrate.OpenCompleted -= HandleOpenCompleted;
                        waitingForOpen = false;
                        return;
                    }

                    HidePrompt();
                    return;
                }

                if (enemyLoot.TryOpenInventory()) HidePrompt();
                return;
            }

            if (!lootCrate.TryOpen()) return;

            interactionUsed = true;

            interactionUI?.DOKill();
            prompt?.DOKill();

            if (prompt != null)
            {
                prompt.localScale = Vector3.one;
                prompt.DOPunchScale(Vector3.one * punchStrength, 0.18f, 5, 0.5f);
            }

            if (interactionUI != null)
                interactionUI.DOFade(0f, 0.18f).SetDelay(0.08f);

            promptVisible = false;
        }

        private void HandleOpenCompleted()
        {
            lootCrate.OpenCompleted -= HandleOpenCompleted;
            waitingForOpen = false;

            if (playerInside && enemyLoot != null && enemyLoot.TryOpenInventory())
                HidePrompt();
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

            promptVisible = true;
        }

        private void HidePrompt()
        {
            if (interactionUI == null || prompt == null) return;

            interactionUI.DOKill();
            prompt.DOKill();

            interactionUI.DOFade(0f, hideDuration);
            prompt.DOScale(hiddenScale, hideDuration).SetEase(Ease.InQuad);

            promptVisible = false;
        }

        private void OnDisable()
        {
            if (lootCrate != null) lootCrate.OpenCompleted -= HandleOpenCompleted;

            interactionUI?.DOKill();
            prompt?.DOKill();
        }
    }
}
