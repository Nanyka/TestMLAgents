using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace JumpeeIsland
{
    public enum GameMode
    {
        NONE,
        ECONOMY,
        BOSS,
        BATTLE,
        REPLAY
    }

    public class GameFlowManager : Singleton<GameFlowManager>
    {
        // [NonSerialized] public UnityEvent OnLoadData = new(); // send to SavingSystemManager
        [NonSerialized] public UnityEvent<long> OnStartGame = new(); // send to EnvironmentManager, invoke at SavingSystemManager
        [NonSerialized] public UnityEvent OnInitiateObjects = new(); // send to Managers; invoke from TileManager
        [NonSerialized] public UnityEvent<MovableTile> OnUpdateTilePos = new(); // send to EnvironmentManager; invoke at TileManager
        [NonSerialized] public UnityEvent<GameObject, FactionType> OnDomainRegister = new(); // send to EnvironmentManager; invoke at BuildingManager, ResourceManager, CreatureManager
        [NonSerialized] public UnityEvent<EntityData> OnSelectEntity = new(); // send to TutorialController; invoke at PlayerFactionController
        [NonSerialized] public UnityEvent OnKickOffEnv = new(); // send to EnvironmentManager; invoke at CreatureMenu in BATTLE MODE, at this script in ECO MODE
        [NonSerialized] public UnityEvent<int> OnGameOver = new(); // send to GameResultPanel, BattleMainUI, this; invoke at CountDownClock, CountDownStep, PlayerFactionController
        [NonSerialized] public UnityEvent OnKilledBoss = new(); // send to GameResultPanel; invoke at CreatureUnlockComp
        [NonSerialized] public UnityEvent OnOpenBattlePass = new(); // send to BattlePassSceneManager; invoke at BattleButton
        [NonSerialized] public UnityEvent<bool> OnChangeAutomationMode = new(); // sent to PlayerFactionController; invoke at 
        
        [FormerlySerializedAs("IsEcoMode")] public GameMode GameMode = GameMode.NONE;
        [SerializeField] public bool _isGameRunning;
        
        protected EnvironmentManager _environmentManager;
        protected TutorialController _tutorialController;
        protected GameSpawner _gameSpawner;
        protected GlobalVfx _globalVfx;

        protected override void Awake()
        {
            base.Awake();
            _environmentManager = FindObjectOfType<EnvironmentManager>();
            _tutorialController = FindObjectOfType<TutorialController>();
            _gameSpawner = FindObjectOfType<GameSpawner>();
            _globalVfx = GetComponent<GlobalVfx>();
            
            OnStartGame.AddListener(RecordStartedState);
            OnKickOffEnv.AddListener(ConfirmGameStarted);
            OnGameOver.AddListener(GameOverState);
        }

        protected virtual void Start()
        {
            SavingSystemManager.Instance.StartLoadData();
        }

        protected virtual void ConfirmGameStarted()
        {
            _isGameRunning = true;
        }

        private void RecordStartedState(long arg0)
        {
            if (GameMode == GameMode.ECONOMY)
            {
                _isGameRunning = true;
                OnKickOffEnv.Invoke();
            }
        }

        private void GameOverState(int delayInterval)
        {
            _isGameRunning = false;
        }

        public EnvironmentManager GetEnvManager()
        {
            return _environmentManager;
        }

        public void LoadTutorialManager(string currentTutorial)
        {
            _tutorialController.Init(currentTutorial);
        }

        public void AskGlobalVfx(GlobalVfxType vfxType, Vector3 atPos)
        {
            _globalVfx.PlayGlobalVfx(vfxType,atPos);
        }

        public void AskForShowingAttackPath(IEnumerable<Vector3> highlightPos)
        {
            _globalVfx.ShowAttackPath(highlightPos);
        }

        // Just use for QuestFlowManager in BossMode
        public virtual Quest GetQuest()
        {
            return null;
        }
        
        public virtual QuestData GetQuestData()
        {
            return null;
        }

        public bool CheckTierPass()
        {
            return _gameSpawner.CheckTierCondition();
        }
    }
}
