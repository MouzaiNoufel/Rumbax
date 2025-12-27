using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Gameplay.Grid;
using Rumbax.Gameplay.Defenders;
using Rumbax.Gameplay.Enemies;

namespace Rumbax.Gameplay
{
    /// <summary>
    /// Main gameplay controller that coordinates game systems.
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private DefenderSpawner defenderSpawner;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private EnemyManager enemyManager;
        
        [Header("Level Settings")]
        [SerializeField] private int levelNumber = 1;
        
        private int _score;
        private int _coinsEarned;
        private float _levelStartTime;
        private bool _levelComplete;
        
        public int Score => _score;
        public int CoinsEarned => _coinsEarned;
        public float LevelDuration => Time.time - _levelStartTime;
        
        // EventBus is now static, no field needed
        private ICurrencyService _currencyService;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            
            SubscribeToEvents();
            
            // Initialize level
            InitializeLevel();
        }

        private void ValidateReferences()
        {
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();
            if (defenderSpawner == null)
                defenderSpawner = FindObjectOfType<DefenderSpawner>();
            if (waveManager == null)
                waveManager = FindObjectOfType<WaveManager>();
            if (enemyManager == null)
                enemyManager = FindObjectOfType<EnemyManager>();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Subscribe<DefenderMergedEvent>(OnDefenderMerged);
            
            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted += OnLevelComplete;
                waveManager.OnPlayerDeath += OnPlayerDeath;
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Unsubscribe<DefenderMergedEvent>(OnDefenderMerged);
            
            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted -= OnLevelComplete;
                waveManager.OnPlayerDeath -= OnPlayerDeath;
            }
        }

        /// <summary>
        /// Initialize the level.
        /// </summary>
        private void InitializeLevel()
        {
            _score = 0;
            _coinsEarned = 0;
            _levelStartTime = Time.time;
            _levelComplete = false;
            
            // Log analytics
            ServiceLocator.Get<IAnalyticsService>()?.LogLevelStart(levelNumber);
            
            // Publish event
            EventBus.Publish(new LevelStartedEvent(levelNumber));
            
            GameManager.Instance?.ChangeState(GameState.Playing);
            
            Debug.Log($"[LevelController] Level {levelNumber} initialized");
        }

        /// <summary>
        /// Start the level (waves begin).
        /// </summary>
        public void StartLevel()
        {
            waveManager?.StartWaves();
        }

        /// <summary>
        /// Called when player spawns a defender.
        /// </summary>
        public void OnSpawnButtonPressed()
        {
            if (defenderSpawner != null && defenderSpawner.CanSpawn)
            {
                defenderSpawner.TrySpawn();
                ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("spawn");
            }
            else
            {
                ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("error");
            }
        }

        /// <summary>
        /// Handle enemy defeated event.
        /// </summary>
        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            _score += 10;
            _coinsEarned += (int)evt.CoinsDropped;
        }

        /// <summary>
        /// Handle wave completed event.
        /// </summary>
        private void OnWaveCompleted(WaveCompletedEvent evt)
        {
            // Bonus score for completing wave
            _score += 50 * evt.WaveNumber;
            
            // Wave completion bonus coins
            int waveBonus = 20 * evt.WaveNumber;
            _currencyService?.AddCoins(waveBonus);
            _coinsEarned += waveBonus;
        }

        /// <summary>
        /// Handle defender merged event.
        /// </summary>
        private void OnDefenderMerged(DefenderMergedEvent evt)
        {
            // Score bonus for merging
            _score += 5 * evt.NewLevel;
        }

        /// <summary>
        /// Handle level complete.
        /// </summary>
        private void OnLevelComplete()
        {
            if (_levelComplete) return;
            _levelComplete = true;
            
            // Calculate stars
            int stars = CalculateStars();
            
            // Award completion bonus
            var gameManager = GameManager.Instance;
            int bonusCoins = gameManager?.Config?.BaseWinCoins ?? 100;
            int bonusGems = gameManager?.Config?.BaseWinGems ?? 1;
            
            // Apply subscription multiplier
            if (ServiceLocator.TryGet<ISubscriptionService>(out var subService) && 
                subService.HasBenefit(SubscriptionBenefit.FasterProgression))
            {
                bonusCoins = (int)(bonusCoins * 1.5f);
            }
            
            _currencyService?.AddCoins(bonusCoins);
            _currencyService?.AddGems(bonusGems);
            _coinsEarned += bonusCoins;
            
            // Save progress
            SaveLevelProgress(stars);
            
            // Log analytics
            ServiceLocator.Get<IAnalyticsService>()?.LogLevelComplete(
                levelNumber, _score, LevelDuration);
            
            // Publish event
            EventBus.Publish(new LevelCompletedEvent(levelNumber, stars, _score, _coinsEarned));
            
            Debug.Log($"[LevelController] Level {levelNumber} complete! Score: {_score}, Stars: {stars}");
        }

        /// <summary>
        /// Calculate stars based on performance.
        /// </summary>
        private int CalculateStars()
        {
            float healthPercent = waveManager?.PlayerHealthPercent ?? 1f;
            
            if (healthPercent >= 0.75f) return 3;
            if (healthPercent >= 0.4f) return 2;
            return 1;
        }

        /// <summary>
        /// Save level progress.
        /// </summary>
        private void SaveLevelProgress(int stars)
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var data = saveService?.GetPlayerData();
            
            if (data != null)
            {
                // Update level progress
                var levelProgress = data.LevelProgress.Find(p => p.LevelNumber == levelNumber);
                
                if (levelProgress == null)
                {
                    levelProgress = new Data.LevelProgressData { LevelNumber = levelNumber };
                    data.LevelProgress.Add(levelProgress);
                }
                
                levelProgress.IsCompleted = true;
                levelProgress.TimesPlayed++;
                levelProgress.TimesCompleted++;
                levelProgress.Stars = Mathf.Max(levelProgress.Stars, stars);
                levelProgress.HighScore = System.Math.Max(levelProgress.HighScore, _score);
                levelProgress.BestTime = levelProgress.BestTime > 0 ? 
                    Mathf.Min(levelProgress.BestTime, LevelDuration) : LevelDuration;
                
                // Update player stats
                data.TotalScore += _score;
                data.HighScore = System.Math.Max(data.HighScore, _score);
                data.TotalStars += stars;
                data.HighestLevel = Mathf.Max(data.HighestLevel, levelNumber + 1);
                data.Statistics.TotalWins++;
                
                saveService.SaveGame();
            }
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void OnPlayerDeath()
        {
            // Log analytics
            ServiceLocator.Get<IAnalyticsService>()?.LogLevelFail(
                levelNumber, "Player health depleted");
            
            // Update stats
            var saveService = ServiceLocator.Get<ISaveService>();
            var data = saveService?.GetPlayerData();
            
            if (data != null)
            {
                data.Statistics.TotalLosses++;
                data.Statistics.MaxWaveReached = Mathf.Max(
                    data.Statistics.MaxWaveReached, 
                    waveManager?.CurrentWave ?? 0);
                
                saveService.SaveGame();
            }
        }

        /// <summary>
        /// Restart the level.
        /// </summary>
        public void RestartLevel()
        {
            gridManager?.ClearGrid();
            waveManager?.Reset();
            defenderSpawner?.ResetSpawnCount();
            
            InitializeLevel();
            StartLevel();
        }

        /// <summary>
        /// Request revive (show rewarded ad).
        /// </summary>
        public void RequestRevive()
        {
            var adService = ServiceLocator.Get<IAdService>();
            
            if (adService?.IsRewardedAdReady == true)
            {
                adService.ShowRewardedAd(
                    onReward: () => {
                        waveManager?.RevivePlayer(0.5f);
                        ServiceLocator.Get<IAnalyticsService>()?.LogAdWatched("rewarded", "revive");
                    },
                    onFailed: () => {
                        Debug.Log("[LevelController] Revive ad failed");
                    }
                );
            }
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            GameManager.Instance?.PauseGame();
        }

        /// <summary>
        /// Resume the game.
        /// </summary>
        public void ResumeGame()
        {
            GameManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// Return to main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            ResumeGame();
            GameManager.Instance?.LoadScene("MainMenu");
        }
    }
}
