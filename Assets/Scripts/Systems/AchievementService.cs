using System;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Events;
using Rumbax.Core.Services;
using Rumbax.Data;

namespace Rumbax.Systems
{
    /// <summary>
    /// Achievement categories.
    /// </summary>
    public enum AchievementCategory
    {
        Combat,
        Progression,
        Collection,
        Economy,
        Social,
        Special
    }

    /// <summary>
    /// Represents an achievement definition.
    /// </summary>
    [System.Serializable]
    public class Achievement
    {
        public string Id;
        public string Title;
        public string Description;
        public AchievementCategory Category;
        public Sprite Icon;
        public int TargetValue;
        public int CurrentProgress;
        public int CoinReward;
        public int GemReward;
        public bool IsUnlocked;
        public bool IsRewardClaimed;
        public bool IsHidden;
        public DateTime UnlockDate;
    }

    /// <summary>
    /// Manages achievements and Google Play Games integration.
    /// </summary>
    public class AchievementService : MonoBehaviour, IAchievementService
    {
        [Header("Settings")]
        [SerializeField] private bool _useGooglePlayGames = true;

        // IAchievementService interface event
        public event Action<string> OnAchievementUnlocked;

        private Dictionary<string, Achievement> _achievements = new Dictionary<string, Achievement>();
        private PlayerData _playerData;
        private ISaveService _saveService;
        private ICurrencyService _currencyService;
        private IAnalyticsService _analyticsService;

        private bool _isAuthenticated;

        private void Awake()
        {
            ServiceLocator.Register<IAchievementService>(this);
        }

        private void Start()
        {
            _saveService = ServiceLocator.Get<ISaveService>();
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            _analyticsService = ServiceLocator.Get<IAnalyticsService>();

            InitializeAchievements();
            LoadProgress();
            SubscribeToEvents();
            
            if (_useGooglePlayGames)
            {
                InitializeGooglePlayGames();
            }
        }

        private void InitializeAchievements()
        {
            // Combat achievements
            AddAchievement("kill_100", "First Blood", "Defeat 100 enemies", AchievementCategory.Combat, 100, 200, 10);
            AddAchievement("kill_1000", "Enemy Hunter", "Defeat 1,000 enemies", AchievementCategory.Combat, 1000, 500, 25);
            AddAchievement("kill_10000", "Annihilator", "Defeat 10,000 enemies", AchievementCategory.Combat, 10000, 2000, 100);
            AddAchievement("kill_100000", "Legendary Warrior", "Defeat 100,000 enemies", AchievementCategory.Combat, 100000, 10000, 500, true);
            
            // Merge achievements
            AddAchievement("merge_50", "Beginner Merger", "Merge defenders 50 times", AchievementCategory.Combat, 50, 150, 8);
            AddAchievement("merge_500", "Expert Merger", "Merge defenders 500 times", AchievementCategory.Combat, 500, 800, 40);
            AddAchievement("merge_5000", "Master Merger", "Merge defenders 5,000 times", AchievementCategory.Combat, 5000, 4000, 200);
            
            // Max level defender
            AddAchievement("max_defender", "Maximum Power", "Reach defender level 10", AchievementCategory.Combat, 10, 1000, 50);
            
            // Progression achievements
            AddAchievement("complete_level_10", "Getting Started", "Complete level 10", AchievementCategory.Progression, 10, 300, 15);
            AddAchievement("complete_level_50", "Halfway There", "Complete level 50", AchievementCategory.Progression, 50, 1000, 50);
            AddAchievement("complete_level_100", "Century", "Complete level 100", AchievementCategory.Progression, 100, 3000, 150);
            AddAchievement("complete_level_500", "Veteran", "Complete level 500", AchievementCategory.Progression, 500, 10000, 500, true);
            
            // Wave achievements
            AddAchievement("waves_100", "Wave Rider", "Complete 100 waves", AchievementCategory.Progression, 100, 200, 10);
            AddAchievement("waves_1000", "Wave Master", "Complete 1,000 waves", AchievementCategory.Progression, 1000, 1000, 50);
            
            // Perfect waves
            AddAchievement("perfect_10", "Careful Player", "Complete 10 waves without damage", AchievementCategory.Progression, 10, 500, 25);
            AddAchievement("perfect_100", "Untouchable", "Complete 100 waves without damage", AchievementCategory.Progression, 100, 2500, 125);
            
            // Economy achievements
            AddAchievement("earn_10000", "Coin Starter", "Earn 10,000 coins total", AchievementCategory.Economy, 10000, 100, 5);
            AddAchievement("earn_100000", "Coin Collector", "Earn 100,000 coins total", AchievementCategory.Economy, 100000, 500, 25);
            AddAchievement("earn_1000000", "Millionaire", "Earn 1,000,000 coins total", AchievementCategory.Economy, 1000000, 2500, 125);
            
            AddAchievement("spend_50000", "Investor", "Spend 50,000 coins", AchievementCategory.Economy, 50000, 300, 15);
            AddAchievement("spend_500000", "Big Spender", "Spend 500,000 coins", AchievementCategory.Economy, 500000, 1500, 75);
            
            // Collection achievements
            AddAchievement("defenders_all", "Collector", "Unlock all defender types", AchievementCategory.Collection, 6, 2000, 100);
            
            // Social achievements
            AddAchievement("daily_7", "Week Warrior", "Complete daily challenges 7 days in a row", AchievementCategory.Social, 7, 1000, 50);
            AddAchievement("daily_30", "Monthly Champion", "Complete daily challenges 30 days in a row", AchievementCategory.Social, 30, 5000, 250);
            
            // Special achievements
            AddAchievement("first_purchase", "Supporter", "Make your first purchase", AchievementCategory.Special, 1, 500, 100);
            AddAchievement("subscriber", "VIP Member", "Subscribe to premium", AchievementCategory.Special, 1, 1000, 200);
            
            Debug.Log($"[Achievement] Initialized {_achievements.Count} achievements");
        }

