using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(Image))]
    public sealed class InventoryCellUI : MonoBehaviour
    {
        [SerializeField] private Image background;

        private void Reset()
        {
            background = GetComponent<Image>();
        }

        public void SetHighlight(Color color)
        {
            if (background == null) background = GetComponent<Image>();
            background.color = color;
        }
    }
}