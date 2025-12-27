using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Quest System - Daily, Weekly, and Special missions for player engagement.
    /// Provides short-term goals with rewards to keep players coming back.
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        // Events
        public event Action<Quest> OnQuestProgress;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestRewardClaimed;
        public event Action OnDailyQuestsRefreshed;
        public event Action OnWeeklyQuestsRefreshed;

        // Constants
        private const int DAILY_QUESTS_COUNT = 5;
        private const int WEEKLY_QUESTS_COUNT = 3;
        private const int SPECIAL_QUESTS_COUNT = 3;

        // State
        private List<Quest> _dailyQuests = new List<Quest>();
        private List<Quest> _weeklyQuests = new List<Quest>();
        private List<Quest> _specialQuests = new List<Quest>();
        private DateTime _lastDailyRefresh;
        private DateTime _lastWeeklyRefresh;

        // Quest templates
        private List<QuestTemplate> _dailyTemplates = new List<QuestTemplate>();
        private List<QuestTemplate> _weeklyTemplates = new List<QuestTemplate>();
        private List<QuestTemplate> _specialTemplates = new List<QuestTemplate>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeQuestTemplates();
            LoadProgress();
            CheckRefresh();
        }

        private void InitializeQuestTemplates()
        {
            // === DAILY QUEST TEMPLATES ===
            _dailyTemplates.AddRange(new[]
            {
                new QuestTemplate
                {
                    type = QuestType.KillEnemies,
                    name = "Warrior's Path",
                    description = "Defeat {0} enemies",
                    icon = "⚔️",
                    targetMin = 50, targetMax = 150,
                    coinRewardMin = 200, coinRewardMax = 500,
                    gemRewardMin = 0, gemRewardMax = 5,
                    xpReward = 50
                },
                new QuestTemplate
                {
                    type = QuestType.ReachWave,
                    name = "Wave Crusher",
                    description = "Reach wave {0}",
                    icon = "🌊",
                    targetMin = 5, targetMax = 15,
                    coinRewardMin = 300, coinRewardMax = 600,
                    gemRewardMin = 5, gemRewardMax = 10,
                    xpReward = 75
                },
                new QuestTemplate
                {
                    type = QuestType.CollectCoins,
                    name = "Gold Rush",
                    description = "Collect {0} coins in battles",
                    icon = "💰",
                    targetMin = 500, targetMax = 2000,
                    coinRewardMin = 100, coinRewardMax = 300,
                    gemRewardMin = 0, gemRewardMax = 3,
                    xpReward = 40
                },
                new QuestTemplate
                {
                    type = QuestType.MergeUnits,
                    name = "Fusion Master",
                    description = "Merge units {0} times",
                    icon = "🔄",
                    targetMin = 10, targetMax = 30,
                    coinRewardMin = 250, coinRewardMax = 450,
                    gemRewardMin = 3, gemRewardMax = 8,
                    xpReward = 60
                },
                new QuestTemplate
                {
                    type = QuestType.PlayGames,
                    name = "Dedicated Player",
                    description = "Complete {0} battles",
                    icon = "🎮",
                    targetMin = 3, targetMax = 7,
                    coinRewardMin = 300, coinRewardMax = 500,
                    gemRewardMin = 5, gemRewardMax = 10,
                    xpReward = 80
                },
                new QuestTemplate
                {
                    type = QuestType.UseSkills,
                    name = "Skill User",
                    description = "Use hero skills {0} times",
                    icon = "⚡",
                    targetMin = 5, targetMax = 15,
                    coinRewardMin = 200, coinRewardMax = 400,
                    gemRewardMin = 2, gemRewardMax = 5,
                    xpReward = 45
                },
                new QuestTemplate
                {
                    type = QuestType.KillBosses,
                    name = "Boss Hunter",
                    description = "Defeat {0} bosses",
                    icon = "👹",
                    targetMin = 1, targetMax = 5,
                    coinRewardMin = 400, coinRewardMax = 800,
                    gemRewardMin = 8, gemRewardMax = 15,
                    xpReward = 100
                },
                new QuestTemplate
                {
                    type = QuestType.AchieveCombo,
                    name = "Combo King",
                    description = "Reach a combo of {0}x",
                    icon = "🔥",
                    targetMin = 10, targetMax = 50,
                    coinRewardMin = 350, coinRewardMax = 550,
                    gemRewardMin = 5, gemRewardMax = 10,
                    xpReward = 70
                },
                new QuestTemplate
                {
                    type = QuestType.SpendCoins,
                    name = "Big Spender",
                    description = "Spend {0} coins",
                    icon = "🛒",
                    targetMin = 500, targetMax = 2000,
                    coinRewardMin = 100, coinRewardMax = 200,
                    gemRewardMin = 5, gemRewardMax = 12,
                    xpReward = 55
                },
                new QuestTemplate
                {
                    type = QuestType.UpgradeUnits,
                    name = "Power Up",
                    description = "Upgrade units {0} times",
                    icon = "📈",
                    targetMin = 5, targetMax = 20,
                    coinRewardMin = 280, coinRewardMax = 480,
                    gemRewardMin = 4, gemRewardMax = 8,
                    xpReward = 65
                }
            });

            // === WEEKLY QUEST TEMPLATES ===
            _weeklyTemplates.AddRange(new[]
            {
                new QuestTemplate
                {
                    type = QuestType.KillEnemies,
                    name = "Weekly Slayer",
                    description = "Defeat {0} enemies this week",
                    icon = "⚔️",
                    targetMin = 500, targetMax = 1500,
                    coinRewardMin = 2000, coinRewardMax = 5000,
                    gemRewardMin = 30, gemRewardMax = 60,
                    xpReward = 500
                },
                new QuestTemplate
                {
                    type = QuestType.ReachWave,
                    name = "Wave Champion",
                    description = "Reach wave {0} in any mode",
                    icon = "🏆",
                    targetMin = 25, targetMax = 50,
                    coinRewardMin = 3000, coinRewardMax = 6000,
                    gemRewardMin = 50, gemRewardMax = 100,
                    xpReward = 750
                },
                new QuestTemplate
                {
                    type = QuestType.KillBosses,
                    name = "Boss Slayer",
                    description = "Defeat {0} bosses this week",
                    icon = "👹",
                    targetMin = 10, targetMax = 25,
                    coinRewardMin = 2500, coinRewardMax = 4500,
                    gemRewardMin = 40, gemRewardMax = 80,
                    xpReward = 600
                },
                new QuestTemplate
                {
                    type = QuestType.PlayGames,
                    name = "Weekly Warrior",
                    description = "Complete {0} battles this week",
                    icon = "🎮",
                    targetMin = 20, targetMax = 40,
                    coinRewardMin = 2000, coinRewardMax = 4000,
                    gemRewardMin = 35, gemRewardMax = 70,
                    xpReward = 550
                },
                new QuestTemplate
                {
                    type = QuestType.CollectCoins,
                    name = "Treasure Hunter",
                    description = "Collect {0} coins in battles",
                    icon = "💎",
                    targetMin = 10000, targetMax = 30000,
                    coinRewardMin = 1500, coinRewardMax = 3000,
                    gemRewardMin = 25, gemRewardMax = 50,
                    xpReward = 400
                }
            });

            // === SPECIAL QUEST TEMPLATES ===
            _specialTemplates.AddRange(new[]
            {
                new QuestTemplate
                {
                    type = QuestType.UnlockHero,
                    name = "Hero Collector",
                    description = "Unlock a new hero",
                    icon = "🦸",
                    targetMin = 1, targetMax = 1,
                    coinRewardMin = 1000, coinRewardMax = 1000,
                    gemRewardMin = 50, gemRewardMax = 50,
                    xpReward = 200
                },
                new QuestTemplate
                {
                    type = QuestType.MaxHero,
                    name = "Legendary Trainer",
                    description = "Max out a hero's level",
                    icon = "⭐",
                    targetMin = 1, targetMax = 1,
                    coinRewardMin = 5000, coinRewardMax = 5000,
                    gemRewardMin = 100, gemRewardMax = 100,
                    xpReward = 500
                },
                new QuestTemplate
                {
                    type = QuestType.CompleteBattlePass,
                    name = "Season Champion",
                    description = "Reach Battle Pass level {0}",
                    icon = "🏅",
                    targetMin = 25, targetMax = 50,
                    coinRewardMin = 3000, coinRewardMax = 10000,
                    gemRewardMin = 75, gemRewardMax = 200,
                    xpReward = 1000
                }
            });
        }

        private void CheckRefresh()
        {
            DateTime now = DateTime.UtcNow;
            DateTime todayReset = now.Date.AddHours(0); // Midnight UTC

            // Daily refresh
            if (_lastDailyRefresh < todayReset || _dailyQuests.Count == 0)
            {
                GenerateDailyQuests();
                _lastDailyRefresh = now;
                OnDailyQuestsRefreshed?.Invoke();
            }

            // Weekly refresh (Monday)
            DateTime thisMonday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).Date;
            if (_lastWeeklyRefresh < thisMonday || _weeklyQuests.Count == 0)
            {
                GenerateWeeklyQuests();
                _lastWeeklyRefresh = now;
                OnWeeklyQuestsRefreshed?.Invoke();
            }

            // Special quests don't refresh automatically
            if (_specialQuests.Count == 0)
            {
                GenerateSpecialQuests();
            }

            SaveProgress();
        }

        private void GenerateDailyQuests()
        {
            _dailyQuests.Clear();

            // Shuffle and pick daily quests
            List<QuestTemplate> shuffled = new List<QuestTemplate>(_dailyTemplates);
            ShuffleList(shuffled);

            for (int i = 0; i < DAILY_QUESTS_COUNT && i < shuffled.Count; i++)
            {
                _dailyQuests.Add(CreateQuestFromTemplate(shuffled[i], QuestCategory.Daily, i));
            }

            Debug.Log($"[Quests] Generated {_dailyQuests.Count} daily quests");
        }

        private void GenerateWeeklyQuests()
        {
            _weeklyQuests.Clear();

            List<QuestTemplate> shuffled = new List<QuestTemplate>(_weeklyTemplates);
            ShuffleList(shuffled);

            for (int i = 0; i < WEEKLY_QUESTS_COUNT && i < shuffled.Count; i++)
            {
                _weeklyQuests.Add(CreateQuestFromTemplate(shuffled[i], QuestCategory.Weekly, i));
            }

            Debug.Log($"[Quests] Generated {_weeklyQuests.Count} weekly quests");
        }

        private void GenerateSpecialQuests()
        {
            _specialQuests.Clear();

            for (int i = 0; i < SPECIAL_QUESTS_COUNT && i < _specialTemplates.Count; i++)
            {
                _specialQuests.Add(CreateQuestFromTemplate(_specialTemplates[i], QuestCategory.Special, i));
            }

            Debug.Log($"[Quests] Generated {_specialQuests.Count} special quests");
        }

        private Quest CreateQuestFromTemplate(QuestTemplate template, QuestCategory category, int index)
        {
            int target = UnityEngine.Random.Range(template.targetMin, template.targetMax + 1);
            
            // Round target to nice numbers
            if (target >= 100) target = (target / 50) * 50;
            else if (target >= 10) target = (target / 5) * 5;

            return new Quest
            {
                id = $"{category}_{template.type}_{index}",
                type = template.type,
                category = category,
                name = template.name,
                description = string.Format(template.description, target),
                icon = template.icon,
                targetAmount = target,
                currentProgress = 0,
                coinReward = UnityEngine.Random.Range(template.coinRewardMin, template.coinRewardMax + 1),
                gemReward = UnityEngine.Random.Range(template.gemRewardMin, template.gemRewardMax + 1),
                xpReward = template.xpReward,
                isCompleted = false,
                isRewardClaimed = false
            };
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        // === PUBLIC API - Progress Updates ===

        public void OnEnemyKilled(int count = 1)
        {
            UpdateProgress(QuestType.KillEnemies, count);
        }

        public void OnWaveReached(int wave)
        {
            UpdateProgressMax(QuestType.ReachWave, wave);
        }

        public void OnCoinsCollected(int amount)
        {
            UpdateProgress(QuestType.CollectCoins, amount);
        }

        public void OnMerge(int count = 1)
        {
            UpdateProgress(QuestType.MergeUnits, count);
        }

        public void OnGameCompleted()
        {
            UpdateProgress(QuestType.PlayGames, 1);
        }

        public void OnSkillUsed(int count = 1)
        {
            UpdateProgress(QuestType.UseSkills, count);
        }

        public void OnBossKilled(int count = 1)
        {
            UpdateProgress(QuestType.KillBosses, count);
        }

        public void OnComboReached(int combo)
        {
            UpdateProgressMax(QuestType.AchieveCombo, combo);
        }

        public void OnCoinsSpent(int amount)
        {
            UpdateProgress(QuestType.SpendCoins, amount);
        }

        public void OnUpgrade(int count = 1)
        {
            UpdateProgress(QuestType.UpgradeUnits, count);
        }

        public void OnHeroUnlocked()
        {
            UpdateProgress(QuestType.UnlockHero, 1);
        }

        public void OnHeroMaxed()
        {
            UpdateProgress(QuestType.MaxHero, 1);
        }

        public void OnBattlePassLevel(int level)
        {
            UpdateProgressMax(QuestType.CompleteBattlePass, level);
        }

        private void UpdateProgress(QuestType type, int amount)
        {
            UpdateQuestList(_dailyQuests, type, amount, false);
            UpdateQuestList(_weeklyQuests, type, amount, false);
            UpdateQuestList(_specialQuests, type, amount, false);
            SaveProgress();
        }

        private void UpdateProgressMax(QuestType type, int value)
        {
            UpdateQuestList(_dailyQuests, type, value, true);
            UpdateQuestList(_weeklyQuests, type, value, true);
            UpdateQuestList(_specialQuests, type, value, true);
            SaveProgress();
        }

        private void UpdateQuestList(List<Quest> quests, QuestType type, int amount, bool isMaxValue)
        {
            foreach (var quest in quests)
            {
                if (quest.type == type && !quest.isCompleted)
                {
                    if (isMaxValue)
                    {
                        if (amount > quest.currentProgress)
                            quest.currentProgress = amount;
                    }
                    else
                    {
                        quest.currentProgress += amount;
                    }

                    OnQuestProgress?.Invoke(quest);

                    if (quest.currentProgress >= quest.targetAmount)
                    {
                        quest.isCompleted = true;
                        OnQuestCompleted?.Invoke(quest);
                        Debug.Log($"[Quests] Quest completed: {quest.name}");
                    }
                }
            }
        }

        // === PUBLIC API - Claiming ===

        public bool CanClaimReward(string questId)
        {
            var quest = GetQuestById(questId);
            return quest != null && quest.isCompleted && !quest.isRewardClaimed;
        }

        public Quest ClaimReward(string questId, ref int coins, ref int gems)
        {
            var quest = GetQuestById(questId);
            if (quest == null || !quest.isCompleted || quest.isRewardClaimed) return null;

            quest.isRewardClaimed = true;
            coins += quest.coinReward;
            gems += quest.gemReward;
            BattlePassSystem.Instance?.AddXP(quest.xpReward);

            OnQuestRewardClaimed?.Invoke(quest);
            SaveProgress();

            Debug.Log($"[Quests] Claimed: {quest.coinReward} coins, {quest.gemReward} gems, {quest.xpReward} XP");
            return quest;
        }

        public void ClaimAllAvailable(ref int coins, ref int gems)
        {
            foreach (var quest in GetAllQuests())
            {
                if (quest.isCompleted && !quest.isRewardClaimed)
                {
                    ClaimReward(quest.id, ref coins, ref gems);
                }
            }
        }

        // === GETTERS ===

        public List<Quest> GetDailyQuests() => new List<Quest>(_dailyQuests);
        public List<Quest> GetWeeklyQuests() => new List<Quest>(_weeklyQuests);
        public List<Quest> GetSpecialQuests() => new List<Quest>(_specialQuests);

        public List<Quest> GetAllQuests()
        {
            var all = new List<Quest>();
            all.AddRange(_dailyQuests);
            all.AddRange(_weeklyQuests);
            all.AddRange(_specialQuests);
            return all;
        }

        public Quest GetQuestById(string id)
        {
            foreach (var q in _dailyQuests) if (q.id == id) return q;
            foreach (var q in _weeklyQuests) if (q.id == id) return q;
            foreach (var q in _specialQuests) if (q.id == id) return q;
            return null;
        }

        public int GetClaimableCount()
        {
            int count = 0;
            foreach (var q in GetAllQuests())
                if (q.isCompleted && !q.isRewardClaimed) count++;
            return count;
        }

        public int GetCompletedDailyCount()
        {
            int count = 0;
            foreach (var q in _dailyQuests) if (q.isCompleted) count++;
            return count;
        }

        public int GetCompletedWeeklyCount()
        {
            int count = 0;
            foreach (var q in _weeklyQuests) if (q.isCompleted) count++;
            return count;
        }

        public float GetDailyProgress()
        {
            if (_dailyQuests.Count == 0) return 0f;
            return (float)GetCompletedDailyCount() / _dailyQuests.Count;
        }

        public float GetWeeklyProgress()
        {
            if (_weeklyQuests.Count == 0) return 0f;
            return (float)GetCompletedWeeklyCount() / _weeklyQuests.Count;
        }

        public string GetTimeUntilDailyReset()
        {
            DateTime nextReset = DateTime.UtcNow.Date.AddDays(1);
            TimeSpan remaining = nextReset - DateTime.UtcNow;
            return $"{remaining.Hours}h {remaining.Minutes}m";
        }

        public string GetTimeUntilWeeklyReset()
        {
            DateTime now = DateTime.UtcNow;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            DateTime nextReset = now.Date.AddDays(daysUntilMonday);
            TimeSpan remaining = nextReset - now;
            return $"{remaining.Days}d {remaining.Hours}h";
        }

        // === PERSISTENCE ===

        private void SaveProgress()
        {
            PlayerPrefs.SetString("Quest_DailyRefresh", _lastDailyRefresh.ToString("O"));
            PlayerPrefs.SetString("Quest_WeeklyRefresh", _lastWeeklyRefresh.ToString("O"));

            SaveQuestList(_dailyQuests, "Daily");
            SaveQuestList(_weeklyQuests, "Weekly");
            SaveQuestList(_specialQuests, "Special");

            PlayerPrefs.Save();
        }

        private void SaveQuestList(List<Quest> quests, string prefix)
        {
            PlayerPrefs.SetInt($"Quest_{prefix}_Count", quests.Count);
            for (int i = 0; i < quests.Count; i++)
            {
                var q = quests[i];
                PlayerPrefs.SetString($"Quest_{prefix}_{i}_id", q.id);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_type", (int)q.type);
                PlayerPrefs.SetString($"Quest_{prefix}_{i}_name", q.name);
                PlayerPrefs.SetString($"Quest_{prefix}_{i}_desc", q.description);
                PlayerPrefs.SetString($"Quest_{prefix}_{i}_icon", q.icon);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_target", q.targetAmount);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_progress", q.currentProgress);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_coins", q.coinReward);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_gems", q.gemReward);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_xp", q.xpReward);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_completed", q.isCompleted ? 1 : 0);
                PlayerPrefs.SetInt($"Quest_{prefix}_{i}_claimed", q.isRewardClaimed ? 1 : 0);
            }
        }

        private void LoadProgress()
        {
            string dailyRefreshStr = PlayerPrefs.GetString("Quest_DailyRefresh", "");
            string weeklyRefreshStr = PlayerPrefs.GetString("Quest_WeeklyRefresh", "");

            if (!string.IsNullOrEmpty(dailyRefreshStr))
                DateTime.TryParse(dailyRefreshStr, out _lastDailyRefresh);
            if (!string.IsNullOrEmpty(weeklyRefreshStr))
                DateTime.TryParse(weeklyRefreshStr, out _lastWeeklyRefresh);

            LoadQuestList(_dailyQuests, "Daily", QuestCategory.Daily);
            LoadQuestList(_weeklyQuests, "Weekly", QuestCategory.Weekly);
            LoadQuestList(_specialQuests, "Special", QuestCategory.Special);
        }

        private void LoadQuestList(List<Quest> quests, string prefix, QuestCategory category)
        {
            quests.Clear();
            int count = PlayerPrefs.GetInt($"Quest_{prefix}_Count", 0);

            for (int i = 0; i < count; i++)
            {
                quests.Add(new Quest
                {
                    id = PlayerPrefs.GetString($"Quest_{prefix}_{i}_id", ""),
                    type = (QuestType)PlayerPrefs.GetInt($"Quest_{prefix}_{i}_type", 0),
                    category = category,
                    name = PlayerPrefs.GetString($"Quest_{prefix}_{i}_name", ""),
                    description = PlayerPrefs.GetString($"Quest_{prefix}_{i}_desc", ""),
                    icon = PlayerPrefs.GetString($"Quest_{prefix}_{i}_icon", ""),
                    targetAmount = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_target", 0),
                    currentProgress = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_progress", 0),
                    coinReward = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_coins", 0),
                    gemReward = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_gems", 0),
                    xpReward = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_xp", 0),
                    isCompleted = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_completed", 0) == 1,
                    isRewardClaimed = PlayerPrefs.GetInt($"Quest_{prefix}_{i}_claimed", 0) == 1
                });
            }
        }
    }

    // === DATA CLASSES ===

    public enum QuestType
    {
        KillEnemies,
        ReachWave,
        CollectCoins,
        MergeUnits,
        PlayGames,
        UseSkills,
        KillBosses,
        AchieveCombo,
        SpendCoins,
        UpgradeUnits,
        UnlockHero,
        MaxHero,
        CompleteBattlePass,
        WatchAds,
        ShareScore,
        JoinClan
    }

    public enum QuestCategory
    {
        Daily,
        Weekly,
        Special,
        Event
    }

    [Serializable]
    public class Quest
    {
        public string id;
        public QuestType type;
        public QuestCategory category;
        public string name;
        public string description;
        public string icon;
        public int targetAmount;
        public int currentProgress;
        public int coinReward;
        public int gemReward;
        public int xpReward;
        public bool isCompleted;
        public bool isRewardClaimed;

        public float Progress => targetAmount > 0 ? (float)currentProgress / targetAmount : 0f;
        public string ProgressText => $"{currentProgress}/{targetAmount}";
    }

    [Serializable]
    public class QuestTemplate
    {
        public QuestType type;
        public string name;
        public string description;
        public string icon;
        public int targetMin;
        public int targetMax;
        public int coinRewardMin;
        public int coinRewardMax;
        public int gemRewardMin;
        public int gemRewardMax;
        public int xpReward;
    }
}
