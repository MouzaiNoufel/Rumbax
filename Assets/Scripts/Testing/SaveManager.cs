using UnityEngine;
using System;
using System.IO;

namespace Rumbax.Testing
{
    /// <summary>
    /// Simple save/load system for player data.
    /// Uses PlayerPrefs for simplicity, but can be extended to use files.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_KEY = "RumbaxPlayerData";
        
        public PlayerData Data { get; private set; }

        public event Action OnDataLoaded;
        public event Action OnDataSaved;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadData()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                Data = JsonUtility.FromJson<PlayerData>(json);
                UnityEngine.Debug.Log("[SaveManager] Data loaded successfully");
            }
            else
            {
                Data = new PlayerData();
                UnityEngine.Debug.Log("[SaveManager] New save created");
            }

            OnDataLoaded?.Invoke();
        }

        public void SaveData()
        {
            Data.lastSaveTime = DateTime.Now.ToBinary();
            string json = JsonUtility.ToJson(Data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            UnityEngine.Debug.Log("[SaveManager] Data saved successfully");
            OnDataSaved?.Invoke();
        }

        public void ResetData()
        {
            Data = new PlayerData();
            SaveData();
            UnityEngine.Debug.Log("[SaveManager] Data reset to defaults");
        }

        // === CURRENCY METHODS ===

        public void AddCoins(int amount)
        {
            Data.coins += amount;
            SaveData();
        }

        public void AddGems(int amount)
        {
            Data.gems += amount;
            SaveData();
        }

        public bool SpendCoins(int amount)
        {
            if (Data.coins >= amount)
            {
                Data.coins -= amount;
                SaveData();
                return true;
            }
            return false;
        }

        public bool SpendGems(int amount)
        {
            if (Data.gems >= amount)
            {
                Data.gems -= amount;
                SaveData();
                return true;
            }
            return false;
        }

        // === PROGRESS METHODS ===

        public void CompleteLevel(int levelNumber, int stars, int score)
        {
            // Update highest level
            if (levelNumber > Data.highestLevel)
            {
                Data.highestLevel = levelNumber;
            }

            // Update stars for this level (keep highest)
            EnsureLevelStarsCapacity(levelNumber);
            if (stars > Data.levelStars[levelNumber - 1])
            {
                Data.levelStars[levelNumber - 1] = stars;
            }

            // Update high score
            if (score > Data.highScore)
            {
                Data.highScore = score;
            }

            Data.totalGamesPlayed++;
            Data.totalWavesCompleted += levelNumber; // Assume each level has wave count = level number

            SaveData();
        }

        private void EnsureLevelStarsCapacity(int levelNumber)
        {
            if (Data.levelStars == null)
            {
                Data.levelStars = new int[100];
            }
            // Array is fixed size for simplicity
        }

        // === DAILY REWARDS ===

        public bool CanClaimDailyReward()
        {
            if (Data.lastDailyClaimTime == 0)
                return true;

            DateTime lastClaim = DateTime.FromBinary(Data.lastDailyClaimTime);
            DateTime now = DateTime.Now;

            // Can claim if it's a new day
            return now.Date > lastClaim.Date;
        }

        public int GetCurrentDailyDay()
        {
            if (Data.lastDailyClaimTime == 0)
                return 1;

            DateTime lastClaim = DateTime.FromBinary(Data.lastDailyClaimTime);
            DateTime now = DateTime.Now;

            // If more than 2 days have passed, reset streak
            if ((now.Date - lastClaim.Date).Days > 1)
            {
                Data.dailyStreak = 0;
                return 1;
            }

            return Mathf.Min(Data.dailyStreak + 1, 7);
        }

        public void ClaimDailyReward()
        {
            int day = GetCurrentDailyDay();
            
            // Give rewards based on day
            int[] coinRewards = { 50, 100, 150, 200, 250, 300, 500 };
            int[] gemRewards = { 0, 5, 0, 10, 0, 15, 50 };

            int coinReward = coinRewards[Mathf.Min(day - 1, coinRewards.Length - 1)];
            int gemReward = gemRewards[Mathf.Min(day - 1, gemRewards.Length - 1)];

            Data.coins += coinReward;
            Data.gems += gemReward;
            Data.dailyStreak = day;
            Data.lastDailyClaimTime = DateTime.Now.ToBinary();

            SaveData();

            UnityEngine.Debug.Log($"[SaveManager] Claimed daily reward day {day}: {coinReward} coins, {gemReward} gems");
        }

        // === SETTINGS ===

        public void SetMusicEnabled(bool enabled)
        {
            Data.musicEnabled = enabled;
            SaveData();
        }

        public void SetSfxEnabled(bool enabled)
        {
            Data.sfxEnabled = enabled;
            SaveData();
        }

        public void SetVibrationEnabled(bool enabled)
        {
            Data.vibrationEnabled = enabled;
            SaveData();
        }

        public void SetNotificationsEnabled(bool enabled)
        {
            Data.notificationsEnabled = enabled;
            SaveData();
        }
    }

    [Serializable]
    public class PlayerData
    {
        // Currency
        public int coins = 100;
        public int gems = 10;

        // Progress
        public int highestLevel = 1;
        public int highScore = 0;
        public int[] levelStars = new int[100];
        public int totalGamesPlayed = 0;
        public int totalWavesCompleted = 0;
        public int totalEnemiesKilled = 0;

        // Daily rewards
        public int dailyStreak = 0;
        public long lastDailyClaimTime = 0;

        // Settings
        public bool musicEnabled = true;
        public bool sfxEnabled = true;
        public bool vibrationEnabled = true;
        public bool notificationsEnabled = true;
        public float musicVolume = 0.5f;
        public float sfxVolume = 0.8f;

        // Unlocks
        public bool[] defendersUnlocked = new bool[20];
        public bool[] skinsUnlocked = new bool[50];

        // IAP tracking
        public bool removeAds = false;
        public bool starterPackPurchased = false;

        // Timestamps
        public long firstPlayTime = 0;
        public long lastSaveTime = 0;
        public long totalPlayTimeSeconds = 0;

        public PlayerData()
        {
            firstPlayTime = DateTime.Now.ToBinary();
            defendersUnlocked[0] = true; // First defender always unlocked
        }
    }
}
