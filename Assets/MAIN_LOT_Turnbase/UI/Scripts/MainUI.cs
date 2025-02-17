using System;
using System.Collections.Generic;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace JumpeeIsland
{
    public class MainUI : Singleton<MainUI>
    {
        [NonSerialized] public UnityEvent OnEnableInteract = new(); // send to DontMoveButton; invoke here in EcoMode and at DropTroopMenu in BattleMode

        [NonSerialized] public UnityEvent<IShowInfo> OnShowInfo = new(); // send to CreatureInfoUI; invoke at PlayerFactionController

        [NonSerialized] public UnityEvent<long> OnRemainStep = new(); // send to StepCounter; invoke at EnvironmentManager

        [NonSerialized] public UnityEvent OnUpdateCurrencies = new(); // send to CurrenciesInfo; invoke at SavingSystemManager

        [NonSerialized] public UnityEvent OnClickIdleButton = new(); // send to PlayerFactionManager; invoke at DontMoveButton & MovingPath

        [NonSerialized] public UnityEvent<List<JIInventoryItem>> OnBuyBuildingMenu = new(); // send to BuyBuildingMenu, invoke at InventoryLoader

        [NonSerialized] public UnityEvent<IConfirmFunction> OnInteractBuildingMenu = new(); // send to InteractBuildingMenu, invoke at BuildingController

        [NonSerialized] public UnityEvent<List<JIInventoryItem>> OnShowCreatureMenu = new(); // send to CreatureMenu, invoke at InventoryLoader

        [NonSerialized] public UnityEvent<List<VirtualPurchaseDefinition>> OnShowShoppingMenu = new(); // send to ShoppingMenu, invoke at JICloudConnector

        [NonSerialized] public UnityEvent OnShowBossSelector = new(); // send to BossSelector, invoke at ShowBossSelectorButton

        [NonSerialized] public UnityEvent<List<CreatureData>> OnShowDropTroopMenu = new(); // send to DropTroopMenu, invoke at ShowBattleCreatureMenu

        [NonSerialized] public UnityEvent<CreatureEntity> OnShowCreatureDetails = new(); // send to CreatureDetailsMenu, invoke at CreatureInfoUI

        [NonSerialized] public UnityEvent<Vector3, bool> OnSwitchButtonPointer = new(); // send to ButtonPointer, invoke at TutorialController

        [NonSerialized] public UnityEvent<string, bool> OnConversationUI = new(); // send to ConversationDialog, invoke at TutorialController
        
        [NonSerialized] public UnityEvent<string, string, bool> OnImageTutorial = new(); // send to ConversationDialog, invoke at TutorialController

        [NonSerialized] public UnityEvent OnHideAllMenu = new(); // send to BuildingMenu

        [NonSerialized] public UnityEvent OnShowAnUI = new(); // send to TutorialController, invoke at CreatureInfoUI, BossSelector;

        [NonSerialized] public UnityEvent<string, int, Vector3> OnShowCurrencyVfx = new(); // send to CollectCurrencyEffects; invoke at SavingSystemManager

        [NonSerialized] public UnityEvent<SelectionCircle> OnSelectDirection = new(); // send to MovingVisual

        [NonSerialized] public UnityEvent<int, string, bool> OnStarGuide = new(); // send to QuestInfoButton ;invoke at GameResultCalculator  

        public bool IsInteractable;
        public bool IsInRelocating;

        [SerializeField] private GameObject[] _panels;
        [SerializeField] private GameObject[] _buttons;

        private BuyBuildingMenu _buyBuildingMenu;
        protected CreatureMenu _creatureMenu;
        private CreatureInfoUI _creatureInfo;
        protected Camera _mainCamera;
        protected int _layerMask = 1 << 9 | 1 << 8 | 1 << 7;

        protected virtual void Start()
        {
            _buyBuildingMenu = GetComponent<BuyBuildingMenu>();
            _creatureMenu = GetComponent<CreatureMenu>();
            _creatureInfo = GetComponent<CreatureInfoUI>();
            _mainCamera = Camera.main;

            GameFlowManager.Instance.OnStartGame.AddListener(EnableInteract);
        }

        protected void EnableInteract(long arg0)
        {
            OnEnableInteract.Invoke();
            IsInteractable = true;
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (PointingChecker.IsPointerOverUIObject() || _buyBuildingMenu.IsInADeal() ||
                    _creatureMenu.IsInADeal() || IsInRelocating)
                    return;

                var moveRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(moveRay, out var moveHit, 100f, _layerMask))
                {
                    if (moveHit.collider.TryGetComponent(out SelectionCircle selectionCircle))
                        OnSelectDirection.Invoke(selectionCircle);
                        
                    return;
                }
                
                OnHideAllMenu.Invoke();
            }
        }

        public bool CheckUIActive(string UIName)
        {
            foreach (var uiElement in _panels)
                if (uiElement.name.Equals(UIName))
                    return uiElement.activeInHierarchy;

            return false;
        }

        public GameObject GetButtonPosition(string buttonName)
        {
            foreach (var button in _buttons)
            {
                if (button.name.Equals(buttonName))
                    return button;
            }

            return null;
        }

        public Entity GetSelectedEntity()
        {
            return _creatureInfo.GetSelectedEntity();
        }
    }
}