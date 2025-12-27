using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Daily Rewards System - Professional mobile game feature with login streaks and escalating rewards.
    /// </summary>
    [Serializable]
    public class DailyRewardData
    {
        public int day;
        public RewardType type;
        public int amount;
        public string heroId; // For hero fragment rewards
        public bool claimed;
    }

    public enum RewardType
    {
        Coins,
        Gems,
        HeroFragment,
        PowerUp,
        ExperienceBoost,
        LuckySpins
    }

    public class DailyRewardsSystem : MonoBehaviour
    {
        public static DailyRewardsSystem Instance { get; private set; }

        private const int REWARD_CYCLE_DAYS = 28; // 4 weeks cycle
        private List<DailyRewardData> _rewards = new List<DailyRewardData>();
        private int _currentStreak;
        private DateTime _lastClaimDate;
        private bool _canClaimToday;

        // Events
        public event Action<DailyRewardData> OnRewardClaimed;
        public event Action<int> OnStreakUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeRewards();
                LoadProgress();
                CheckDailyReset();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeRewards()
        {
            _rewards.Clear();

            // Week 1 - Basic rewards
            AddReward(1, RewardType.Coins, 100);
            AddReward(2, RewardType.Gems, 5);
            AddReward(3, RewardType.Coins, 200);
            AddReward(4, RewardType.PowerUp, 2);
            AddReward(5, RewardType.Gems, 10);
            AddReward(6, RewardType.Coins, 400);
            AddReward(7, RewardType.Gems, 25); // Week reward

            // Week 2 - Better rewards
            AddReward(8, RewardType.Coins, 300);
            AddReward(9, RewardType.HeroFragment, 1, "archer");
            AddReward(10, RewardType.Gems, 15);
            AddReward(11, RewardType.Coins, 500);
            AddReward(12, RewardType.ExperienceBoost, 1);
            AddReward(13, RewardType.Gems, 20);
            AddReward(14, RewardType.Gems, 50); // Week 2 reward

            // Week 3 - Good rewards
            AddReward(15, RewardType.Coins, 600);
            AddReward(16, RewardType.HeroFragment, 2, "paladin");
            AddReward(17, RewardType.Gems, 25);
            AddReward(18, RewardType.Coins, 800);
            AddReward(19, RewardType.LuckySpins, 3);
            AddReward(20, RewardType.Gems, 30);
            AddReward(21, RewardType.Gems, 100); // Week 3 reward

            // Week 4 - Premium rewards
            AddReward(22, RewardType.Coins, 1000);
            AddReward(23, RewardType.HeroFragment, 3, "shadow_blade");
            AddReward(24, RewardType.Gems, 50);
            AddReward(25, RewardType.Coins, 1500);
            AddReward(26, RewardType.LuckySpins, 5);
            AddReward(27, RewardType.Gems, 75);
            AddReward(28, RewardType.Gems, 200); // Month reward!
        }

        private void AddReward(int day, RewardType type, int amount, string heroId = "")
        {
            _rewards.Add(new DailyRewardData
            {
                day = day,
                type = type,
                amount = amount,
                heroId = heroId,
                claimed = false
            });
        }

        private void CheckDailyReset()
        {
            DateTime today = DateTime.Today;
            
            if (_lastClaimDate.Date == today)
            {
                _canClaimToday = false;
            }
            else if (_lastClaimDate.Date == today.AddDays(-1))
            {
                // Consecutive day - can claim
                _canClaimToday = true;
            }
            else if (_lastClaimDate == DateTime.MinValue)
            {
                // First time player
                _canClaimToday = true;
                _currentStreak = 0;
            }
            else
            {
                // Streak broken - reset but can still claim
                _currentStreak = 0;
                _canClaimToday = true;
                ResetAllRewards();
            }
        }

        public bool CanClaimToday() => _canClaimToday;
        public int GetCurrentStreak() => _currentStreak;
        public int GetCurrentDay() => (_currentStreak % REWARD_CYCLE_DAYS) + 1;

        public DailyRewardData GetTodaysReward()
        {
            int dayIndex = _currentStreak % REWARD_CYCLE_DAYS;
            return _rewards[dayIndex];
        }

        public List<DailyRewardData> GetAllRewards() => _rewards;

        public DailyRewardData ClaimReward()
        {
            if (!_canClaimToday) return null;

            int dayIndex = _currentStreak % REWARD_CYCLE_DAYS;
            DailyRewardData reward = _rewards[dayIndex];

            _currentStreak++;
            _lastClaimDate = DateTime.Today;
            _canClaimToday = false;
            reward.claimed = true;

            SaveProgress();
            
            OnRewardClaimed?.Invoke(reward);
            OnStreakUpdated?.Invoke(_currentStreak);

            return reward;
        }

        public void ApplyReward(DailyRewardData reward, ref int coins, ref int gems)
        {
            switch (reward.type)
            {
                case RewardType.Coins:
                    coins += reward.amount;
                    break;
                case RewardType.Gems:
                    gems += reward.amount;
                    break;
                case RewardType.HeroFragment:
                    // Add hero fragments - unlock after collecting enough
                    AddHeroFragments(reward.heroId, reward.amount);
                    break;
                case RewardType.PowerUp:
                    AddPowerUps(reward.amount);
                    break;
                case RewardType.ExperienceBoost:
                    ActivateExpBoost(reward.amount);
                    break;
                case RewardType.LuckySpins:
                    AddLuckySpins(reward.amount);
                    break;
            }
        }

        private void AddHeroFragments(string heroId, int amount)
        {
            int current = PlayerPrefs.GetInt($"Fragments_{heroId}", 0);
            PlayerPrefs.SetInt($"Fragments_{heroId}", current + amount);
            
            // Check if enough to unlock (10 fragments to unlock)
            if (current + amount >= 10)
            {
                // Unlock hero logic would go here
                Debug.Log($"[DailyRewards] Can unlock hero: {heroId}!");
            }
            
            PlayerPrefs.Save();
        }

        public int GetHeroFragments(string heroId)
        {
            return PlayerPrefs.GetInt($"Fragments_{heroId}", 0);
        }

        private void AddPowerUps(int amount)
        {
            int current = PlayerPrefs.GetInt("StoredPowerUps", 0);
            PlayerPrefs.SetInt("StoredPowerUps", current + amount);
            PlayerPrefs.Save();
        }

        private void ActivateExpBoost(int hours)
        {
            DateTime expireTime = DateTime.Now.AddHours(hours);
            PlayerPrefs.SetString("ExpBoostExpire", expireTime.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        public bool IsExpBoostActive()
        {
            string expireStr = PlayerPrefs.GetString("ExpBoostExpire", "");
            if (string.IsNullOrEmpty(expireStr)) return false;
            
            if (long.TryParse(expireStr, out long binary))
            {
                DateTime expireTime = DateTime.FromBinary(binary);
                return DateTime.Now < expireTime;
            }
            return false;
        }

        public void AddLuckySpins(int amount)
        {
            int current = PlayerPrefs.GetInt("LuckySpins", 0);
            PlayerPrefs.SetInt("LuckySpins", current + amount);
            PlayerPrefs.Save();
        }

        public int GetLuckySpins()
        {
            return PlayerPrefs.GetInt("LuckySpins", 0);
        }

        public void UseLuckySpin()
        {
            int current = GetLuckySpins();
            if (current > 0)
            {
                PlayerPrefs.SetInt("LuckySpins", current - 1);
                PlayerPrefs.Save();
            }
        }

        private void ResetAllRewards()
        {
            foreach (var reward in _rewards)
            {
                reward.claimed = false;
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("DailyStreak", _currentStreak);
            PlayerPrefs.SetString("LastClaimDate", _lastClaimDate.ToBinary().ToString());
            
            for (int i = 0; i < _rewards.Count; i++)
            {
                PlayerPrefs.SetInt($"DailyReward_{i}_Claimed", _rewards[i].claimed ? 1 : 0);
            }
            
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _currentStreak = PlayerPrefs.GetInt("DailyStreak", 0);
            
            string lastClaimStr = PlayerPrefs.GetString("LastClaimDate", "");
            if (!string.IsNullOrEmpty(lastClaimStr) && long.TryParse(lastClaimStr, out long binary))
            {
                _lastClaimDate = DateTime.FromBinary(binary);
            }
            else
            {
                _lastClaimDate = DateTime.MinValue;
            }

            for (int i = 0; i < _rewards.Count; i++)
            {
                _rewards[i].claimed = PlayerPrefs.GetInt($"DailyReward_{i}_Claimed", 0) == 1;
            }
        }

        public string GetRewardDescription(DailyRewardData reward)
        {
            return reward.type switch
            {
                RewardType.Coins => $"💰 {reward.amount} Coins",
                RewardType.Gems => $"💎 {reward.amount} Gems",
                RewardType.HeroFragment => $"🧩 {reward.amount}x {reward.heroId} Fragments",
                RewardType.PowerUp => $"⚡ {reward.amount}x Power-ups",
                RewardType.ExperienceBoost => $"📈 {reward.amount}hr XP Boost",
                RewardType.LuckySpins => $"🎰 {reward.amount}x Lucky Spins",
                _ => "Unknown"
            };
        }

        public string GetRewardIcon(RewardType type)
        {
            return type switch
            {
                RewardType.Coins => "💰",
                RewardType.Gems => "💎",
                RewardType.HeroFragment => "🧩",
                RewardType.PowerUp => "⚡",
                RewardType.ExperienceBoost => "📈",
                RewardType.LuckySpins => "🎰",
                _ => "❓"
            };
        }

        public Color GetRewardColor(RewardType type)
        {
            return type switch
            {
                RewardType.Coins => new Color(1f, 0.85f, 0.2f),
                RewardType.Gems => new Color(0.4f, 0.8f, 1f),
                RewardType.HeroFragment => new Color(0.8f, 0.4f, 1f),
                RewardType.PowerUp => new Color(1f, 0.5f, 0.2f),
                RewardType.ExperienceBoost => new Color(0.4f, 1f, 0.4f),
                RewardType.LuckySpins => new Color(1f, 0.3f, 0.5f),
                _ => Color.white
            };
        }
    }
}