        private void AddAchievement(string id, string title, string description, AchievementCategory category, 
            int targetValue, int coinReward, int gemReward, bool isHidden = false)
        {
            Achievement achievement = new Achievement
            {
                Id = id,
                Title = title,
                Description = description,
                Category = category,
                TargetValue = targetValue,
                CurrentProgress = 0,
                CoinReward = coinReward,
                GemReward = gemReward,
                IsUnlocked = false,
                IsRewardClaimed = false,
                IsHidden = isHidden
            };

            _achievements[id] = achievement;
        }

        private void LoadProgress()
        {
            _playerData = _saveService.LoadPlayerData();

            if (_playerData.Achievements == null)
            {
                _playerData.Achievements = new List<AchievementProgress>();
            }

            foreach (var progress in _playerData.Achievements)
            {
                if (_achievements.TryGetValue(progress.Id, out Achievement achievement))
                {
                    achievement.CurrentProgress = progress.Progress;
                    achievement.IsUnlocked = progress.IsUnlocked;
                    achievement.IsRewardClaimed = progress.IsRewardClaimed;
                    achievement.UnlockDate = progress.UnlockDate;
                }
            }

            // Update with current stats
            UpdateFromStats();
        }

        private void UpdateFromStats()
        {
            if (_playerData.Statistics == null) return;

            UpdateProgress("kill_100", _playerData.Statistics.TotalEnemiesKilled);
            UpdateProgress("kill_1000", _playerData.Statistics.TotalEnemiesKilled);
            UpdateProgress("kill_10000", _playerData.Statistics.TotalEnemiesKilled);
            UpdateProgress("kill_100000", _playerData.Statistics.TotalEnemiesKilled);

            UpdateProgress("waves_100", _playerData.Statistics.TotalWavesCompleted);
            UpdateProgress("waves_1000", _playerData.Statistics.TotalWavesCompleted);

            UpdateProgress("earn_10000", _playerData.Statistics.TotalCoinsEarned);
            UpdateProgress("earn_100000", _playerData.Statistics.TotalCoinsEarned);
            UpdateProgress("earn_1000000", _playerData.Statistics.TotalCoinsEarned);

            UpdateProgress("complete_level_10", _playerData.HighestLevel);
            UpdateProgress("complete_level_50", _playerData.HighestLevel);
            UpdateProgress("complete_level_100", _playerData.HighestLevel);
            UpdateProgress("complete_level_500", _playerData.HighestLevel);
        }

        private void SaveProgress()
        {
            _playerData.Achievements.Clear();

            foreach (var kvp in _achievements)
            {
                _playerData.Achievements.Add(new AchievementProgress
                {
                    Id = kvp.Key,
                    Progress = kvp.Value.CurrentProgress,
                    IsUnlocked = kvp.Value.IsUnlocked,
                    IsRewardClaimed = kvp.Value.IsRewardClaimed,
                    UnlockDate = kvp.Value.UnlockDate
                });
            }

            _saveService.SavePlayerData(_playerData);
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<DefenderMergedEvent>(OnDefenderMerged);
            EventBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
            EventBus.Subscribe<SubscriptionChangedEvent>(OnSubscriptionChanged);
            EventBus.Subscribe<AllDailyChallengesCompletedEvent>(OnAllDailyChallengesCompleted);
        }

