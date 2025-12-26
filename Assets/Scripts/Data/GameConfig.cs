using UnityEngine;
using System.Collections.Generic;

namespace Rumbax.Data
{
    /// <summary>
    /// Scriptable object for game configuration.
    /// Allows tweaking game parameters without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Rumbax/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public string GameVersion = "1.0.0";
        public int TargetFrameRate = 60;
        
        [Header("Economy")]
        public long StartingCoins = 100;
        public int StartingGems = 10;
        public float BaseCoinsPerMinute = 10f;
        public float CoinMultiplierPerLevel = 0.1f;
        
        [Header("Offline Earnings")]
        public int MaxOfflineHours = 8;
        public float OfflineEarningsRate = 0.5f;
        
        [Header("Spawning")]
        public int BaseSpawnCost = 50;
        public float SpawnCostMultiplier = 1.2f;
        public int MaxDefendersOnGrid = 25;
        
        [Header("Merging")]
        public int MaxMergeLevel = 15;
        public float MergeLevelPowerMultiplier = 1.5f;
        
        [Header("Combat")]
        public float BaseDamage = 10f;
        public float BaseAttackSpeed = 1f;
        public float EnemyHealthMultiplier = 1.1f;
        public float WaveHealthMultiplier = 1.15f;
        
        [Header("Wave System")]
        public int EnemiesPerWave = 5;
        public int EnemyIncreasePerWave = 2;
        public float TimeBetweenWaves = 5f;
        public float EnemySpawnInterval = 1f;
        
        [Header("Rewards")]
        public int BaseWinCoins = 100;
        public int BaseWinGems = 1;
        public int CoinsPerStar = 50;
        public int GemsPerLevel = 2;
        
        [Header("Daily Rewards")]
        public int[] DailyStreakCoins = { 100, 200, 300, 500, 750, 1000, 2000 };
        public int[] DailyStreakGems = { 0, 0, 1, 0, 2, 0, 5 };
        
        [Header("Ad Configuration")]
        public int InterstitialFrequency = 3;
        public int RewardedAdCoinBonus = 100;
        public int RewardedAdGemBonus = 5;
        public float ReviveHealthPercent = 0.5f;
        
        [Header("Subscription Benefits")]
        public float SubscriptionCoinMultiplier = 2f;
        public float SubscriptionProgressionMultiplier = 1.5f;
        
        [Header("Leaderboard IDs")]
        public string HighScoreLeaderboardId = "high_score_leaderboard";
        public string TotalStarsLeaderboardId = "total_stars_leaderboard";
        
        [Header("Achievement IDs")]
        public List<AchievementConfig> Achievements = new List<AchievementConfig>();
        
        [Header("Level Configuration")]
        public List<LevelConfig> Levels = new List<LevelConfig>();
    }

    /// <summary>
    /// Configuration for individual achievements.
    /// </summary>
    [System.Serializable]
    public class AchievementConfig
    {
        public string Id;
        public string Name;
        public string Description;
        public AchievementType Type;
        public int TargetValue;
        public int CoinReward;
        public int GemReward;
    }

    public enum AchievementType
    {
        GamesPlayed,
        GamesWon,
        EnemiesDefeated,
        DefendersSpawned,
        MergesPerformed,
        MaxMergeLevel,
        CoinsEarned,
        WaveReached,
        Streak,
        AdsWatched
    }

    /// <summary>
    /// Configuration for individual levels.
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        public int LevelNumber;
        public string LevelName;
        public int TotalWaves;
        public float DifficultyMultiplier = 1f;
        public List<string> AvailableEnemyTypes = new List<string>();
        public int ThreeStarScore;
        public int TwoStarScore;
        public int OneStarScore;
        public int CoinReward;
        public int GemReward;
        public string SpecialReward;
    }
}
