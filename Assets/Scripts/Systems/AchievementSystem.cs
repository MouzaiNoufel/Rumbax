using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// AchievementEntry System - Tracks player accomplishments with rewards.
    /// Professional mobile game feature.
    /// </summary>
    [Serializable]
    public class AchievementEntry
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public AchievementCategoryType category;
        public AchievementTierLevel tier;
        public int targetValue;
        public int currentValue;
        public int coinReward;
        public int gemReward;
        public bool isCompleted;
        public bool isRewardClaimed;
        public bool isSecret;
    }

    public enum AchievementCategoryType
    {
        Combat,
        Collection,
        Progress,
        Mastery,
        Social,
        Special
    }

    public enum AchievementTierLevel
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond
    }

    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        private List<AchievementEntry> _achievements = new List<AchievementEntry>();
        private int _totalAchievementPoints;

        // Events
        public event Action<AchievementEntry> OnAchievementCompleted;
        public event Action<AchievementEntry> OnAchievementProgress;
        public event Action<int> OnPointsUpdated;

        // Tier colors
        public static readonly Dictionary<AchievementTierLevel, Color> TierColors = new Dictionary<AchievementTierLevel, Color>
        {
            { AchievementTierLevel.Bronze, new Color(0.8f, 0.5f, 0.2f) },
            { AchievementTierLevel.Silver, new Color(0.75f, 0.75f, 0.8f) },
            { AchievementTierLevel.Gold, new Color(1f, 0.85f, 0.2f) },
            { AchievementTierLevel.Platinum, new Color(0.4f, 0.9f, 0.9f) },
            { AchievementTierLevel.Diamond, new Color(0.7f, 0.4f, 1f) }
        };

        public static readonly Dictionary<AchievementTierLevel, int> TierPoints = new Dictionary<AchievementTierLevel, int>
        {
            { AchievementTierLevel.Bronze, 5 },
            { AchievementTierLevel.Silver, 10 },
            { AchievementTierLevel.Gold, 25 },
            { AchievementTierLevel.Platinum, 50 },
            { AchievementTierLevel.Diamond, 100 }
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAchievements();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAchievements()
        {
            _achievements.Clear();

            // === COMBAT ACHIEVEMENTS ===
            AddAchievement("first_blood", "First Blood", "Kill your first enemy", "⚔️",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 1, 50, 0);
            AddAchievement("kill_100", "Hunter", "Kill 100 enemies", "🎯",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 100, 200, 5);
            AddAchievement("kill_500", "Slayer", "Kill 500 enemies", "💀",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 500, 500, 15);
            AddAchievement("kill_1000", "Destroyer", "Kill 1,000 enemies", "☠️",
                AchievementCategoryType.Combat, AchievementTierLevel.Gold, 1000, 1000, 30);
            AddAchievement("kill_5000", "Annihilator", "Kill 5,000 enemies", "🔥",
                AchievementCategoryType.Combat, AchievementTierLevel.Platinum, 5000, 2500, 75);
            AddAchievement("kill_10000", "Legend of Death", "Kill 10,000 enemies", "👑",
                AchievementCategoryType.Combat, AchievementTierLevel.Diamond, 10000, 5000, 200);

            AddAchievement("boss_1", "Boss Hunter", "Defeat your first boss", "👹",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 1, 100, 5);
            AddAchievement("boss_10", "Boss Slayer", "Defeat 10 bosses", "🐲",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 10, 300, 20);
            AddAchievement("boss_50", "Boss Destroyer", "Defeat 50 bosses", "💪",
                AchievementCategoryType.Combat, AchievementTierLevel.Gold, 50, 800, 50);

            AddAchievement("combo_10", "Combo Starter", "Reach a 10x combo", "🔢",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 10, 100, 5);
            AddAchievement("combo_25", "Combo Master", "Reach a 25x combo", "⚡",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 25, 300, 15);
            AddAchievement("combo_50", "Combo King", "Reach a 50x combo", "👑",
                AchievementCategoryType.Combat, AchievementTierLevel.Gold, 50, 600, 30);
            AddAchievement("combo_100", "Combo God", "Reach a 100x combo", "🌟",
                AchievementCategoryType.Combat, AchievementTierLevel.Diamond, 100, 2000, 100);

            AddAchievement("fever_1", "Fever Time", "Activate Fever Mode", "🔥",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 1, 75, 3);
            AddAchievement("fever_10", "Fever Fan", "Activate Fever Mode 10 times", "🌡️",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 10, 200, 10);
            AddAchievement("fever_50", "Fever Master", "Activate Fever Mode 50 times", "🎆",
                AchievementCategoryType.Combat, AchievementTierLevel.Gold, 50, 500, 25);

            AddAchievement("ultimate_1", "Ultimate Power", "Use Ultimate ability", "⚡",
                AchievementCategoryType.Combat, AchievementTierLevel.Bronze, 1, 75, 3);
            AddAchievement("ultimate_25", "Ultimate Warrior", "Use Ultimate 25 times", "💥",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 25, 250, 12);

            AddAchievement("crit_100", "Critical Eye", "Land 100 critical hits", "💢",
                AchievementCategoryType.Combat, AchievementTierLevel.Silver, 100, 300, 10);
            AddAchievement("crit_1000", "Critical Master", "Land 1,000 critical hits", "🎯",
                AchievementCategoryType.Combat, AchievementTierLevel.Gold, 1000, 800, 40);

            // === COLLECTION ACHIEVEMENTS ===
            AddAchievement("hero_2", "Duo", "Unlock 2 heroes", "👥",
                AchievementCategoryType.Collection, AchievementTierLevel.Bronze, 2, 100, 5);
            AddAchievement("hero_5", "Squad", "Unlock 5 heroes", "👨‍👩‍👧‍👦",
                AchievementCategoryType.Collection, AchievementTierLevel.Silver, 5, 300, 20);
            AddAchievement("hero_10", "Army", "Unlock 10 heroes", "🎖️",
                AchievementCategoryType.Collection, AchievementTierLevel.Gold, 10, 800, 50);
            AddAchievement("hero_all", "Hero Collector", "Unlock all heroes", "🏆",
                AchievementCategoryType.Collection, AchievementTierLevel.Diamond, 15, 5000, 300);

            AddAchievement("rare_hero", "Rare Find", "Unlock a Rare hero", "💙",
                AchievementCategoryType.Collection, AchievementTierLevel.Silver, 1, 200, 10);
            AddAchievement("epic_hero", "Epic Discovery", "Unlock an Epic hero", "💜",
                AchievementCategoryType.Collection, AchievementTierLevel.Gold, 1, 500, 25);
            AddAchievement("legendary_hero", "Legendary Legend", "Unlock a Legendary hero", "💛",
                AchievementCategoryType.Collection, AchievementTierLevel.Platinum, 1, 1000, 50);
            AddAchievement("mythic_hero", "Mythic Miracle", "Unlock a Mythic hero", "❤️",
                AchievementCategoryType.Collection, AchievementTierLevel.Diamond, 1, 3000, 150);

            AddAchievement("merge_10", "Merger", "Merge 10 defenders", "🔗",
                AchievementCategoryType.Collection, AchievementTierLevel.Bronze, 10, 100, 5);
            AddAchievement("merge_100", "Fusion Expert", "Merge 100 defenders", "⚡",
                AchievementCategoryType.Collection, AchievementTierLevel.Silver, 100, 400, 20);
            AddAchievement("merge_500", "Fusion Master", "Merge 500 defenders", "🌟",
                AchievementCategoryType.Collection, AchievementTierLevel.Gold, 500, 1000, 50);

            AddAchievement("tier5_1", "Max Power", "Create a Tier 5 defender", "⭐",
                AchievementCategoryType.Collection, AchievementTierLevel.Silver, 1, 200, 10);
            AddAchievement("tier5_10", "Power House", "Create 10 Tier 5 defenders", "🌟",
                AchievementCategoryType.Collection, AchievementTierLevel.Gold, 10, 600, 30);

            // === PROGRESS ACHIEVEMENTS ===
            AddAchievement("wave_5", "Survivor", "Reach Wave 5", "🌊",
                AchievementCategoryType.Progress, AchievementTierLevel.Bronze, 5, 75, 3);
            AddAchievement("wave_10", "Veteran", "Reach Wave 10", "🌊",
                AchievementCategoryType.Progress, AchievementTierLevel.Bronze, 10, 150, 5);
            AddAchievement("wave_25", "Elite", "Reach Wave 25", "🌊",
                AchievementCategoryType.Progress, AchievementTierLevel.Silver, 25, 400, 15);
            AddAchievement("wave_50", "Champion", "Reach Wave 50", "🏅",
                AchievementCategoryType.Progress, AchievementTierLevel.Gold, 50, 800, 35);
            AddAchievement("wave_100", "Master", "Reach Wave 100", "🏆",
                AchievementCategoryType.Progress, AchievementTierLevel.Platinum, 100, 2000, 100);
            AddAchievement("wave_200", "Immortal", "Reach Wave 200", "👑",
                AchievementCategoryType.Progress, AchievementTierLevel.Diamond, 200, 5000, 250);

            AddAchievement("coins_1000", "Piggy Bank", "Earn 1,000 total coins", "🐷",
                AchievementCategoryType.Progress, AchievementTierLevel.Bronze, 1000, 100, 0);
            AddAchievement("coins_10000", "Wealthy", "Earn 10,000 total coins", "💰",
                AchievementCategoryType.Progress, AchievementTierLevel.Silver, 10000, 500, 10);
            AddAchievement("coins_100000", "Rich", "Earn 100,000 total coins", "🏦",
                AchievementCategoryType.Progress, AchievementTierLevel.Gold, 100000, 1500, 50);
            AddAchievement("coins_1000000", "Millionaire", "Earn 1,000,000 coins", "💎",
                AchievementCategoryType.Progress, AchievementTierLevel.Diamond, 1000000, 5000, 200);

            AddAchievement("games_10", "Regular", "Play 10 games", "🎮",
                AchievementCategoryType.Progress, AchievementTierLevel.Bronze, 10, 100, 5);
            AddAchievement("games_100", "Dedicated", "Play 100 games", "🎯",
                AchievementCategoryType.Progress, AchievementTierLevel.Silver, 100, 500, 25);
            AddAchievement("games_500", "Devoted", "Play 500 games", "❤️",
                AchievementCategoryType.Progress, AchievementTierLevel.Gold, 500, 1500, 75);

            AddAchievement("daily_7", "Week Warrior", "Login 7 days in a row", "📅",
                AchievementCategoryType.Progress, AchievementTierLevel.Silver, 7, 300, 15);
            AddAchievement("daily_30", "Month Master", "Login 30 days in a row", "🗓️",
                AchievementCategoryType.Progress, AchievementTierLevel.Gold, 30, 1000, 50);
            AddAchievement("daily_100", "Loyal Legend", "Login 100 days in a row", "👑",
                AchievementCategoryType.Progress, AchievementTierLevel.Diamond, 100, 3000, 150);

            // === MASTERY ACHIEVEMENTS ===
            AddAchievement("no_damage", "Untouchable", "Complete a wave without losing lives", "🛡️",
                AchievementCategoryType.Mastery, AchievementTierLevel.Silver, 1, 200, 10);
            AddAchievement("speed_3x", "Speed Demon", "Complete a wave on 3x speed", "⚡",
                AchievementCategoryType.Mastery, AchievementTierLevel.Bronze, 1, 100, 5);
            AddAchievement("powerup_all", "Power Collector", "Collect all power-up types", "🎁",
                AchievementCategoryType.Mastery, AchievementTierLevel.Silver, 5, 250, 12);
            AddAchievement("score_10000", "High Scorer", "Achieve a score of 10,000", "🔢",
                AchievementCategoryType.Mastery, AchievementTierLevel.Silver, 10000, 300, 15);
            AddAchievement("score_100000", "Score Master", "Achieve a score of 100,000", "🏅",
                AchievementCategoryType.Mastery, AchievementTierLevel.Gold, 100000, 1000, 50);
            AddAchievement("score_1000000", "Score Legend", "Achieve a score of 1,000,000", "🏆",
                AchievementCategoryType.Mastery, AchievementTierLevel.Diamond, 1000000, 5000, 250);

            // === SPECIAL/SECRET ACHIEVEMENTS ===
            AddAchievement("lucky_spin", "Lucky One", "Win jackpot on lucky wheel", "🎰",
                AchievementCategoryType.Special, AchievementTierLevel.Gold, 1, 0, 100, true);
            AddAchievement("all_achievements", "Perfectionist", "Complete all other achievements", "🌟",
                AchievementCategoryType.Special, AchievementTierLevel.Diamond, 1, 10000, 500, true);
            AddAchievement("easter_egg", "Explorer", "Find a hidden secret", "🔍",
                AchievementCategoryType.Special, AchievementTierLevel.Gold, 1, 500, 50, true);
        }

        private void AddAchievement(string id, string name, string description, string icon,
            AchievementCategoryType category, AchievementTierLevel tier, int target, int coins, int gems, bool secret = false)
        {
            _achievements.Add(new AchievementEntry
            {
                id = id,
                name = name,
                description = description,
                icon = icon,
                category = category,
                tier = tier,
                targetValue = target,
                currentValue = 0,
                coinReward = coins,
                gemReward = gems,
                isCompleted = false,
                isRewardClaimed = false,
                isSecret = secret
            });
        }

        public void UpdateProgress(string achievementId, int newValue, bool additive = true)
        {
            AchievementEntry AchievementEntry = _achievements.Find(a => a.id == achievementId);
            if (AchievementEntry == null || AchievementEntry.isCompleted) return;

            if (additive)
                AchievementEntry.currentValue += newValue;
            else
                AchievementEntry.currentValue = Mathf.Max(AchievementEntry.currentValue, newValue);

            OnAchievementProgress?.Invoke(AchievementEntry);

            if (AchievementEntry.currentValue >= AchievementEntry.targetValue && !AchievementEntry.isCompleted)
            {
                AchievementEntry.isCompleted = true;
                SaveProgress();
                OnAchievementCompleted?.Invoke(AchievementEntry);
            }
        }

        public void IncrementProgress(string achievementId)
        {
            UpdateProgress(achievementId, 1, true);
        }

        public bool ClaimReward(string achievementId, ref int coins, ref int gems)
        {
            AchievementEntry AchievementEntry = _achievements.Find(a => a.id == achievementId);
            if (AchievementEntry == null || !AchievementEntry.isCompleted || AchievementEntry.isRewardClaimed)
                return false;

            coins += AchievementEntry.coinReward;
            gems += AchievementEntry.gemReward;
            AchievementEntry.isRewardClaimed = true;
            
            _totalAchievementPoints += TierPoints[AchievementEntry.tier];
            OnPointsUpdated?.Invoke(_totalAchievementPoints);

            SaveProgress();
            return true;
        }

        public List<AchievementEntry> GetAllAchievements() => _achievements;

        public List<AchievementEntry> GetAchievementsByCategory(AchievementCategoryType category)
        {
            return _achievements.FindAll(a => a.category == category && (!a.isSecret || a.isCompleted));
        }

        public int GetCompletedCount()
        {
            return _achievements.FindAll(a => a.isCompleted).Count;
        }

        public int GetTotalCount()
        {
            return _achievements.Count;
        }

        public int GetUnclaimedCount()
        {
            return _achievements.FindAll(a => a.isCompleted && !a.isRewardClaimed).Count;
        }

        public int GetTotalPoints() => _totalAchievementPoints;

        public float GetCompletionPercentage()
        {
            return (float)GetCompletedCount() / GetTotalCount() * 100f;
        }

        public string GetProgressText(AchievementEntry AchievementEntry)
        {
            return $"{AchievementEntry.currentValue:N0} / {AchievementEntry.targetValue:N0}";
        }

        public float GetProgressPercentage(AchievementEntry AchievementEntry)
        {
            return Mathf.Clamp01((float)AchievementEntry.currentValue / AchievementEntry.targetValue);
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("AchievementPoints", _totalAchievementPoints);

            foreach (var AchievementEntry in _achievements)
            {
                PlayerPrefs.SetInt($"Ach_{AchievementEntry.id}_Value", AchievementEntry.currentValue);
                PlayerPrefs.SetInt($"Ach_{AchievementEntry.id}_Done", AchievementEntry.isCompleted ? 1 : 0);
                PlayerPrefs.SetInt($"Ach_{AchievementEntry.id}_Claimed", AchievementEntry.isRewardClaimed ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _totalAchievementPoints = PlayerPrefs.GetInt("AchievementPoints", 0);

            foreach (var AchievementEntry in _achievements)
            {
                AchievementEntry.currentValue = PlayerPrefs.GetInt($"Ach_{AchievementEntry.id}_Value", 0);
                AchievementEntry.isCompleted = PlayerPrefs.GetInt($"Ach_{AchievementEntry.id}_Done", 0) == 1;
                AchievementEntry.isRewardClaimed = PlayerPrefs.GetInt($"Ach_{AchievementEntry.id}_Claimed", 0) == 1;
            }
        }

        // Quick access methods for common AchievementEntry updates
        public void OnEnemyKilled() => IncrementProgress("kill_100");
        public void OnBossKilled() => IncrementProgress("boss_1");
        public void OnComboReached(int combo) => UpdateProgress("combo_10", combo, false);
        public void OnFeverActivated() => IncrementProgress("fever_1");
        public void OnUltimateUsed() => IncrementProgress("ultimate_1");
        public void OnCriticalHit() => IncrementProgress("crit_100");
        public void OnMerge() => IncrementProgress("merge_10");
        public void OnWaveReached(int wave) => UpdateProgress("wave_5", wave, false);
        public void OnCoinsEarned(int coins) => UpdateProgress("coins_1000", coins, true);
        public void OnGamePlayed() => IncrementProgress("games_10");
        public void OnHeroUnlocked() => IncrementProgress("hero_2");
        public void OnDailyStreak(int days) => UpdateProgress("daily_7", days, false);
        public void OnScoreReached(int score) => UpdateProgress("score_10000", score, false);
    }
}