        private void InitializeGooglePlayGames()
        {
            Debug.Log("[Achievement] Initializing Google Play Games...");

            // Actual implementation:
            /*
            var config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();
            
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
            
            Social.localUser.Authenticate(success => {
                _isAuthenticated = success;
                Debug.Log($"[Achievement] Google Play Games auth: {success}");
                
                if (success)
                {
                    SyncWithGooglePlay();
                }
            });
            */

            _isAuthenticated = true;
            Debug.Log("[Achievement] Google Play Games initialized (simulated)");
        }

        public void UpdateProgress(string achievementId, int progress)
        {
            if (!_achievements.TryGetValue(achievementId, out Achievement achievement))
            {
                return;
            }

            if (achievement.IsUnlocked) return;

            int previousProgress = achievement.CurrentProgress;
            achievement.CurrentProgress = Mathf.Max(achievement.CurrentProgress, progress);

            if (achievement.CurrentProgress >= achievement.TargetValue && !achievement.IsUnlocked)
            {
                UnlockAchievement(achievementId);
            }
            else if (achievement.CurrentProgress != previousProgress)
            {
                SaveProgress();
            }
        }

        public void IncrementProgress(string achievementId, int amount = 1)
        {
            if (!_achievements.TryGetValue(achievementId, out Achievement achievement))
            {
                return;
            }

            if (achievement.IsUnlocked) return;

            achievement.CurrentProgress += amount;

            if (achievement.CurrentProgress >= achievement.TargetValue)
            {
                UnlockAchievement(achievementId);
            }
            else
            {
                SaveProgress();
            }
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!_achievements.TryGetValue(achievementId, out Achievement achievement))
            {
                Debug.LogWarning($"[Achievement] Unknown achievement: {achievementId}");
                return;
            }

            if (achievement.IsUnlocked) return;

            achievement.IsUnlocked = true;
            achievement.UnlockDate = DateTime.UtcNow;
            achievement.CurrentProgress = achievement.TargetValue;

            SaveProgress();

            Debug.Log($"[Achievement] Unlocked: {achievement.Title}");

            // Report to Google Play Games
            if (_isAuthenticated && _useGooglePlayGames)
            {
                ReportToGooglePlay(achievementId);
            }

