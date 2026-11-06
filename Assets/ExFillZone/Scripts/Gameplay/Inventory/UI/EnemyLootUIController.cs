using ExFillZone.Gameplay.Loot;
using UnityEngine;

namespace ExFillZone.Gameplay.Inventory
{
    public sealed class EnemyLootUIController : MonoBehaviour
    {
        [Header("Inventory Menu")]
        [SerializeField] private InventoryMenuController inventoryMenu;

        [Header("Panels")]
        [SerializeField] private GameObject groundPanel;
        [SerializeField] private GameObject enemyPanelRoot;

        [Header("Enemy Inventory")]
        [SerializeField] private PlayerInventoryPanelsUI enemyPanels;
        [SerializeField] private PlayerEquipmentPanelUI enemyEquipment;

        private EnemyLootContainer activeLoot;

        private void Awake()
        {
            if (enemyPanelRoot != null) enemyPanelRoot.SetActive(false);
        }

        private void LateUpdate()
        {
            if (activeLoot == null) return;

            if (inventoryMenu == null || !inventoryMenu.Open)
                CloseLoot();
        }

        public bool OpenLoot(EnemyLootContainer loot)
        {
            if (loot == null || !loot.Initialized || loot.Inventory == null)
                return false;

            if (inventoryMenu == null || groundPanel == null || enemyPanelRoot == null ||
                enemyPanels == null || enemyEquipment == null)
            {
                Debug.LogError("[EnemyLootUI] Faltan referencias en el Inspector.", this);
                return false;
            }

            activeLoot = loot;

            groundPanel.SetActive(false);
            enemyPanelRoot.SetActive(true);

            enemyPanels.Bind(loot.Inventory);
            enemyEquipment.Bind(loot.Inventory);

            inventoryMenu.SetOpen(true);

            Debug.Log($"[EnemyLootUI] Abriendo inventario de {loot.name}", this);
            return true;
        }

        public void CloseLoot()
        {
            activeLoot = null;

            if (enemyPanelRoot != null) enemyPanelRoot.SetActive(false);
            if (groundPanel != null) groundPanel.SetActive(true);
        }
    }
}
