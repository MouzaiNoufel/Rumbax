using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rumbax.Data
{
    /// <summary>
    /// Main player data container for save/load operations.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // Version for migration support
        public int DataVersion = 1;
        
        // Player identification
        public string PlayerId;
        public string PlayerName;
        
        // Currencies
        public long Coins;
        public int Gems;
        
        // Progression
        public int CurrentLevel;
        public int HighestLevel;
        public long TotalScore;
        public long HighScore;
        public int TotalStars;
        
        // Level data
        public List<LevelProgressData> LevelProgress = new List<LevelProgressData>();
        
        // Defenders
        public List<DefenderData> UnlockedDefenders = new List<DefenderData>();
        public string SelectedDefenderSkin = "default";
        
        // Economy
        public float CoinsPerMinute;
        public float CoinMultiplier = 1f;
        public float ExpMultiplier = 1f;
        
        // Timing
        public DateTime FirstPlayTime;
        public DateTime LastPlayTime;
        public DateTime LastDailyRewardTime;
        public float TotalPlayTime;
        
        // Daily system
        public int LoginStreak;
        public DateTime LastLoginDate;
        public List<DailyChallengeProgress> DailyChallenges = new List<DailyChallengeProgress>();
        public DailyProgressData DailyProgress = new DailyProgressData();
        
        // Achievements
        public List<string> UnlockedAchievements = new List<string>();
        public Dictionary<string, int> AchievementProgressDict = new Dictionary<string, int>();
        public List<AchievementProgress> Achievements = new List<AchievementProgress>();
        
        // Statistics
        public PlayerStatistics Statistics = new PlayerStatistics();
        
        // Subscription
        public bool IsSubscribed;
        public string SubscriptionTier;
        public DateTime SubscriptionExpiry;
        public bool AdsRemoved;
        
        // Settings
        public PlayerSettings Settings = new PlayerSettings();
        
        // Inventory
        public List<string> OwnedSkins = new List<string>();
        public List<string> OwnedPowerups = new List<string>();
        
        public PlayerData()
        {
            PlayerId = Guid.NewGuid().ToString();
            FirstPlayTime = DateTime.UtcNow;
            LastPlayTime = DateTime.UtcNow;
            CurrentLevel = 1;
            HighestLevel = 1;
            OwnedSkins.Add("default");
        }
    }

    /// <summary>
    /// Progress data for individual levels.
    /// </summary>
    [Serializable]
    public class LevelProgressData
    {
        public int LevelNumber;
        public bool IsCompleted;
        public int Stars;
        public long HighScore;
        public int TimesPlayed;
        public int TimesCompleted;
        public float BestTime;
    }

    /// <summary>
    /// Data for unlocked defenders.
    /// </summary>
    [Serializable]
    public class DefenderData
    {
        public string DefenderId;
        public string DefenderType;
        public int Level;
        public int Experience;
        public string EquippedSkin;
        public bool IsUnlocked;
    }

    /// <summary>
    /// Daily challenge progress tracking.
    /// </summary>
    [Serializable]
    public class DailyChallengeProgress
    {
        public string ChallengeId;
        public string ChallengeType;
        public int TargetValue;
        public int CurrentValue;
        public bool IsCompleted;
        public bool RewardClaimed;
        public DateTime ExpiryTime;
        public int CurrentStreak;
        public DateTime LastCompletionDate;
    }

    /// <summary>
    /// Daily progress tracking (streak, last completion, etc.)
    /// </summary>
    [Serializable]
    public class DailyProgressData
    {
        public int CurrentStreak;
        public DateTime LastCompletionDate;
        public List<string> CompletedChallengeIds = new List<string>();
    }

    /// <summary>
    /// Achievement progress tracking.
    /// </summary>
    [Serializable]
    public class AchievementProgress
    {
        public string Id;
        public int Progress;
        public bool IsUnlocked;
        public bool IsRewardClaimed;
        public DateTime UnlockDate;
    }

    /// <summary>
    /// Player statistics for analytics and achievements.
    /// </summary>
    [Serializable]
    public class PlayerStatistics
    {
        public int TotalGamesPlayed;
        public int TotalWins;
        public int TotalLosses;
        public int TotalEnemiesDefeated;
        public int TotalEnemiesKilled; // Alias for TotalEnemiesDefeated
        public int TotalDefendersSpawned;
        public int TotalMerges;
        public int TotalWavesCompleted;
        public int MaxMergeLevel;
        public long TotalCoinsEarned;
        public int TotalGemsEarned;
        public int TotalAdsWatched;
        public int MaxWaveReached;
        public int MaxCombo;
        public float LongestSession;
        public int ConsecutiveWins;
        public int MaxConsecutiveWins;
    }

    /// <summary>
    /// Player preferences and settings.
    /// </summary>
    [Serializable]
    public class PlayerSettings
    {
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1f;
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;
        public bool VibrationEnabled = true;
        public bool NotificationsEnabled = true;
        public string Language = "en";
        public int GraphicsQuality = 2; // 0=Low, 1=Medium, 2=High
        public bool ShowTutorial = true;
        public bool AutoMerge = false;
        public bool FastForward = false;
    }

    /// <summary>
    /// Subscription benefits configuration.
    /// </summary>
    [Serializable]
    public class SubscriptionBenefits
    {
        public bool RemoveAds;
        public bool ExclusiveSkins;
        public float ProgressionMultiplier = 1f;
        public float CoinMultiplier = 1f;
        public bool DoubleOfflineEarnings;
        public int ExtraLives;
        public bool PrioritySupport;
        public List<string> UnlockedSkins = new List<string>();
    }
}
