using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public sealed class InventoryVisualTestController : MonoBehaviour
    {
        [Header("Grid Views")]
        [SerializeField] private InventoryGridView handsView;
        [SerializeField] private InventoryGridView groundView;

        [Header("Test Items")]
        [SerializeField] private ItemDefinition rifleDefinition;
        [SerializeField] private ItemDefinition medkitDefinition;

        private void Start()
        {
            if (handsView == null || groundView == null || rifleDefinition == null || medkitDefinition == null)
            {
                Debug.LogError("[VisualTest] Faltan referencias.", this);
                return;
            }

            CharacterInventory character = GetComponent<CharacterInventory>();

            bool rifleAdded = character.Pockets.TryAutoPlace(new ItemInstance(rifleDefinition));
            bool medkitAdded = character.Pockets.TryAutoPlace(new ItemInstance(medkitDefinition));

            handsView.ShowGrid(character.Pockets);

            InventoryGrid ground = new InventoryGrid(7, 13);
            ground.TryAutoPlace(new ItemInstance(rifleDefinition));

            for (int i = 0; i < 4; i++)
                ground.TryAutoPlace(new ItemInstance(medkitDefinition));

            groundView.ShowGrid(ground);

            Debug.Log($"[VisualTest] Rifle: {rifleAdded}, Medkit: {medkitAdded}", this);
            Debug.Log($"[VisualTest] Hands: {character.Pockets.Width}x{character.Pockets.Height}", this);
            Debug.Log($"[VisualTest] Ground: {ground.Width}x{ground.Height}", this);
        }
    }
}