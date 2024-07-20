using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;

namespace JumpeeIsland
{
    public class JIEconomyManager : MonoBehaviour
    {
        public static JIEconomyManager instance { get; private set; }

        // Dictionary of all Virtual Purchase transactions ids to lists of costs & rewards.
        public Dictionary<string, (List<JIItemAndAmountSpec> costs, List<JIItemAndAmountSpec> rewards)>
            virtualPurchaseTransactions { get; private set; } = new();

        private List<CurrencyDefinition> currencyDefinitions { get; set; }
        private List<PlayerBalance> _playerBalances;
        private List<InventoryItemDefinition> inventoryItemDefinitions { get; set; }
        private List<PlayersInventoryItem> _playersInventory = new();

        private int k_EconomyPurchaseCostsNotMetStatusCode = 10504;
        private List<VirtualPurchaseDefinition> m_VirtualPurchaseDefinitions;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public async Task RefreshEconomyConfiguration()
        {
            // Calling SyncConfigurationAsync(), will update the cached configuration list (the lists of Currency,
            // Inventory Item, and Purchase definitions) with any definitions that have been published or changed by
            // Economy or overriden by Game Overrides since the last time the player's configuration was cached. It also
            // ensures that other services like Cloud Code are working with the same configuration that has been cached.
            await EconomyService.Instance.Configuration.SyncConfigurationAsync();

            // Check that scene has not been unloaded while processing async wait to prevent throw.
            if (this == null)
                return;

            currencyDefinitions = EconomyService.Instance.Configuration.GetCurrencies();
            inventoryItemDefinitions = EconomyService.Instance.Configuration.GetInventoryItems();
            m_VirtualPurchaseDefinitions = EconomyService.Instance.Configuration.GetVirtualPurchases();

            InitializeVirtualPurchaseLookup();
        }

        #region CURRENCY

        public string GetSpriteAddress(string currencyId)
        {
            var currencyData = currencyDefinitions.Find(t => t.Id.Equals(currencyId));
            if (currencyData == null)
                return null;

            return currencyData.CustomDataDeserializable.GetAs<CurrencyCustomData>().spriteAddress;
        }

        public async Task<List<PlayerBalance>> RefreshCurrencyBalances()
        {
            GetBalancesResult balanceResult = null;

            try
            {
                balanceResult = await GetEconomyBalances();
            }
            catch (EconomyRateLimitedException e)
            {
                balanceResult = await JIUtils.RetryEconomyFunction(GetEconomyBalances, e.RetryAfter);
            }
            catch (Exception e)
            {
                Debug.Log("Problem getting Economy currency balances:");
                Debug.LogException(e);
            }

            if (this == null || balanceResult == null)
                return null;

            _playerBalances = balanceResult.Balances;
            return _playerBalances;
        }

        public List<PlayerBalance> GetBalances()
        {
            return _playerBalances;
        }

        public async Task InGameRefreshCurrencyBalances()
        {
            GetBalancesResult balanceResult = null;

            try
            {
                balanceResult = await GetEconomyBalances();
            }
            catch (EconomyRateLimitedException e)
            {
                balanceResult = await JIUtils.RetryEconomyFunction(GetEconomyBalances, e.RetryAfter);
            }
            catch (Exception e)
            {
                Debug.Log("Problem getting Economy currency balances:");
                Debug.LogException(e);
            }

            // Check that scene has not been unloaded while processing async wait to prevent throw.
            if (this == null) return;
        }

        static Task<GetBalancesResult> GetEconomyBalances()
        {
            var options = new GetBalancesOptions { ItemsPerFetch = 100 };
            return EconomyService.Instance.PlayerBalances.GetBalancesAsync(options);
        }

