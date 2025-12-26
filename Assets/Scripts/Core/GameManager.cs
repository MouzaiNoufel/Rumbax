using UnityEngine;
using UnityEngine.SceneManagement;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;

namespace Rumbax.Core
{
    /// <summary>
    /// Main game manager that orchestrates all game systems.
    /// Implements singleton pattern and persists across scenes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Game Configuration")]
        [SerializeField] private GameConfig gameConfig;
        
        public GameConfig Config => gameConfig;
        public GameState CurrentState { get; private set; } = GameState.Menu;
        public bool IsInitialized { get; private set; }
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Initialize all services in order
            ServiceLocator.Initialize();
            
            // Register core services
            ServiceLocator.Register<ISaveService>(new SaveService());
            ServiceLocator.Register<IEventBus>(new EventBus());
            ServiceLocator.Register<ICurrencyService>(new CurrencyService());
            ServiceLocator.Register<IAudioService>(GetComponent<AudioService>() ?? gameObject.AddComponent<AudioService>());
            
            // Load saved data
            ServiceLocator.Get<ISaveService>().LoadGame();
            
            // Calculate offline earnings
            CalculateOfflineEarnings();
            
            IsInitialized = true;
            
            Debug.Log("[GameManager] Game initialized successfully");
        }

        private void CalculateOfflineEarnings()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService.GetPlayerData();
            
            if (playerData == null) return;
            
            var lastPlayTime = playerData.LastPlayTime;
            var currentTime = System.DateTime.UtcNow;
            var offlineTime = currentTime - lastPlayTime;
            
            // Cap offline time to configured maximum (default 8 hours)
            var maxOfflineHours = gameConfig != null ? gameConfig.MaxOfflineHours : 8;
            if (offlineTime.TotalHours > maxOfflineHours)
            {
                offlineTime = System.TimeSpan.FromHours(maxOfflineHours);
            }
            
            // Calculate earnings based on player's production rate
            if (offlineTime.TotalMinutes >= 1)
            {
                long offlineCoins = (long)(playerData.CoinsPerMinute * offlineTime.TotalMinutes);
                if (offlineCoins > 0)
                {
                    ServiceLocator.Get<ICurrencyService>().AddCoins(offlineCoins);
                    ServiceLocator.Get<IEventBus>().Publish(new OfflineEarningsEvent(offlineCoins, offlineTime));
                }
            }
        }

        public void ChangeState(GameState newState)
        {
            var previousState = CurrentState;
            CurrentState = newState;
            ServiceLocator.Get<IEventBus>().Publish(new GameStateChangedEvent(previousState, newState));
        }

        public void PauseGame()
        {
            if (IsPaused) return;
            
            IsPaused = true;
            Time.timeScale = 0f;
            ServiceLocator.Get<IEventBus>().Publish(new GamePausedEvent(true));
        }

        public void ResumeGame()
        {
            if (!IsPaused) return;
            
            IsPaused = false;
            Time.timeScale = 1f;
            ServiceLocator.Get<IEventBus>().Publish(new GamePausedEvent(false));
        }

        public void LoadScene(string sceneName)
        {
            ServiceLocator.Get<ISaveService>().SaveGame();
            SceneManager.LoadScene(sceneName);
        }

        public void RestartLevel()
        {
            ResumeGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ServiceLocator.Get<ISaveService>()?.SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Get<ISaveService>()?.SaveGame();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ServiceLocator.Cleanup();
            }
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory,
        Loading
    }
}
