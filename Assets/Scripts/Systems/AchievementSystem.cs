using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Achievement System - Tracks player accomplishments with rewards.
    /// Professional mobile game feature.
    /// </summary>
    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public AchievementCategory category;
        public AchievementTier tier;
        public int targetValue;
        public int currentValue;
        public int coinReward;
        public int gemReward;
        public bool isCompleted;
        public bool isRewardClaimed;
        public bool isSecret;
    }

    public enum AchievementCategory
    {
        Combat,
        Collection,
        Progress,
        Mastery,
        Social,
        Special
    }

    public enum AchievementTier
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

        private List<Achievement> _achievements = new List<Achievement>();
        private int _totalAchievementPoints;

        // Events
        public event Action<Achievement> OnAchievementCompleted;
        public event Action<Achievement> OnAchievementProgress;
        public event Action<int> OnPointsUpdated;

        // Tier colors
        public static readonly Dictionary<AchievementTier, Color> TierColors = new Dictionary<AchievementTier, Color>
        {
            { AchievementTier.Bronze, new Color(0.8f, 0.5f, 0.2f) },
            { AchievementTier.Silver, new Color(0.75f, 0.75f, 0.8f) },
            { AchievementTier.Gold, new Color(1f, 0.85f, 0.2f) },
            { AchievementTier.Platinum, new Color(0.4f, 0.9f, 0.9f) },
            { AchievementTier.Diamond, new Color(0.7f, 0.4f, 1f) }
        };

        public static readonly Dictionary<AchievementTier, int> TierPoints = new Dictionary<AchievementTier, int>
        {
            { AchievementTier.Bronze, 5 },
            { AchievementTier.Silver, 10 },
            { AchievementTier.Gold, 25 },
            { AchievementTier.Platinum, 50 },
            { AchievementTier.Diamond, 100 }
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
                AchievementCategory.Combat, AchievementTier.Bronze, 1, 50, 0);
            AddAchievement("kill_100", "Hunter", "Kill 100 enemies", "🎯",
                AchievementCategory.Combat, AchievementTier.Bronze, 100, 200, 5);
            AddAchievement("kill_500", "Slayer", "Kill 500 enemies", "💀",
                AchievementCategory.Combat, AchievementTier.Silver, 500, 500, 15);
            AddAchievement("kill_1000", "Destroyer", "Kill 1,000 enemies", "☠️",
                AchievementCategory.Combat, AchievementTier.Gold, 1000, 1000, 30);
            AddAchievement("kill_5000", "Annihilator", "Kill 5,000 enemies", "🔥",
                AchievementCategory.Combat, AchievementTier.Platinum, 5000, 2500, 75);
            AddAchievement("kill_10000", "Legend of Death", "Kill 10,000 enemies", "👑",
                AchievementCategory.Combat, AchievementTier.Diamond, 10000, 5000, 200);

            AddAchievement("boss_1", "Boss Hunter", "Defeat your first boss", "👹",
                AchievementCategory.Combat, AchievementTier.Bronze, 1, 100, 5);
            AddAchievement("boss_10", "Boss Slayer", "Defeat 10 bosses", "🐲",
                AchievementCategory.Combat, AchievementTier.Silver, 10, 300, 20);
            AddAchievement("boss_50", "Boss Destroyer", "Defeat 50 bosses", "💪",
                AchievementCategory.Combat, AchievementTier.Gold, 50, 800, 50);

            AddAchievement("combo_10", "Combo Starter", "Reach a 10x combo", "🔢",
                AchievementCategory.Combat, AchievementTier.Bronze, 10, 100, 5);
            AddAchievement("combo_25", "Combo Master", "Reach a 25x combo", "⚡",
                AchievementCategory.Combat, AchievementTier.Silver, 25, 300, 15);
            AddAchievement("combo_50", "Combo King", "Reach a 50x combo", "👑",
                AchievementCategory.Combat, AchievementTier.Gold, 50, 600, 30);
            AddAchievement("combo_100", "Combo God", "Reach a 100x combo", "🌟",
                AchievementCategory.Combat, AchievementTier.Diamond, 100, 2000, 100);

            AddAchievement("fever_1", "Fever Time", "Activate Fever Mode", "🔥",
                AchievementCategory.Combat, AchievementTier.Bronze, 1, 75, 3);
            AddAchievement("fever_10", "Fever Fan", "Activate Fever Mode 10 times", "🌡️",
                AchievementCategory.Combat, AchievementTier.Silver, 10, 200, 10);
            AddAchievement("fever_50", "Fever Master", "Activate Fever Mode 50 times", "🎆",
                AchievementCategory.Combat, AchievementTier.Gold, 50, 500, 25);

            AddAchievement("ultimate_1", "Ultimate Power", "Use Ultimate ability", "⚡",
                AchievementCategory.Combat, AchievementTier.Bronze, 1, 75, 3);
            AddAchievement("ultimate_25", "Ultimate Warrior", "Use Ultimate 25 times", "💥",
                AchievementCategory.Combat, AchievementTier.Silver, 25, 250, 12);

            AddAchievement("crit_100", "Critical Eye", "Land 100 critical hits", "💢",
                AchievementCategory.Combat, AchievementTier.Silver, 100, 300, 10);
            AddAchievement("crit_1000", "Critical Master", "Land 1,000 critical hits", "🎯",
                AchievementCategory.Combat, AchievementTier.Gold, 1000, 800, 40);

            // === COLLECTION ACHIEVEMENTS ===
            AddAchievement("hero_2", "Duo", "Unlock 2 heroes", "👥",
                AchievementCategory.Collection, AchievementTier.Bronze, 2, 100, 5);
            AddAchievement("hero_5", "Squad", "Unlock 5 heroes", "👨‍👩‍👧‍👦",
                AchievementCategory.Collection, AchievementTier.Silver, 5, 300, 20);
            AddAchievement("hero_10", "Army", "Unlock 10 heroes", "🎖️",
                AchievementCategory.Collection, AchievementTier.Gold, 10, 800, 50);
            AddAchievement("hero_all", "Hero Collector", "Unlock all heroes", "🏆",
                AchievementCategory.Collection, AchievementTier.Diamond, 15, 5000, 300);

            AddAchievement("rare_hero", "Rare Find", "Unlock a Rare hero", "💙",
                AchievementCategory.Collection, AchievementTier.Silver, 1, 200, 10);
            AddAchievement("epic_hero", "Epic Discovery", "Unlock an Epic hero", "💜",
                AchievementCategory.Collection, AchievementTier.Gold, 1, 500, 25);
            AddAchievement("legendary_hero", "Legendary Legend", "Unlock a Legendary hero", "💛",
                AchievementCategory.Collection, AchievementTier.Platinum, 1, 1000, 50);
            AddAchievement("mythic_hero", "Mythic Miracle", "Unlock a Mythic hero", "❤️",
                AchievementCategory.Collection, AchievementTier.Diamond, 1, 3000, 150);

            AddAchievement("merge_10", "Merger", "Merge 10 defenders", "🔗",
                AchievementCategory.Collection, AchievementTier.Bronze, 10, 100, 5);
            AddAchievement("merge_100", "Fusion Expert", "Merge 100 defenders", "⚡",
                AchievementCategory.Collection, AchievementTier.Silver, 100, 400, 20);
            AddAchievement("merge_500", "Fusion Master", "Merge 500 defenders", "🌟",
                AchievementCategory.Collection, AchievementTier.Gold, 500, 1000, 50);

            AddAchievement("tier5_1", "Max Power", "Create a Tier 5 defender", "⭐",
                AchievementCategory.Collection, AchievementTier.Silver, 1, 200, 10);
            AddAchievement("tier5_10", "Power House", "Create 10 Tier 5 defenders", "🌟",
                AchievementCategory.Collection, AchievementTier.Gold, 10, 600, 30);

            // === PROGRESS ACHIEVEMENTS ===
            AddAchievement("wave_5", "Survivor", "Reach Wave 5", "🌊",
                AchievementCategory.Progress, AchievementTier.Bronze, 5, 75, 3);
            AddAchievement("wave_10", "Veteran", "Reach Wave 10", "🌊",
                AchievementCategory.Progress, AchievementTier.Bronze, 10, 150, 5);
            AddAchievement("wave_25", "Elite", "Reach Wave 25", "🌊",
                AchievementCategory.Progress, AchievementTier.Silver, 25, 400, 15);
            AddAchievement("wave_50", "Champion", "Reach Wave 50", "🏅",
                AchievementCategory.Progress, AchievementTier.Gold, 50, 800, 35);
            AddAchievement("wave_100", "Master", "Reach Wave 100", "🏆",
                AchievementCategory.Progress, AchievementTier.Platinum, 100, 2000, 100);
            AddAchievement("wave_200", "Immortal", "Reach Wave 200", "👑",
                AchievementCategory.Progress, AchievementTier.Diamond, 200, 5000, 250);

            AddAchievement("coins_1000", "Piggy Bank", "Earn 1,000 total coins", "🐷",
                AchievementCategory.Progress, AchievementTier.Bronze, 1000, 100, 0);
            AddAchievement("coins_10000", "Wealthy", "Earn 10,000 total coins", "💰",
                AchievementCategory.Progress, AchievementTier.Silver, 10000, 500, 10);
            AddAchievement("coins_100000", "Rich", "Earn 100,000 total coins", "🏦",
                AchievementCategory.Progress, AchievementTier.Gold, 100000, 1500, 50);
            AddAchievement("coins_1000000", "Millionaire", "Earn 1,000,000 coins", "💎",
                AchievementCategory.Progress, AchievementTier.Diamond, 1000000, 5000, 200);

            AddAchievement("games_10", "Regular", "Play 10 games", "🎮",
                AchievementCategory.Progress, AchievementTier.Bronze, 10, 100, 5);
            AddAchievement("games_100", "Dedicated", "Play 100 games", "🎯",
                AchievementCategory.Progress, AchievementTier.Silver, 100, 500, 25);
            AddAchievement("games_500", "Devoted", "Play 500 games", "❤️",
                AchievementCategory.Progress, AchievementTier.Gold, 500, 1500, 75);

            AddAchievement("daily_7", "Week Warrior", "Login 7 days in a row", "📅",
                AchievementCategory.Progress, AchievementTier.Silver, 7, 300, 15);
            AddAchievement("daily_30", "Month Master", "Login 30 days in a row", "🗓️",
                AchievementCategory.Progress, AchievementTier.Gold, 30, 1000, 50);
            AddAchievement("daily_100", "Loyal Legend", "Login 100 days in a row", "👑",
                AchievementCategory.Progress, AchievementTier.Diamond, 100, 3000, 150);

            // === MASTERY ACHIEVEMENTS ===
            AddAchievement("no_damage", "Untouchable", "Complete a wave without losing lives", "🛡️",
                AchievementCategory.Mastery, AchievementTier.Silver, 1, 200, 10);
            AddAchievement("speed_3x", "Speed Demon", "Complete a wave on 3x speed", "⚡",
                AchievementCategory.Mastery, AchievementTier.Bronze, 1, 100, 5);
            AddAchievement("powerup_all", "Power Collector", "Collect all power-up types", "🎁",
                AchievementCategory.Mastery, AchievementTier.Silver, 5, 250, 12);
            AddAchievement("score_10000", "High Scorer", "Achieve a score of 10,000", "🔢",
                AchievementCategory.Mastery, AchievementTier.Silver, 10000, 300, 15);
            AddAchievement("score_100000", "Score Master", "Achieve a score of 100,000", "🏅",
                AchievementCategory.Mastery, AchievementTier.Gold, 100000, 1000, 50);
            AddAchievement("score_1000000", "Score Legend", "Achieve a score of 1,000,000", "🏆",
                AchievementCategory.Mastery, AchievementTier.Diamond, 1000000, 5000, 250);

            // === SPECIAL/SECRET ACHIEVEMENTS ===
            AddAchievement("lucky_spin", "Lucky One", "Win jackpot on lucky wheel", "🎰",
                AchievementCategory.Special, AchievementTier.Gold, 1, 0, 100, true);
            AddAchievement("all_achievements", "Perfectionist", "Complete all other achievements", "🌟",
                AchievementCategory.Special, AchievementTier.Diamond, 1, 10000, 500, true);
            AddAchievement("easter_egg", "Explorer", "Find a hidden secret", "🔍",
                AchievementCategory.Special, AchievementTier.Gold, 1, 500, 50, true);
        }

        private void AddAchievement(string id, string name, string description, string icon,
            AchievementCategory category, AchievementTier tier, int target, int coins, int gems, bool secret = false)
        {
            _achievements.Add(new Achievement
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
            Achievement achievement = _achievements.Find(a => a.id == achievementId);
            if (achievement == null || achievement.isCompleted) return;

            if (additive)
                achievement.currentValue += newValue;
            else
                achievement.currentValue = Mathf.Max(achievement.currentValue, newValue);

            OnAchievementProgress?.Invoke(achievement);

            if (achievement.currentValue >= achievement.targetValue && !achievement.isCompleted)
            {
                achievement.isCompleted = true;
                SaveProgress();
                OnAchievementCompleted?.Invoke(achievement);
            }
        }

        public void IncrementProgress(string achievementId)
        {
            UpdateProgress(achievementId, 1, true);
        }

        public bool ClaimReward(string achievementId, ref int coins, ref int gems)
        {
            Achievement achievement = _achievements.Find(a => a.id == achievementId);
            if (achievement == null || !achievement.isCompleted || achievement.isRewardClaimed)
                return false;

            coins += achievement.coinReward;
            gems += achievement.gemReward;
            achievement.isRewardClaimed = true;
            
            _totalAchievementPoints += TierPoints[achievement.tier];
            OnPointsUpdated?.Invoke(_totalAchievementPoints);

            SaveProgress();
            return true;
        }

        public List<Achievement> GetAllAchievements() => _achievements;

        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
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

        public string GetProgressText(Achievement achievement)
        {
            return $"{achievement.currentValue:N0} / {achievement.targetValue:N0}";
        }

        public float GetProgressPercentage(Achievement achievement)
        {
            return Mathf.Clamp01((float)achievement.currentValue / achievement.targetValue);
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("AchievementPoints", _totalAchievementPoints);

            foreach (var achievement in _achievements)
            {
                PlayerPrefs.SetInt($"Ach_{achievement.id}_Value", achievement.currentValue);
                PlayerPrefs.SetInt($"Ach_{achievement.id}_Done", achievement.isCompleted ? 1 : 0);
                PlayerPrefs.SetInt($"Ach_{achievement.id}_Claimed", achievement.isRewardClaimed ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _totalAchievementPoints = PlayerPrefs.GetInt("AchievementPoints", 0);

            foreach (var achievement in _achievements)
            {
                achievement.currentValue = PlayerPrefs.GetInt($"Ach_{achievement.id}_Value", 0);
                achievement.isCompleted = PlayerPrefs.GetInt($"Ach_{achievement.id}_Done", 0) == 1;
                achievement.isRewardClaimed = PlayerPrefs.GetInt($"Ach_{achievement.id}_Claimed", 0) == 1;
            }
        }

        // Quick access methods for common achievement updates
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
