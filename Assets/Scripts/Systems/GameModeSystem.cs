using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Game Modes System - Multiple exciting ways to play.
    /// Professional mobile game feature.
    /// </summary>
    public enum GameModeType
    {
        Classic,        // Standard waves with increasing difficulty
        Endless,        // No end, see how far you can go
        TimeAttack,     // Limited time, maximize kills/score
        BossRush,       // Only boss enemies
        DailyChallenge, // Daily special challenge with modifiers
        Hardcore,       // One life only
        Survival,       // Enemies get stronger, no breaks between waves
        PvE_Event       // Special limited-time events
    }

    [Serializable]
    public class GameModeData
    {
        public GameModeType type;
        public string name;
        public string description;
        public string icon;
        public Color color;
        public bool isUnlocked;
        public int unlockWave;      // Wave needed to unlock
        public int highScore;
        public int highWave;
        public int timesPlayed;
        public float coinMultiplier;
        public float expMultiplier;
        public bool hasTimeLimit;
        public float timeLimit;
        public List<GameModifier> modifiers;
    }

    [Serializable]
    public class GameModifier
    {
        public string name;
        public string description;
        public ModifierType type;
        public float value;
        public bool isPositive;
    }

    public enum ModifierType
    {
        EnemyHealth,
        EnemySpeed,
        EnemySpawnRate,
        DefenderDamage,
        DefenderCost,
        CoinReward,
        StartingCoins,
        StartingLives,
        ComboMultiplier,
        CriticalChance,
        PowerUpSpawn,
        NoBosses,
        OnlyBosses,
        NoMerging,
        FastForward,
        SlowMotion
    }

    [Serializable]
    public class DailyChallenge
    {
        public DateTime date;
        public int seed;
        public string name;
        public List<GameModifier> modifiers;
        public int targetScore;
        public int coinReward;
        public int gemReward;
        public bool isCompleted;
        public int playerScore;
    }

    public class GameModeSystem : MonoBehaviour
    {
        public static GameModeSystem Instance { get; private set; }

        private List<GameModeData> _gameModes = new List<GameModeData>();
        private DailyChallenge _todaysChallenge;
        private GameModeType _currentMode = GameModeType.Classic;

        // Events
        public event Action<GameModeType> OnModeSelected;
        public event Action<DailyChallenge> OnDailyChallengeCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameModes();
                GenerateDailyChallenge();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGameModes()
        {
            _gameModes.Clear();

            // Classic Mode - Always unlocked
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.Classic,
                name = "Classic",
                description = "The standard tower defense experience. Survive waves of enemies!",
                icon = "🎮",
                color = new Color(0.3f, 0.7f, 0.4f),
                isUnlocked = true,
                unlockWave = 0,
                coinMultiplier = 1f,
                expMultiplier = 1f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>()
            });

            // Endless Mode
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.Endless,
                name = "Endless",
                description = "No breaks between waves. How long can you survive?",
                icon = "♾️",
                color = new Color(0.5f, 0.3f, 0.8f),
                isUnlocked = false,
                unlockWave = 10,
                coinMultiplier = 1.5f,
                expMultiplier = 1.5f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "No Breaks", description = "Waves start immediately", type = ModifierType.EnemySpawnRate, value = 0.7f, isPositive = false },
                    new GameModifier { name = "Scaling", description = "Enemies scale infinitely", type = ModifierType.EnemyHealth, value = 1.05f, isPositive = false }
                }
            });

            // Time Attack
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.TimeAttack,
                name = "Time Attack",
                description = "Kill as many enemies as possible in 3 minutes!",
                icon = "⏱️",
                color = new Color(1f, 0.5f, 0.2f),
                isUnlocked = false,
                unlockWave = 15,
                coinMultiplier = 2f,
                expMultiplier = 1.2f,
                hasTimeLimit = true,
                timeLimit = 180f, // 3 minutes
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "Fast Spawns", description = "Enemies spawn faster", type = ModifierType.EnemySpawnRate, value = 0.5f, isPositive = false },
                    new GameModifier { name = "Bonus Coins", description = "2x coin rewards", type = ModifierType.CoinReward, value = 2f, isPositive = true },
                    new GameModifier { name = "Starting Boost", description = "Start with extra coins", type = ModifierType.StartingCoins, value = 200f, isPositive = true }
                }
            });

            // Boss Rush
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.BossRush,
                name = "Boss Rush",
                description = "Only boss enemies! Ultimate challenge for big rewards.",
                icon = "👹",
                color = new Color(0.8f, 0.2f, 0.3f),
                isUnlocked = false,
                unlockWave = 25,
                coinMultiplier = 3f,
                expMultiplier = 2f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "Only Bosses", description = "Every enemy is a boss", type = ModifierType.OnlyBosses, value = 1f, isPositive = false },
                    new GameModifier { name = "Big Rewards", description = "3x coin rewards", type = ModifierType.CoinReward, value = 3f, isPositive = true },
                    new GameModifier { name = "Power Surge", description = "More power-ups spawn", type = ModifierType.PowerUpSpawn, value = 2f, isPositive = true }
                }
            });

            // Daily Challenge
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.DailyChallenge,
                name = "Daily Challenge",
                description = "New challenge every day! Compete for the best score.",
                icon = "📅",
                color = new Color(0.9f, 0.7f, 0.2f),
                isUnlocked = false,
                unlockWave = 5,
                coinMultiplier = 1.5f,
                expMultiplier = 1.5f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>() // Generated daily
            });

            // Hardcore Mode
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.Hardcore,
                name = "Hardcore",
                description = "One life only. Can you survive?",
                icon = "💀",
                color = new Color(0.2f, 0.2f, 0.2f),
                isUnlocked = false,
                unlockWave = 20,
                coinMultiplier = 2.5f,
                expMultiplier = 2f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "One Life", description = "Only 1 life!", type = ModifierType.StartingLives, value = 1f, isPositive = false },
                    new GameModifier { name = "No Mistakes", description = "Game over on first leak", type = ModifierType.StartingLives, value = 1f, isPositive = false },
                    new GameModifier { name = "High Stakes", description = "2.5x coin rewards", type = ModifierType.CoinReward, value = 2.5f, isPositive = true }
                }
            });

            // Survival Mode
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.Survival,
                name = "Survival",
                description = "Enemies get stronger every 30 seconds. No wave breaks!",
                icon = "🏃",
                color = new Color(0.4f, 0.6f, 0.2f),
                isUnlocked = false,
                unlockWave = 30,
                coinMultiplier = 2f,
                expMultiplier = 1.8f,
                hasTimeLimit = false,
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "Constant Pressure", description = "Enemies never stop", type = ModifierType.EnemySpawnRate, value = 0.8f, isPositive = false },
                    new GameModifier { name = "Scaling Difficulty", description = "Enemies scale over time", type = ModifierType.EnemyHealth, value = 1.1f, isPositive = false },
                    new GameModifier { name = "Fast Enemies", description = "Enemies move faster", type = ModifierType.EnemySpeed, value = 1.2f, isPositive = false }
                }
            });

            // Special Event (template)
            _gameModes.Add(new GameModeData
            {
                type = GameModeType.PvE_Event,
                name = "Winter Event",
                description = "Limited time event! Special rewards await!",
                icon = "❄️",
                color = new Color(0.3f, 0.7f, 0.9f),
                isUnlocked = false, // Controlled by event system
                unlockWave = 0,
                coinMultiplier = 2f,
                expMultiplier = 2f,
                hasTimeLimit = true,
                timeLimit = 300f,
                modifiers = new List<GameModifier>
                {
                    new GameModifier { name = "Frozen Foes", description = "Enemies have ice armor", type = ModifierType.EnemyHealth, value = 1.5f, isPositive = false },
                    new GameModifier { name = "Slippery", description = "Enemies move faster", type = ModifierType.EnemySpeed, value = 1.3f, isPositive = false },
                    new GameModifier { name = "Holiday Bonus", description = "Double rewards!", type = ModifierType.CoinReward, value = 2f, isPositive = true }
                }
            });
        }

        private void GenerateDailyChallenge()
        {
            DateTime today = DateTime.Today;
            int seed = today.Year * 10000 + today.Month * 100 + today.Day;
            UnityEngine.Random.InitState(seed);

            _todaysChallenge = new DailyChallenge
            {
                date = today,
                seed = seed,
                modifiers = new List<GameModifier>(),
                isCompleted = PlayerPrefs.GetInt($"DailyChallenge_{seed}_Done", 0) == 1
            };

            // Generate random modifiers
            List<string> challengeNames = new List<string>
            {
                "Glass Cannon", "Speed Demons", "Tank Invasion", "Resource Scarcity",
                "Power Surge", "Critical Chaos", "Swarm Attack", "Elite Forces",
                "Iron Defense", "Rapid Fire", "Economy Challenge", "Survival Test"
            };

            _todaysChallenge.name = challengeNames[UnityEngine.Random.Range(0, challengeNames.Count)];

            // Add 2-4 random modifiers
            int modifierCount = UnityEngine.Random.Range(2, 5);
            List<GameModifier> possibleMods = GetRandomModifiers();

            for (int i = 0; i < Mathf.Min(modifierCount, possibleMods.Count); i++)
            {
                int idx = UnityEngine.Random.Range(0, possibleMods.Count);
                _todaysChallenge.modifiers.Add(possibleMods[idx]);
                possibleMods.RemoveAt(idx);
            }

            // Set rewards based on difficulty
            int difficulty = _todaysChallenge.modifiers.FindAll(m => !m.isPositive).Count;
            _todaysChallenge.targetScore = 1000 + difficulty * 500;
            _todaysChallenge.coinReward = 500 + difficulty * 250;
            _todaysChallenge.gemReward = 10 + difficulty * 5;
            _todaysChallenge.playerScore = PlayerPrefs.GetInt($"DailyChallenge_{seed}_Score", 0);

            // Reset random state
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        }

        private List<GameModifier> GetRandomModifiers()
        {
            return new List<GameModifier>
            {
                new GameModifier { name = "Tough Enemies", description = "+50% enemy health", type = ModifierType.EnemyHealth, value = 1.5f, isPositive = false },
                new GameModifier { name = "Speedy Foes", description = "+30% enemy speed", type = ModifierType.EnemySpeed, value = 1.3f, isPositive = false },
                new GameModifier { name = "Rush Hour", description = "Faster enemy spawns", type = ModifierType.EnemySpawnRate, value = 0.6f, isPositive = false },
                new GameModifier { name = "Expensive", description = "+50% defender cost", type = ModifierType.DefenderCost, value = 1.5f, isPositive = false },
                new GameModifier { name = "Weak Attacks", description = "-25% damage", type = ModifierType.DefenderDamage, value = 0.75f, isPositive = false },
                new GameModifier { name = "Limited Lives", description = "Start with 5 lives", type = ModifierType.StartingLives, value = 5f, isPositive = false },
                new GameModifier { name = "No Power-ups", description = "Power-ups don't spawn", type = ModifierType.PowerUpSpawn, value = 0f, isPositive = false },
                new GameModifier { name = "Rich Start", description = "+100 starting coins", type = ModifierType.StartingCoins, value = 100f, isPositive = true },
                new GameModifier { name = "Power Surge", description = "+25% damage", type = ModifierType.DefenderDamage, value = 1.25f, isPositive = true },
                new GameModifier { name = "Lucky", description = "+20% crit chance", type = ModifierType.CriticalChance, value = 0.2f, isPositive = true },
                new GameModifier { name = "Combo King", description = "+50% combo mult", type = ModifierType.ComboMultiplier, value = 1.5f, isPositive = true },
                new GameModifier { name = "Loot Rain", description = "+100% coins", type = ModifierType.CoinReward, value = 2f, isPositive = true }
            };
        }

        public List<GameModeData> GetAllModes() => _gameModes;

        public List<GameModeData> GetUnlockedModes()
        {
            return _gameModes.FindAll(m => m.isUnlocked);
        }

        public GameModeData GetMode(GameModeType type)
        {
            return _gameModes.Find(m => m.type == type);
        }

        public DailyChallenge GetTodaysChallenge() => _todaysChallenge;

        public void SelectMode(GameModeType type)
        {
            _currentMode = type;
            OnModeSelected?.Invoke(type);
        }

        public GameModeType GetCurrentMode() => _currentMode;

        public GameModeData GetCurrentModeData()
        {
            return GetMode(_currentMode);
        }

        public void CheckUnlocks(int highestWave)
        {
            foreach (var mode in _gameModes)
            {
                if (!mode.isUnlocked && highestWave >= mode.unlockWave)
                {
                    mode.isUnlocked = true;
                    Debug.Log($"[GameModes] Unlocked: {mode.name}!");
                }
            }
            SaveProgress();
        }

        public void RecordScore(GameModeType type, int score, int wave)
        {
            GameModeData mode = GetMode(type);
            if (mode == null) return;

            mode.timesPlayed++;
            if (score > mode.highScore) mode.highScore = score;
            if (wave > mode.highWave) mode.highWave = wave;

            if (type == GameModeType.DailyChallenge)
            {
                if (score > _todaysChallenge.playerScore)
                {
                    _todaysChallenge.playerScore = score;
                    PlayerPrefs.SetInt($"DailyChallenge_{_todaysChallenge.seed}_Score", score);
                }

                if (!_todaysChallenge.isCompleted && score >= _todaysChallenge.targetScore)
                {
                    _todaysChallenge.isCompleted = true;
                    PlayerPrefs.SetInt($"DailyChallenge_{_todaysChallenge.seed}_Done", 1);
                    OnDailyChallengeCompleted?.Invoke(_todaysChallenge);
                }
            }

            SaveProgress();
        }

        public float ApplyModifier(ModifierType type, float baseValue)
        {
            GameModeData mode = GetCurrentModeData();
            if (mode == null) return baseValue;

            // Also apply daily challenge modifiers if in that mode
            List<GameModifier> allMods = new List<GameModifier>(mode.modifiers);
            if (_currentMode == GameModeType.DailyChallenge)
            {
                allMods.AddRange(_todaysChallenge.modifiers);
            }

            float result = baseValue;
            foreach (var mod in allMods)
            {
                if (mod.type == type)
                {
                    result *= mod.value;
                }
            }

            return result;
        }

        public bool HasModifier(ModifierType type)
        {
            GameModeData mode = GetCurrentModeData();
            if (mode == null) return false;

            foreach (var mod in mode.modifiers)
            {
                if (mod.type == type) return true;
            }

            if (_currentMode == GameModeType.DailyChallenge)
            {
                foreach (var mod in _todaysChallenge.modifiers)
                {
                    if (mod.type == type) return true;
                }
            }

            return false;
        }

        private void SaveProgress()
        {
            foreach (var mode in _gameModes)
            {
                PlayerPrefs.SetInt($"Mode_{mode.type}_Unlocked", mode.isUnlocked ? 1 : 0);
                PlayerPrefs.SetInt($"Mode_{mode.type}_HighScore", mode.highScore);
                PlayerPrefs.SetInt($"Mode_{mode.type}_HighWave", mode.highWave);
                PlayerPrefs.SetInt($"Mode_{mode.type}_Played", mode.timesPlayed);
            }
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            foreach (var mode in _gameModes)
            {
                if (PlayerPrefs.HasKey($"Mode_{mode.type}_Unlocked"))
                {
                    mode.isUnlocked = PlayerPrefs.GetInt($"Mode_{mode.type}_Unlocked") == 1 || mode.type == GameModeType.Classic;
                    mode.highScore = PlayerPrefs.GetInt($"Mode_{mode.type}_HighScore", 0);
                    mode.highWave = PlayerPrefs.GetInt($"Mode_{mode.type}_HighWave", 0);
                    mode.timesPlayed = PlayerPrefs.GetInt($"Mode_{mode.type}_Played", 0);
                }
            }
        }

        public string GetModeUnlockText(GameModeData mode)
        {
            if (mode.isUnlocked) return "Unlocked";
            return $"Reach Wave {mode.unlockWave} to unlock";
        }
    }
}
