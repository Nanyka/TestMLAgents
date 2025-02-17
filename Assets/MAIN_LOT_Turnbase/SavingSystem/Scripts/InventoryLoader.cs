using System;
using System.Collections.Generic;
using GleyLocalization;
using Unity.Services.Economy.Model;
using UnityEngine;

namespace JumpeeIsland
{
    public class InventoryLoader : MonoBehaviour
    {
        private List<JIInventoryItem> m_Inventories;

        private void Start()
        {
            GameFlowManager.Instance.OnStartGame.AddListener(SetupBuildingMenu);
        }

        public void SetData(List<PlayersInventoryItem> inventories)
        {
            m_Inventories = new();

            foreach (var item in inventories)
                m_Inventories.Add(item.GetItemDefinition().CustomDataDeserializable.GetAs<JIInventoryItem>());
            
            // SavingSystemManager.Instance.OnSetUpBuildingMenus.Invoke(m_Inventories);
        }

        private void SetupBuildingMenu(long arg0)
        {
            SavingSystemManager.Instance.OnSetUpBuildingMenus.Invoke(m_Inventories);
        }

        public void SendInventoriesToBuildingMenu()
        {
            MainUI.Instance.OnBuyBuildingMenu.Invoke(m_Inventories);
        }

        public void SendInventoriesToCreatureMenu()
        {
            MainUI.Instance.OnShowCreatureMenu.Invoke(m_Inventories);
        }

        public IEnumerable<JIInventoryItem> GetInventoriesByType(InventoryType inventoryType)
        {
            return m_Inventories.FindAll(t => t.inventoryType == inventoryType);
        }
    }

    public class JIInventoryItem
    {
        public string inventoryName;
        public InventoryType inventoryType; // To decide which category this inventory item is
        public string spriteAddress;
        public string description = "Have no information about this entity";
        public string virtualPurchaseId; // How much does it cost to place this item in the game
        public List<string> skinAddress;
        public EntityData EntityData; // Just used in BattleMode to place creatures
        public List<string> skillsAddress;
        public List<string> statsAddress;
        public List<BuildingStats> buildingStats;
        public List<CreatureStats> creatureStats;
    }

    public enum InventoryType
    {
        None,
        Building,
        Creature,
        Resource,
        Tower,
        Trap,
        Decoration,
    }
}