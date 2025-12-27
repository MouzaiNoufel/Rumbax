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
    /// Challenge types available in the game.
    /// </summary>
    public enum ChallengeType
    {
        KillEnemies,
        MergeDefenders,
        CompleteWaves,
        CompleteLevels,
        EarnCoins,
        SpendCoins,
        WatchAds,
        PlayTime,
        ReachDefenderLevel,
        WinWithoutDamage,
        UseSpecialAbility
    }

    /// <summary>
    /// Represents a single challenge.
    /// </summary>
    [System.Serializable]
    public class Challenge
    {
        public string Id;
        public ChallengeType Type;
        public string Title;
        public string Description;
        public int TargetValue;
        public int CurrentProgress;
        public int CoinReward;
        public int GemReward;
        public bool IsCompleted;
        public bool IsRewardClaimed;
    }

    /// <summary>
    /// Manages daily challenges and streak rewards.
    /// </summary>
    public class DailyChallengeService : MonoBehaviour
    {
        [Header("Challenge Settings")]
        [SerializeField] private int _challengesPerDay = 3;
        [SerializeField] private int _streakBonusMultiplier = 10;
        [SerializeField] private int _maxStreakBonus = 100;

        private PlayerData _playerData;
        private ISaveService _saveService;
        private ICurrencyService _currencyService;
        private IAnalyticsService _analyticsService;

        private List<Challenge> _dailyChallenges = new List<Challenge>();
        private DateTime _lastRefreshDate;
        private int _currentStreak;

        // Challenge templates for generation
        private readonly List<(ChallengeType type, string title, string descFormat, int minTarget, int maxTarget, int baseCoins, int baseGems)> _challengeTemplates = new List<(ChallengeType, string, string, int, int, int, int)>
        {
            (ChallengeType.KillEnemies, "Enemy Slayer", "Defeat {0} enemies", 20, 100, 100, 5),
            (ChallengeType.MergeDefenders, "Merge Master", "Merge defenders {0} times", 5, 20, 150, 8),
            (ChallengeType.CompleteWaves, "Wave Crusher", "Complete {0} waves", 5, 15, 200, 10),
            (ChallengeType.CompleteLevels, "Level Champion", "Complete {0} levels", 1, 5, 300, 15),
            (ChallengeType.EarnCoins, "Coin Collector", "Earn {0} coins", 500, 2000, 100, 5),
            (ChallengeType.SpendCoins, "Big Spender", "Spend {0} coins", 300, 1000, 150, 8),
            (ChallengeType.WatchAds, "Ad Watcher", "Watch {0} rewarded ads", 1, 5, 50, 20),
            (ChallengeType.PlayTime, "Dedicated Player", "Play for {0} minutes", 10, 30, 100, 5),
            (ChallengeType.ReachDefenderLevel, "High Tier", "Reach defender level {0}", 5, 10, 250, 12),
            (ChallengeType.WinWithoutDamage, "Perfect Defense", "Complete a wave without taking damage", 1, 1, 500, 25),
        };

        private void Awake()
        {
            _saveService = ServiceLocator.Get<ISaveService>();
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            _analyticsService = ServiceLocator.Get<IAnalyticsService>();
        }

        private void Start()
        {
            LoadChallenges();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<DefenderMergedEvent>(OnDefenderMerged);
            EventBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<AdWatchedEvent>(OnAdWatched);
        }

        private void LoadChallenges()
        {
            _playerData = _saveService.GetPlayerData();

            if (_playerData.DailyProgress == null)
            {
                _playerData.DailyProgress = new DailyProgressData
                {
                    CurrentStreak = 0,
                    LastCompletionDate = DateTime.MinValue
                };
            }

            _lastRefreshDate = _playerData.DailyProgress.LastCompletionDate;
            _currentStreak = _playerData.DailyProgress.CurrentStreak;

            if (ShouldRefreshChallenges())
            {
                GenerateNewChallenges();
            }
            else
            {
                LoadSavedChallenges();
            }
        }

        private bool ShouldRefreshChallenges()
        {
            DateTime now = DateTime.UtcNow.Date;
            DateTime lastRefresh = _lastRefreshDate.Date;
            
            return now > lastRefresh;
        }

        private void GenerateNewChallenges()
        {
            _dailyChallenges.Clear();

            // Check streak
            DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
            if (_lastRefreshDate.Date == yesterday)
            {
                _currentStreak++;
                Debug.Log($"[DailyChallenge] Streak increased to {_currentStreak}");
            }
            else if (_lastRefreshDate.Date < yesterday)
            {
                _currentStreak = 0;
                Debug.Log("[DailyChallenge] Streak reset");
            }

            // Select random challenges
            List<int> usedIndices = new List<int>();
            System.Random random = new System.Random(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Year * 1000);

            for (int i = 0; i < _challengesPerDay; i++)
            {
                int templateIndex;
                do
                {
                    templateIndex = random.Next(_challengeTemplates.Count);
                } while (usedIndices.Contains(templateIndex));

                usedIndices.Add(templateIndex);

                var template = _challengeTemplates[templateIndex];
                int target = random.Next(template.minTarget, template.maxTarget + 1);
                
                // Apply streak bonus to rewards
                int streakBonus = Mathf.Min(_currentStreak * _streakBonusMultiplier, _maxStreakBonus);
                int coinReward = template.baseCoins + (template.baseCoins * streakBonus / 100);
                int gemReward = template.baseGems + (template.baseGems * streakBonus / 100);

                Challenge challenge = new Challenge
                {
                    Id = $"daily_{DateTime.UtcNow:yyyyMMdd}_{i}",
                    Type = template.type,
                    Title = template.title,
                    Description = string.Format(template.descFormat, target),
                    TargetValue = target,
                    CurrentProgress = 0,
                    CoinReward = coinReward,
                    GemReward = gemReward,
                    IsCompleted = false,
                    IsRewardClaimed = false
                };

                _dailyChallenges.Add(challenge);
            }

            _lastRefreshDate = DateTime.UtcNow;
            SaveChallenges();

            Debug.Log($"[DailyChallenge] Generated {_challengesPerDay} new challenges");
            EventBus.Publish(new DailyChallengesRefreshedEvent { Challenges = _dailyChallenges });
        }

        private void LoadSavedChallenges()
        {
            // Load from player data
            string savedChallenges = PlayerPrefs.GetString("DailyChallenges", "");
            
            if (!string.IsNullOrEmpty(savedChallenges))
            {
                try
                {
                    ChallengeListWrapper wrapper = JsonUtility.FromJson<ChallengeListWrapper>(savedChallenges);
                    _dailyChallenges = wrapper.Challenges ?? new List<Challenge>();
                    Debug.Log($"[DailyChallenge] Loaded {_dailyChallenges.Count} saved challenges");
                }
                catch
                {
                    Debug.LogWarning("[DailyChallenge] Failed to load saved challenges, generating new ones");
                    GenerateNewChallenges();
                }
            }
            else
            {
                GenerateNewChallenges();
            }
        }

        private void SaveChallenges()
        {
            ChallengeListWrapper wrapper = new ChallengeListWrapper { Challenges = _dailyChallenges };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString("DailyChallenges", json);
            PlayerPrefs.Save();

            // Update player data
            _playerData.DailyProgress.CurrentStreak = _currentStreak;
            _playerData.DailyProgress.LastCompletionDate = _lastRefreshDate;
            _saveService.UpdatePlayerData(_playerData);
        }

        public List<Challenge> GetDailyChallenges()
        {
            return new List<Challenge>(_dailyChallenges);
        }

        public int GetCurrentStreak()
        {
            return _currentStreak;
        }

        public TimeSpan GetTimeUntilRefresh()
        {
            DateTime nextRefresh = DateTime.UtcNow.Date.AddDays(1);
            return nextRefresh - DateTime.UtcNow;
        }

        public bool ClaimReward(string challengeId)
        {
            Challenge challenge = _dailyChallenges.Find(c => c.Id == challengeId);
            
            if (challenge == null || !challenge.IsCompleted || challenge.IsRewardClaimed)
            {
                return false;
            }

            // Award rewards
            _currencyService.AddCoins(challenge.CoinReward);
            _currencyService.AddGems(challenge.GemReward);
            
            challenge.IsRewardClaimed = true;
            SaveChallenges();

            _analyticsService?.LogEvent("challenge_reward_claimed", new Dictionary<string, object>
            {
                { "challenge_type", challenge.Type.ToString() },
                { "coins", challenge.CoinReward },
                { "gems", challenge.GemReward }
            });

            Debug.Log($"[DailyChallenge] Claimed reward for {challenge.Title}: {challenge.CoinReward} coins, {challenge.GemReward} gems");
            
            EventBus.Publish(new ChallengeRewardClaimedEvent 
            { 
                ChallengeId = challengeId,
                Coins = challenge.CoinReward,
                Gems = challenge.GemReward
            });

            // Check if all challenges completed
            CheckAllChallengesCompleted();

            return true;
        }

        private void CheckAllChallengesCompleted()
        {
            bool allCompleted = true;
            bool allClaimed = true;

            foreach (var challenge in _dailyChallenges)
            {
                if (!challenge.IsCompleted) allCompleted = false;
                if (!challenge.IsRewardClaimed) allClaimed = false;
            }

            if (allCompleted && allClaimed)
            {
                // Bonus for completing all daily challenges
                int bonusCoins = 500;
                int bonusGems = 25;

                _currencyService.AddCoins(bonusCoins);
                _currencyService.AddGems(bonusGems);

                Debug.Log($"[DailyChallenge] All challenges completed! Bonus: {bonusCoins} coins, {bonusGems} gems");
                
                EventBus.Publish(new AllDailyChallengesCompletedEvent
                {
                    BonusCoins = bonusCoins,
                    BonusGems = bonusGems
                });
            }
        }

        private void UpdateChallengeProgress(ChallengeType type, int amount)
        {
            foreach (var challenge in _dailyChallenges)
            {
                if (challenge.Type != type || challenge.IsCompleted) continue;

                challenge.CurrentProgress += amount;

                if (challenge.CurrentProgress >= challenge.TargetValue)
                {
                    challenge.CurrentProgress = challenge.TargetValue;
                    challenge.IsCompleted = true;
                    
                    Debug.Log($"[DailyChallenge] Challenge completed: {challenge.Title}");
                    
                    EventBus.Publish(new ChallengeCompletedEvent 
                    { 
                        ChallengeId = challenge.Id,
                        Title = challenge.Title
                    });
                }

                SaveChallenges();
            }
        }

        // Event handlers
        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            UpdateChallengeProgress(ChallengeType.KillEnemies, 1);
        }

        private void OnDefenderMerged(DefenderMergedEvent evt)
        {
            UpdateChallengeProgress(ChallengeType.MergeDefenders, 1);
            UpdateChallengeProgress(ChallengeType.ReachDefenderLevel, evt.NewLevel);
        }

        private void OnWaveCompleted(WaveCompletedEvent evt)
        {
            UpdateChallengeProgress(ChallengeType.CompleteWaves, 1);
            
            if (evt.PerfectWave)
            {
                UpdateChallengeProgress(ChallengeType.WinWithoutDamage, 1);
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            UpdateChallengeProgress(ChallengeType.CompleteLevels, 1);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Type == Rumbax.Core.Events.CurrencyType.Coins)
            {
                if (evt.Delta > 0)
                {
                    UpdateChallengeProgress(ChallengeType.EarnCoins, evt.Delta);
                }
                else
                {
                    UpdateChallengeProgress(ChallengeType.SpendCoins, -evt.Delta);
                }
            }
        }

        private void OnAdWatched(AdWatchedEvent evt)
        {
            UpdateChallengeProgress(ChallengeType.WatchAds, 1);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Unsubscribe<DefenderMergedEvent>(OnDefenderMerged);
            EventBus.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Unsubscribe<AdWatchedEvent>(OnAdWatched);
        }

        [System.Serializable]
        private class ChallengeListWrapper
        {
            public List<Challenge> Challenges;
        }
    }

    // Additional events for challenges
    public class DailyChallengesRefreshedEvent : Rumbax.Core.Events.IGameEvent
    {
        public List<Challenge> Challenges;
    }

    public class ChallengeCompletedEvent : Rumbax.Core.Events.IGameEvent
    {
        public string ChallengeId;
        public string Title;
    }

    public class ChallengeRewardClaimedEvent : Rumbax.Core.Events.IGameEvent
    {
        public string ChallengeId;
        public int Coins;
        public int Gems;
    }

    public class AllDailyChallengesCompletedEvent : Rumbax.Core.Events.IGameEvent
    {
        public int BonusCoins;
        public int BonusGems;
    }
}