            // Log analytics
            _analyticsService?.LogEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "achievement_title", achievement.Title },
                { "category", achievement.Category.ToString() }
            });

            EventBus.Publish(new AchievementUnlockedEvent(achievementId, achievement.Title));
            OnAchievementUnlocked?.Invoke(achievementId);
        }

        private void ReportToGooglePlay(string achievementId)
        {
            Debug.Log($"[Achievement] Reporting to Google Play: {achievementId}");

            // Actual implementation:
            /*
            string gpgsId = GetGooglePlayAchievementId(achievementId);
            if (!string.IsNullOrEmpty(gpgsId))
            {
                Social.ReportProgress(gpgsId, 100.0, success => {
                    Debug.Log($"[Achievement] Google Play report: {success}");
                });
            }
            */
        }

        public bool ClaimReward(string achievementId)
        {
            if (!_achievements.TryGetValue(achievementId, out Achievement achievement))
            {
                return false;
            }

            if (!achievement.IsUnlocked || achievement.IsRewardClaimed)
            {
                return false;
            }

            _currencyService.AddCoins(achievement.CoinReward);
            _currencyService.AddGems(achievement.GemReward);

            achievement.IsRewardClaimed = true;
            SaveProgress();

            Debug.Log($"[Achievement] Claimed reward for {achievement.Title}: {achievement.CoinReward} coins, {achievement.GemReward} gems");

            _analyticsService?.LogEvent("achievement_reward_claimed", new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "coins", achievement.CoinReward },
                { "gems", achievement.GemReward }
            });

            EventBus.Publish(new AchievementRewardClaimedEvent(achievementId, achievement.CoinReward, achievement.GemReward));

            return true;
        }

        public Achievement GetAchievement(string achievementId)
        {
            _achievements.TryGetValue(achievementId, out Achievement achievement);
            return achievement;
        }

        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(_achievements.Values);
        }

        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
        {
            List<Achievement> result = new List<Achievement>();
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.Category == category)
                {
                    result.Add(achievement);
                }
            }
            return result;
        }

        public int GetUnlockedCount()
        {
            int count = 0;
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.IsUnlocked) count++;
            }
            return count;
        }

        public int GetTotalCount()
        {
            return _achievements.Count;
        }

        public int GetUnclaimedCount()
        {
            int count = 0;
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.IsUnlocked && !achievement.IsRewardClaimed) count++;
            }
            return count;
        }

        public void ShowAchievementsUI()
        {
            if (_isAuthenticated && _useGooglePlayGames)
            {
                Debug.Log("[Achievement] Showing Google Play achievements UI");
                // Social.ShowAchievementsUI();
            }
            else
            {
                Debug.Log("[Achievement] Google Play not available, show in-game UI");
                EventBus.Publish(new ShowAchievementsUIEvent());
            }
        }

        // IAchievementService interface implementations
        public void Initialize()
        {
            InitializeAchievements();
            LoadProgress();
            SubscribeToEvents();
            if (_useGooglePlayGames)
            {
                InitializeGooglePlayGames();
            }
        }

        public void IncrementAchievement(string achievementId, int steps)
        {
            IncrementProgress(achievementId, steps);
        }

        public void GetAchievements(Action<List<AchievementData>> callback)
        {
            var dataList = new List<AchievementData>();
            foreach (var kvp in _achievements)
            {
                var a = kvp.Value;
                dataList.Add(new AchievementData
                {
                    Id = a.Id,
                    Name = a.Title,
                    Description = a.Description,
                    IsUnlocked = a.IsUnlocked,
                    Progress = (float)a.CurrentProgress / a.TargetValue,
                    CurrentSteps = a.CurrentProgress,
                    TotalSteps = a.TargetValue
                });
            }
            callback?.Invoke(dataList);
        }

        public bool IsUnlocked(string achievementId)
        {
            if (_achievements.TryGetValue(achievementId, out var achievement))
            {
                return achievement.IsUnlocked;
            }
            return false;
        }

        // Event handlers
        private int _cumulativeKills;
        private int _cumulativeMerges;
        private int _perfectWaves;
        private int _consecutiveDailyCompletions;

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            _cumulativeKills++;
            UpdateProgress("kill_100", _cumulativeKills);
            UpdateProgress("kill_1000", _cumulativeKills);
            UpdateProgress("kill_10000", _cumulativeKills);
            UpdateProgress("kill_100000", _cumulativeKills);
        }

        private void OnDefenderMerged(DefenderMergedEvent evt)
        {
            _cumulativeMerges++;
            IncrementProgress("merge_50", 1);
            IncrementProgress("merge_500", 1);
            IncrementProgress("merge_5000", 1);

            if (evt.NewLevel >= 10)
            {
                UnlockAchievement("max_defender");
            }
        }

        private void OnWaveCompleted(WaveCompletedEvent evt)
        {
            if (evt.PerfectWave)
            {
                _perfectWaves++;
                UpdateProgress("perfect_10", _perfectWaves);
                UpdateProgress("perfect_100", _perfectWaves);
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            UpdateProgress("complete_level_10", evt.Level);
            UpdateProgress("complete_level_50", evt.Level);
            UpdateProgress("complete_level_100", evt.Level);
            UpdateProgress("complete_level_500", evt.Level);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == CurrencyType.Coins)
            {
                if (evt.Amount > 0)
                {
                    IncrementProgress("earn_10000", evt.Amount);
                    IncrementProgress("earn_100000", evt.Amount);
                    IncrementProgress("earn_1000000", evt.Amount);
                }
                else
                {
                    IncrementProgress("spend_50000", -evt.Amount);
                    IncrementProgress("spend_500000", -evt.Amount);
                }
            }
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent evt)
        {
            UnlockAchievement("first_purchase");
        }

        private void OnSubscriptionChanged(SubscriptionChangedEvent evt)
        {
            if (evt.IsActive)
            {
                UnlockAchievement("subscriber");
            }
        }

        private void OnAllDailyChallengesCompleted(AllDailyChallengesCompletedEvent evt)
        {
            _consecutiveDailyCompletions++;
            UpdateProgress("daily_7", _consecutiveDailyCompletions);
            UpdateProgress("daily_30", _consecutiveDailyCompletions);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<DefenderMergedEvent>(OnDefenderMerged);
            EventBus.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
            EventBus.Unsubscribe<SubscriptionChangedEvent>(OnSubscriptionChanged);
            EventBus.Unsubscribe<AllDailyChallengesCompletedEvent>(OnAllDailyChallengesCompleted);
        }
    }

    // Achievement events (using Rumbax.Core.Events.AchievementUnlockedEvent)
    public class AchievementRewardClaimedEvent : IGameEvent
    {
        public string AchievementId { get; }
        public int Coins { get; }
        public int Gems { get; }

        public AchievementRewardClaimedEvent(string id, int coins, int gems)
        {
            AchievementId = id;
            Coins = coins;
            Gems = gems;
        }
    }

    public class ShowAchievementsUIEvent : IGameEvent { }
}
