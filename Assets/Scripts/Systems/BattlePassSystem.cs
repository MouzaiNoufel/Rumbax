using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Battle Pass System - Seasonal rewards with free and premium tracks.
    /// Provides long-term engagement through leveled progression and exclusive rewards.
    /// </summary>
    public class BattlePassSystem : MonoBehaviour
    {
        public static BattlePassSystem Instance { get; private set; }

        // Events
        public event Action<int> OnLevelUp;
        public event Action<int, bool> OnRewardClaimed;
        public event Action<int> OnXPGained;
        public event Action OnPremiumUnlocked;

        // Constants
        private const int MAX_LEVEL = 50;
        private const int XP_PER_LEVEL_BASE = 100;
        private const int XP_INCREMENT = 25;
        private const int SEASON_DURATION_DAYS = 60;

        // State
        private int _currentLevel = 1;
        private int _currentXP;
        private bool _hasPremium;
        private List<bool> _freeRewardsClaimed = new List<bool>();
        private List<bool> _premiumRewardsClaimed = new List<bool>();
        private DateTime _seasonStartDate;
        private DateTime _seasonEndDate;
        private int _seasonNumber = 1;

        // Rewards data
        private List<BattlePassReward> _freeTrack = new List<BattlePassReward>();
        private List<BattlePassReward> _premiumTrack = new List<BattlePassReward>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSeasonDates();
            GenerateSeasonRewards();
            LoadProgress();
        }

        private void InitializeSeasonDates()
        {
            // Check for saved season data
            string savedStartDate = PlayerPrefs.GetString("BP_SeasonStart", "");
            if (!string.IsNullOrEmpty(savedStartDate))
            {
                _seasonStartDate = DateTime.Parse(savedStartDate);
                _seasonNumber = PlayerPrefs.GetInt("BP_SeasonNumber", 1);
            }
            else
            {
                // Start new season
                _seasonStartDate = DateTime.UtcNow;
                _seasonNumber = 1;
            }

            _seasonEndDate = _seasonStartDate.AddDays(SEASON_DURATION_DAYS);

            // Check if season expired
            if (DateTime.UtcNow > _seasonEndDate)
            {
                StartNewSeason();
            }
        }

        private void StartNewSeason()
        {
            _seasonNumber++;
            _seasonStartDate = DateTime.UtcNow;
            _seasonEndDate = _seasonStartDate.AddDays(SEASON_DURATION_DAYS);
            _currentLevel = 1;
            _currentXP = 0;
            _hasPremium = false;
            _freeRewardsClaimed.Clear();
            _premiumRewardsClaimed.Clear();

            GenerateSeasonRewards();
            SaveProgress();

            Debug.Log($"[BattlePass] Season {_seasonNumber} started!");
        }

        private void GenerateSeasonRewards()
        {
            _freeTrack.Clear();
            _premiumTrack.Clear();

            for (int level = 1; level <= MAX_LEVEL; level++)
            {
                _freeTrack.Add(GenerateFreeReward(level));
                _premiumTrack.Add(GeneratePremiumReward(level));
            }

            // Initialize claimed lists
            while (_freeRewardsClaimed.Count < MAX_LEVEL) _freeRewardsClaimed.Add(false);
            while (_premiumRewardsClaimed.Count < MAX_LEVEL) _premiumRewardsClaimed.Add(false);
        }

        private BattlePassReward GenerateFreeReward(int level)
        {
            // Every 5 levels = better rewards
            bool isMilestone = level % 5 == 0;
            bool isFinalMilestone = level % 10 == 0;

            if (level == MAX_LEVEL)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.HeroFragments,
                    amount = 30,
                    description = "Epic Hero Fragments",
                    icon = "🦸",
                    heroId = "epic_random"
                };
            }
            else if (isFinalMilestone)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.Gems,
                    amount = 50 + (level / 10) * 25,
                    description = "Gems",
                    icon = "💎"
                };
            }
            else if (isMilestone)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.LuckySpins,
                    amount = 2 + level / 10,
                    description = "Lucky Spins",
                    icon = "🎡"
                };
            }
            else if (level % 3 == 0)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.Coins,
                    amount = 500 + level * 50,
                    description = "Coins",
                    icon = "💰"
                };
            }
            else if (level % 2 == 0)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.XPBoost,
                    amount = 15 + level,
                    description = $"{15 + level} min XP Boost",
                    icon = "⚡"
                };
            }
            else
            {
                return new BattlePassReward
                {
                    type = BPRewardType.Coins,
                    amount = 200 + level * 20,
                    description = "Coins",
                    icon = "💰"
                };
            }
        }

        private BattlePassReward GeneratePremiumReward(int level)
        {
            bool isMilestone = level % 5 == 0;
            bool isFinalMilestone = level % 10 == 0;

            if (level == MAX_LEVEL)
            {
                // Season exclusive hero
                return new BattlePassReward
                {
                    type = BPRewardType.ExclusiveHero,
                    amount = 1,
                    description = $"Season {_seasonNumber} Exclusive Hero",
                    icon = "⭐",
                    heroId = $"season_{_seasonNumber}_hero",
                    isExclusive = true
                };
            }
            else if (isFinalMilestone)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.HeroFragments,
                    amount = 20 + level,
                    description = "Legendary Hero Fragments",
                    icon = "🦸",
                    heroId = "legendary_random"
                };
            }
            else if (isMilestone)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.ExclusiveSkin,
                    amount = 1,
                    description = "Exclusive Skin",
                    icon = "👔",
                    skinId = $"skin_s{_seasonNumber}_lv{level}",
                    isExclusive = true
                };
            }
            else if (level % 4 == 0)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.Gems,
                    amount = 30 + level * 2,
                    description = "Gems",
                    icon = "💎"
                };
            }
            else if (level % 3 == 0)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.PowerUp,
                    amount = 3 + level / 5,
                    description = "Premium Power-ups",
                    icon = "🔮"
                };
            }
            else if (level % 2 == 0)
            {
                return new BattlePassReward
                {
                    type = BPRewardType.Coins,
                    amount = 1000 + level * 100,
                    description = "Coins",
                    icon = "💰"
                };
            }
            else
            {
                return new BattlePassReward
                {
                    type = BPRewardType.LuckySpins,
                    amount = 1 + level / 15,
                    description = "Lucky Spins",
                    icon = "🎡"
                };
            }
        }

        // === PUBLIC API ===

        public void AddXP(int amount)
        {
            if (_currentLevel >= MAX_LEVEL) return;

            _currentXP += amount;
            OnXPGained?.Invoke(amount);

            while (_currentXP >= GetXPRequiredForLevel(_currentLevel) && _currentLevel < MAX_LEVEL)
            {
                _currentXP -= GetXPRequiredForLevel(_currentLevel);
                _currentLevel++;
                OnLevelUp?.Invoke(_currentLevel);
                Debug.Log($"[BattlePass] Level up! Now level {_currentLevel}");
            }

            SaveProgress();
        }

        public int GetXPRequiredForLevel(int level)
        {
            return XP_PER_LEVEL_BASE + (level - 1) * XP_INCREMENT;
        }

        public bool CanClaimFreeReward(int level)
        {
            int index = level - 1;
            return level <= _currentLevel && 
                   index >= 0 && index < _freeRewardsClaimed.Count && 
                   !_freeRewardsClaimed[index];
        }

        public bool CanClaimPremiumReward(int level)
        {
            int index = level - 1;
            return _hasPremium && 
                   level <= _currentLevel && 
                   index >= 0 && index < _premiumRewardsClaimed.Count && 
                   !_premiumRewardsClaimed[index];
        }

        public BattlePassReward ClaimFreeReward(int level)
        {
            if (!CanClaimFreeReward(level)) return null;

            int index = level - 1;
            _freeRewardsClaimed[index] = true;
            SaveProgress();

            OnRewardClaimed?.Invoke(level, false);
            return _freeTrack[index];
        }

        public BattlePassReward ClaimPremiumReward(int level)
        {
            if (!CanClaimPremiumReward(level)) return null;

            int index = level - 1;
            _premiumRewardsClaimed[index] = true;
            SaveProgress();

            OnRewardClaimed?.Invoke(level, true);
            return _premiumTrack[index];
        }

        public void ApplyReward(BattlePassReward reward, ref int coins, ref int gems)
        {
            switch (reward.type)
            {
                case BPRewardType.Coins:
                    coins += reward.amount;
                    break;
                case BPRewardType.Gems:
                    gems += reward.amount;
                    break;
                case BPRewardType.LuckySpins:
                    DailyRewardsSystem.Instance?.AddLuckySpins(reward.amount);
                    break;
                case BPRewardType.HeroFragments:
                    HeroSystem.Instance?.AddFragments(reward.heroId, reward.amount);
                    break;
                case BPRewardType.ExclusiveHero:
                    HeroSystem.Instance?.UnlockHero(reward.heroId);
                    break;
                case BPRewardType.XPBoost:
                    // Store XP boost minutes
                    int currentBoost = PlayerPrefs.GetInt("XPBoostMinutes", 0);
                    PlayerPrefs.SetInt("XPBoostMinutes", currentBoost + reward.amount);
                    break;
                case BPRewardType.PowerUp:
                    int powerUps = PlayerPrefs.GetInt("PremiumPowerUps", 0);
                    PlayerPrefs.SetInt("PremiumPowerUps", powerUps + reward.amount);
                    break;
                case BPRewardType.ExclusiveSkin:
                    PlayerPrefs.SetInt($"Skin_{reward.skinId}", 1);
                    break;
            }

            Debug.Log($"[BattlePass] Applied reward: {reward.description} x{reward.amount}");
        }

        public void UnlockPremium()
        {
            if (_hasPremium) return;

            _hasPremium = true;
            SaveProgress();
            OnPremiumUnlocked?.Invoke();

            Debug.Log("[BattlePass] Premium unlocked!");
        }

        public int GetClaimableFreeRewardsCount()
        {
            int count = 0;
            for (int i = 0; i < _currentLevel && i < _freeRewardsClaimed.Count; i++)
            {
                if (!_freeRewardsClaimed[i]) count++;
            }
            return count;
        }

        public int GetClaimablePremiumRewardsCount()
        {
            if (!_hasPremium) return 0;
            
            int count = 0;
            for (int i = 0; i < _currentLevel && i < _premiumRewardsClaimed.Count; i++)
            {
                if (!_premiumRewardsClaimed[i]) count++;
            }
            return count;
        }

        // === GETTERS ===

        public int CurrentLevel => _currentLevel;
        public int CurrentXP => _currentXP;
        public bool HasPremium => _hasPremium;
        public int SeasonNumber => _seasonNumber;
        public int MaxLevel => MAX_LEVEL;
        public float LevelProgress => (float)_currentXP / GetXPRequiredForLevel(_currentLevel);

        public TimeSpan GetTimeRemaining()
        {
            return _seasonEndDate - DateTime.UtcNow;
        }

        public string GetTimeRemainingString()
        {
            var remaining = GetTimeRemaining();
            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
            else if (remaining.TotalHours >= 1)
                return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            else
                return $"{remaining.Minutes}m";
        }

        public BattlePassReward GetFreeReward(int level)
        {
            int index = level - 1;
            if (index >= 0 && index < _freeTrack.Count)
                return _freeTrack[index];
            return null;
        }

        public BattlePassReward GetPremiumReward(int level)
        {
            int index = level - 1;
            if (index >= 0 && index < _premiumTrack.Count)
                return _premiumTrack[index];
            return null;
        }

        public bool IsFreeRewardClaimed(int level)
        {
            int index = level - 1;
            if (index >= 0 && index < _freeRewardsClaimed.Count)
                return _freeRewardsClaimed[index];
            return true;
        }

        public bool IsPremiumRewardClaimed(int level)
        {
            int index = level - 1;
            if (index >= 0 && index < _premiumRewardsClaimed.Count)
                return _premiumRewardsClaimed[index];
            return true;
        }

        // === PERSISTENCE ===

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("BP_Level", _currentLevel);
            PlayerPrefs.SetInt("BP_XP", _currentXP);
            PlayerPrefs.SetInt("BP_Premium", _hasPremium ? 1 : 0);
            PlayerPrefs.SetInt("BP_SeasonNumber", _seasonNumber);
            PlayerPrefs.SetString("BP_SeasonStart", _seasonStartDate.ToString("O"));

            // Save claimed rewards
            for (int i = 0; i < MAX_LEVEL; i++)
            {
                PlayerPrefs.SetInt($"BP_Free_{i}", _freeRewardsClaimed[i] ? 1 : 0);
                PlayerPrefs.SetInt($"BP_Premium_{i}", _premiumRewardsClaimed[i] ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _currentLevel = PlayerPrefs.GetInt("BP_Level", 1);
            _currentXP = PlayerPrefs.GetInt("BP_XP", 0);
            _hasPremium = PlayerPrefs.GetInt("BP_Premium", 0) == 1;

            _freeRewardsClaimed.Clear();
            _premiumRewardsClaimed.Clear();

            for (int i = 0; i < MAX_LEVEL; i++)
            {
                _freeRewardsClaimed.Add(PlayerPrefs.GetInt($"BP_Free_{i}", 0) == 1);
                _premiumRewardsClaimed.Add(PlayerPrefs.GetInt($"BP_Premium_{i}", 0) == 1);
            }
        }
    }

    // === DATA CLASSES ===

    public enum BPRewardType
    {
        Coins,
        Gems,
        HeroFragments,
        ExclusiveHero,
        ExclusiveSkin,
        LuckySpins,
        XPBoost,
        PowerUp,
        ExclusiveFrame,
        ExclusiveEmote
    }

    [Serializable]
    public class BattlePassReward
    {
        public BPRewardType type;
        public int amount;
        public string description;
        public string icon;
        public string heroId;
        public string skinId;
        public bool isExclusive;
    }
}
