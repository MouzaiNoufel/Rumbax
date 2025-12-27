using System;

namespace Rumbax.Core.Events
{
    /// <summary>
    /// Event fired when game state changes.
    /// </summary>
    public class GameStateChangedEvent : IGameEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previous, GameState newState)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }

    /// <summary>
    /// Event fired when game is paused or resumed.
    /// </summary>
    public class GamePausedEvent : IGameEvent
    {
        public bool IsPaused { get; }

        public GamePausedEvent(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }

    /// <summary>
    /// Event fired when offline earnings are calculated.
    /// </summary>
    public class OfflineEarningsEvent : IGameEvent
    {
        public long CoinsEarned { get; }
        public TimeSpan OfflineTime { get; }

        public OfflineEarningsEvent(long coins, TimeSpan time)
        {
            CoinsEarned = coins;
            OfflineTime = time;
        }
    }

    /// <summary>
    /// Event fired when currency changes.
    /// </summary>
    public class CurrencyChangedEvent : IGameEvent
    {
        public CurrencyType Type { get; }
        public long OldAmount { get; }
        public long NewAmount { get; }
        public long Delta { get; }

        public CurrencyChangedEvent(CurrencyType type, long oldAmount, long newAmount)
        {
            Type = type;
            OldAmount = oldAmount;
            NewAmount = newAmount;
            Delta = newAmount - oldAmount;
        }
    }

    public enum CurrencyType
    {
        Coins,
        Gems
    }

    /// <summary>
    /// Event fired when a level is started.
    /// </summary>
    public class LevelStartedEvent : IGameEvent
    {
        public int LevelNumber { get; }

        public LevelStartedEvent(int level)
        {
            LevelNumber = level;
        }
    }

    /// <summary>
    /// Event fired when a level is completed.
    /// </summary>
    public class LevelCompletedEvent : IGameEvent
    {
        public int LevelNumber { get; }
        public int Level => LevelNumber;
        public int Stars { get; }
        public long Score { get; }
        public long CoinsEarned { get; }

        public LevelCompletedEvent(int level, int stars, long score, long coins)
        {
            LevelNumber = level;
            Stars = stars;
            Score = score;
            CoinsEarned = coins;
        }
    }

    /// <summary>
    /// Event fired when a level fails.
    /// </summary>
    public class LevelFailedEvent : IGameEvent
    {
        public int LevelNumber { get; }
        public string Reason { get; }

        public LevelFailedEvent(int level, string reason)
        {
            LevelNumber = level;
            Reason = reason;
        }
    }

    /// <summary>
    /// Event fired when a wave starts.
    /// </summary>
    public class WaveStartedEvent : IGameEvent
    {
        public int WaveNumber { get; }
        public int TotalWaves { get; }

        public WaveStartedEvent(int wave, int total)
        {
            WaveNumber = wave;
            TotalWaves = total;
        }
    }

    /// <summary>
    /// Event fired when a wave is completed.
    /// </summary>
    public class WaveCompletedEvent : IGameEvent
    {
        public int WaveNumber { get; }
        public bool PerfectWave { get; }

        public WaveCompletedEvent(int wave, bool perfect = false)
        {
            WaveNumber = wave;
            PerfectWave = perfect;
        }
    }

    /// <summary>
    /// Event fired when an enemy is defeated.
    /// </summary>
    public class EnemyDefeatedEvent : IGameEvent
    {
        public string EnemyId { get; }
        public long CoinsDropped { get; }

        public EnemyDefeatedEvent(string enemyId, long coins)
        {
            EnemyId = enemyId;
            CoinsDropped = coins;
        }
    }

    /// <summary>
    /// Event fired when a defender is merged.
    /// </summary>
    public class DefenderMergedEvent : IGameEvent
    {
        public string DefenderType { get; }
        public int NewLevel { get; }
        public UnityEngine.Vector3 Position { get; }

        public DefenderMergedEvent(string type, int level, UnityEngine.Vector3 position = default)
        {
            DefenderType = type;
            NewLevel = level;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when a defender is spawned.
    /// </summary>
    public class DefenderSpawnedEvent : IGameEvent
    {
        public string DefenderId { get; }
        public int Level { get; }
        public UnityEngine.Vector3 Position { get; }

        public DefenderSpawnedEvent(string id, int level, UnityEngine.Vector3 position = default)
        {
            DefenderId = id;
            Level = level;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when an achievement is unlocked.
    /// </summary>
    public class AchievementUnlockedEvent : IGameEvent
    {
        public string AchievementId { get; }
        public string AchievementName { get; }

        public AchievementUnlockedEvent(string id, string name)
        {
            AchievementId = id;
            AchievementName = name;
        }
    }

    /// <summary>
    /// Event fired when a daily challenge is completed.
    /// </summary>
    public class DailyChallengeCompletedEvent : IGameEvent
    {
        public string ChallengeId { get; }
        public int RewardCoins { get; }
        public int RewardGems { get; }

        public DailyChallengeCompletedEvent(string id, int coins, int gems)
        {
            ChallengeId = id;
            RewardCoins = coins;
            RewardGems = gems;
        }
    }

    /// <summary>
    /// Event fired when login streak is updated.
    /// </summary>
    public class StreakUpdatedEvent : IGameEvent
    {
        public int CurrentStreak { get; }
        public bool RewardClaimed { get; }

        public StreakUpdatedEvent(int streak, bool claimed)
        {
            CurrentStreak = streak;
            RewardClaimed = claimed;
        }
    }

    /// <summary>
    /// Event fired when subscription status changes.
    /// </summary>
    public class SubscriptionChangedEvent : IGameEvent
    {
        public bool IsSubscribed { get; }
        public Services.SubscriptionTier Tier { get; }

        public SubscriptionChangedEvent(bool subscribed, Services.SubscriptionTier tier)
        {
            IsSubscribed = subscribed;
            Tier = tier;
        }
    }

    /// <summary>
    /// Event fired when the game is over (player lost).
    /// </summary>
    public class GameOverEvent : IGameEvent
    {
        public int FinalLevel { get; }
        public long FinalScore { get; }
        public string Reason { get; }

        public GameOverEvent(int level, long score, string reason = "Enemies reached base")
        {
            FinalLevel = level;
            FinalScore = score;
            Reason = reason;
        }
    }

    /// <summary>
    /// Event fired when an enemy is killed.
    /// </summary>
    public class EnemyKilledEvent : IGameEvent
    {
        public string EnemyId { get; }
        public string EnemyType { get; }
        public long CoinsDropped { get; }
        public long CoinReward => CoinsDropped;
        public bool IsBoss { get; }
        public UnityEngine.Vector3 Position { get; }

        public EnemyKilledEvent(string enemyId, string enemyType, long coins, bool isBoss = false, UnityEngine.Vector3 position = default)
        {
            EnemyId = enemyId;
            EnemyType = enemyType;
            CoinsDropped = coins;
            IsBoss = isBoss;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when an ad is watched.
    /// </summary>
    public class AdWatchedEvent : IGameEvent
    {
        public string AdType { get; }
        public string Placement { get; }
        public bool WasRewarded { get; }

        public AdWatchedEvent(string adType, string placement, bool wasRewarded = false)
        {
            AdType = adType;
            Placement = placement;
            WasRewarded = wasRewarded;
        }
    }

    /// <summary>
    /// Event fired when a purchase is completed.
    /// </summary>
    public class PurchaseCompletedEvent : IGameEvent
    {
        public string ProductId { get; }
        public string ProductType { get; }
        public decimal Amount { get; }

        public PurchaseCompletedEvent(string productId, string productType, decimal amount = 0)
        {
            ProductId = productId;
            ProductType = productType;
            Amount = amount;
        }
    }
}
