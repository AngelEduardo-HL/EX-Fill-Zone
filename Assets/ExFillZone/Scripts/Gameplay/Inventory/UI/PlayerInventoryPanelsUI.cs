using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class PlayerInventoryPanelsUI : MonoBehaviour
    {
        [Header("Inventory Grids")]
        [SerializeField] private InventoryGridView handsView;
        [SerializeField] private InventoryGridView rigView;
        [SerializeField] private InventoryGridView backpackView;

        [Header("Layout")]
        [SerializeField] private RectTransform contentPlayer;

        private CharacterInventory inventory;

        public void Bind(CharacterInventory character)
        {
            inventory = character;
            Refresh();
        }

        public void Refresh()
        {
            if (inventory == null) return;

            if (handsView == null || rigView == null || backpackView == null)
            {
                Debug.LogError("[PlayerInventoryUI] Faltan referencias de cuadrículas.", this);
                return;
            }

            handsView.ShowGrid(inventory.Pockets);
            rigView.ShowGrid(inventory.RigInventory);
            backpackView.ShowGrid(inventory.BackpackInventory);

            if (contentPlayer != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(contentPlayer);
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}