        // Add currency when sell a building
        private async Task GrantCurrency(string currencyId, int amount)
        {
            try
            {
                await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(currencyId, amount);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task OnGrantCurrency(string currencyId, int amount)
        {
            await GrantCurrency(currencyId, amount);
        }

        public async Task DeductCurrency(string currencyId, int amount)
        {
            try
            {
                await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(currencyId, amount);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task OnSetCurrency(string currencyId, int amount)
        {
            try
            {
                await EconomyService.Instance.PlayerBalances.SetBalanceAsync(currencyId, amount);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private class CurrencyCustomData
        {
            public string spriteAddress;
        }

        #endregion

        #region INVENTORY

        public List<PlayersInventoryItem> GetPlayerInventory()
        {
            return _playersInventory;
        }

        public async Task<List<PlayersInventoryItem>> RefreshInventory()
        {
            GetInventoryResult inventoryResult = null;

            try
            {
                inventoryResult = await LoadPlayerInventory();
            }
            catch (EconomyRateLimitedException e)
            {
                inventoryResult = await JIUtils.RetryEconomyFunction(LoadPlayerInventory, e.RetryAfter);
            }
            catch (Exception e)
            {
                Debug.Log("Problem getting Economy inventory items:");
                Debug.LogException(e);
            }

            if (this == null || inventoryResult == null)
                return null;

            _playersInventory = inventoryResult.PlayersInventoryItems;

            return _playersInventory;
        }
        
        private Task<GetInventoryResult> LoadPlayerInventory()
        {
            var options = new GetInventoryOptions { ItemsPerFetch = 100 };
            return EconomyService.Instance.PlayerInventory.GetInventoryAsync(options);
        }

        public List<InventoryItemDefinition> GetInventoryDefinitions()
        {
            return inventoryItemDefinitions;
        }

        public async Task ClearInventory()
        {
            if (_playersInventory == null || _playersInventory.Count == 0)
                return;

            foreach (var inventory in _playersInventory)
            {
                await EconomyService.Instance.PlayerInventory.DeletePlayersInventoryItemAsync(inventory
                    .PlayersInventoryItemId);
            }

            await RefreshInventory();
        }

        // This method is used to help test this Use Case sample by giving some currency to permit
        // transactions to be completed.
        private async Task<PlayersInventoryItem> GrantDebugInventory(string inventoryId)
        {
            try
            {
                return await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync(inventoryId);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private async Task GrantDebugInventory(string inventoryId, int level)
        {
            try
            {
                var option = new AddInventoryItemOptions { InstanceData = new InventoryInstanceData(level) };
                await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync(inventoryId, option);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task<PlayersInventoryItem> OnGrantInventory(string inventoryId)
        {
            foreach (var inventory in _playersInventory)
                if (inventory.InventoryItemId.Equals(inventoryId))
                    return null;

            return await GrantDebugInventory(inventoryId);
        }

        public async Task OnGrantInventory(string inventoryId, int level)
        {
            foreach (var inventory in _playersInventory)
                if (inventory.InventoryItemId.Equals(inventoryId))
                    return;

            await GrantDebugInventory(inventoryId, level);
        }

        public async Task OnUpdatePlayerInventory(string itemId, int level)
        {
            foreach (var item in _playersInventory)
            {
                if (item.InventoryItemId.Equals(itemId) || item.GetItemDefinition().Name.Equals(itemId))
                {
                    await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(
                        item.PlayersInventoryItemId, new InventoryInstanceData(level));
                }
            }
        }

        public int GetInventoryLevel(string inventoryId)
        {
            int returnLevel = 0;
            var selectedInventory = _playersInventory.Find(t => t.InventoryItemId.Equals(inventoryId)) ??
                                    _playersInventory.Find(t => t.GetItemDefinition().Name.Equals(inventoryId));

            if (selectedInventory == null) return returnLevel;

            var instanceData = selectedInventory.InstanceData.GetAs<InventoryInstanceData>();
            if (instanceData != null)
                returnLevel = instanceData.level;
            return returnLevel;
        }

        #endregion

        #region VIRTUAL PURCHASE

        public void InitializeVirtualPurchaseLookup()
        {
            if (m_VirtualPurchaseDefinitions == null)
            {
                return;
            }

            virtualPurchaseTransactions = new Dictionary<string,
                (List<JIItemAndAmountSpec> costs, List<JIItemAndAmountSpec> rewards)>();

            foreach (var virtualPurchaseDefinition in m_VirtualPurchaseDefinitions)
            {
                var costs = ParseEconomyItems(virtualPurchaseDefinition.Costs);
                var rewards = ParseEconomyItems(virtualPurchaseDefinition.Rewards);

                virtualPurchaseTransactions[virtualPurchaseDefinition.Id] = (costs, rewards);
            }
        }

        List<JIItemAndAmountSpec> ParseEconomyItems(List<PurchaseItemQuantity> itemQuantities)
        {
            var itemsAndAmountsSpec = new List<JIItemAndAmountSpec>();

            foreach (var itemQuantity in itemQuantities)
            {
                var id = itemQuantity.Item.GetReferencedConfigurationItem().Id;
                itemsAndAmountsSpec.Add(new JIItemAndAmountSpec(id, itemQuantity.Amount));
            }

            return itemsAndAmountsSpec;
        }

        public async Task<MakeVirtualPurchaseResult> MakeVirtualPurchaseAsync(string virtualPurchaseId)
        {
            try
            {
                var costs = virtualPurchaseTransactions[virtualPurchaseId].costs;
                foreach (var cost in costs)
                    if (SavingSystemManager.Instance.CheckEnoughCurrency(cost.id, cost.amount) == false)
                        return null;

                return await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(virtualPurchaseId);
            }
            catch (EconomyException e)
                when (e.ErrorCode == k_EconomyPurchaseCostsNotMetStatusCode)
            {
                // Rethrow purchase-cost-not-met exception to be handled by shops manager.
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }

        public List<JIItemAndAmountSpec> GetVirtualPurchaseCost(string virtualPurchaseId)
        {
            return virtualPurchaseTransactions[virtualPurchaseId].costs;
        }

        public List<JIItemAndAmountSpec> GetVirtualPurchaseReward(string virtualPurchaseId)
        {
            return virtualPurchaseTransactions[virtualPurchaseId].rewards;
        }

        public VirtualPurchaseDefinition GetPurchaseDefinition(string id)
        {
            return m_VirtualPurchaseDefinitions.Find(t => t.Id == id);
        }

        public List<VirtualPurchaseDefinition> GetPurchasedDefinitions()
        {
            return m_VirtualPurchaseDefinitions;
        }

        #endregion

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }

    public class InventoryInstanceData
    {
        public int level;

        public InventoryInstanceData(int assignLevel)
        {
            level = assignLevel;
        }
    }
}