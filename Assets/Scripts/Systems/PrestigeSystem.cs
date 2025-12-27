using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Prestige System - Rebirth mechanics for endless progression.
    /// Reset progress to gain permanent multipliers and exclusive rewards.
    /// </summary>
    public class PrestigeSystem : MonoBehaviour
    {
        public static PrestigeSystem Instance { get; private set; }

        // Events
        public event Action<int> OnPrestige;
        public event Action<PrestigeUpgrade> OnUpgradePurchased;
        public event Action<int> OnPrestigePointsChanged;

        // Constants
        private const int BASE_PRESTIGE_REQUIREMENT = 25; // Wave required for first prestige
        private const float REQUIREMENT_MULTIPLIER = 1.5f; // Each prestige needs more waves

        // State
        private int _prestigeLevel;
        private int _prestigePoints;
        private int _totalPrestigePoints;
        private int _highestWaveThisRun;
        private int _highestWaveEver;
        private List<PrestigeUpgrade> _upgrades = new List<PrestigeUpgrade>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUpgrades();
            LoadProgress();
        }

        private void InitializeUpgrades()
        {
            _upgrades = new List<PrestigeUpgrade>
            {
                // === POWER TIER ===
                new PrestigeUpgrade
                {
                    id = "base_damage",
                    name = "Ancient Power",
                    description = "Increase base damage by {0}%",
                    icon = "⚔️",
                    category = PrestigeCategory.Power,
                    baseValue = 10f,
                    maxLevel = 20,
                    currentLevel = 0,
                    baseCost = 1,
                    costMultiplier = 1.5f
                },
                new PrestigeUpgrade
                {
                    id = "crit_damage",
                    name = "Lethal Strikes",
                    description = "Increase critical damage by {0}%",
                    icon = "💥",
                    category = PrestigeCategory.Power,
                    baseValue = 15f,
                    maxLevel = 15,
                    currentLevel = 0,
                    baseCost = 2,
                    costMultiplier = 1.6f
                },
                new PrestigeUpgrade
                {
                    id = "attack_speed",
                    name = "Swift Assault",
                    description = "Increase attack speed by {0}%",
                    icon = "⚡",
                    category = PrestigeCategory.Power,
                    baseValue = 5f,
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 3,
                    costMultiplier = 2f
                },

                // === DEFENSE TIER ===
                new PrestigeUpgrade
                {
                    id = "health_boost",
                    name = "Eternal Vigor",
                    description = "Increase base health by {0}%",
                    icon = "❤️",
                    category = PrestigeCategory.Defense,
                    baseValue = 15f,
                    maxLevel = 20,
                    currentLevel = 0,
                    baseCost = 1,
                    costMultiplier = 1.5f
                },
                new PrestigeUpgrade
                {
                    id = "damage_reduction",
                    name = "Iron Will",
                    description = "Reduce damage taken by {0}%",
                    icon = "🛡️",
                    category = PrestigeCategory.Defense,
                    baseValue = 3f,
                    maxLevel = 15,
                    currentLevel = 0,
                    baseCost = 2,
                    costMultiplier = 1.8f
                },
                new PrestigeUpgrade
                {
                    id = "regen_rate",
                    name = "Undying",
                    description = "Regenerate {0} HP per second",
                    icon = "💚",
                    category = PrestigeCategory.Defense,
                    baseValue = 1f,
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 3,
                    costMultiplier = 2.2f
                },

                // === ECONOMY TIER ===
                new PrestigeUpgrade
                {
                    id = "coin_multiplier",
                    name = "Golden Touch",
                    description = "Increase coin drops by {0}%",
                    icon = "💰",
                    category = PrestigeCategory.Economy,
                    baseValue = 20f,
                    maxLevel = 25,
                    currentLevel = 0,
                    baseCost = 1,
                    costMultiplier = 1.4f
                },
                new PrestigeUpgrade
                {
                    id = "gem_chance",
                    name = "Jewel Hunter",
                    description = "Increase gem drop chance by {0}%",
                    icon = "💎",
                    category = PrestigeCategory.Economy,
                    baseValue = 5f,
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 5,
                    costMultiplier = 2f
                },
                new PrestigeUpgrade
                {
                    id = "xp_multiplier",
                    name = "Fast Learner",
                    description = "Increase XP gain by {0}%",
                    icon = "📚",
                    category = PrestigeCategory.Economy,
                    baseValue = 10f,
                    maxLevel = 20,
                    currentLevel = 0,
                    baseCost = 2,
                    costMultiplier = 1.6f
                },

                // === SPECIAL TIER ===
                new PrestigeUpgrade
                {
                    id = "starting_wave",
                    name = "Timewarp",
                    description = "Start each run at wave {0}",
                    icon = "⏭️",
                    category = PrestigeCategory.Special,
                    baseValue = 1f,
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 10,
                    costMultiplier = 3f
                },
                new PrestigeUpgrade
                {
                    id = "extra_unit_slot",
                    name = "Army Expansion",
                    description = "Start with {0} extra unit slots",
                    icon = "🎖️",
                    category = PrestigeCategory.Special,
                    baseValue = 1f,
                    maxLevel = 5,
                    currentLevel = 0,
                    baseCost = 15,
                    costMultiplier = 4f
                },
                new PrestigeUpgrade
                {
                    id = "auto_merge",
                    name = "Auto Fusion",
                    description = "Auto-merge units every {0} seconds",
                    icon = "🔄",
                    category = PrestigeCategory.Special,
                    baseValue = 30f,
                    valueDecrement = 2f, // Gets faster with levels
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 20,
                    costMultiplier = 2.5f
                },
                new PrestigeUpgrade
                {
                    id = "prestige_boost",
                    name = "Prestige Master",
                    description = "Gain {0}% more Prestige Points",
                    icon = "🌟",
                    category = PrestigeCategory.Meta,
                    baseValue = 10f,
                    maxLevel = 15,
                    currentLevel = 0,
                    baseCost = 5,
                    costMultiplier = 2f
                },
                new PrestigeUpgrade
                {
                    id = "offline_progress",
                    name = "Idle Master",
                    description = "Earn {0}% coins while offline",
                    icon = "😴",
                    category = PrestigeCategory.Meta,
                    baseValue = 10f,
                    maxLevel = 10,
                    currentLevel = 0,
                    baseCost = 8,
                    costMultiplier = 2.5f
                }
            };
        }

        // === PUBLIC API ===

        public bool CanPrestige()
        {
            return _highestWaveThisRun >= GetPrestigeWaveRequirement();
        }

        public int GetPrestigeWaveRequirement()
        {
            return Mathf.RoundToInt(BASE_PRESTIGE_REQUIREMENT * Mathf.Pow(REQUIREMENT_MULTIPLIER, _prestigeLevel));
        }

        public int GetPotentialPrestigePoints()
        {
            if (!CanPrestige()) return 0;

            int basePoints = _highestWaveThisRun / 5;
            float prestigeBoost = 1f + GetUpgradeValue("prestige_boost") / 100f;
            return Mathf.RoundToInt(basePoints * prestigeBoost);
        }

        public void DoPrestige()
        {
            if (!CanPrestige()) return;

            int pointsGained = GetPotentialPrestigePoints();
            _prestigePoints += pointsGained;
            _totalPrestigePoints += pointsGained;
            _prestigeLevel++;
            
            if (_highestWaveThisRun > _highestWaveEver)
                _highestWaveEver = _highestWaveThisRun;
            
            _highestWaveThisRun = 0;

            // Reset non-prestige progress (coins, current wave, etc.)
            // Keep: Prestige upgrades, heroes, achievements, settings
            ResetRunProgress();

            SaveProgress();
            OnPrestige?.Invoke(_prestigeLevel);
            OnPrestigePointsChanged?.Invoke(_prestigePoints);

            Debug.Log($"[Prestige] Prestige #{_prestigeLevel}! Gained {pointsGained} points. Total: {_prestigePoints}");
        }

        private void ResetRunProgress()
        {
            // Reset coins but keep gems
            PlayerPrefs.SetInt("Coins", 0);
            
            // Reset gameplay stats
            PlayerPrefs.SetInt("CurrentWave", GetStartingWave());
            PlayerPrefs.SetInt("EnemiesKilled", 0);
            PlayerPrefs.SetInt("BossesKilled", 0);
            PlayerPrefs.SetInt("TotalMerges", 0);
            
            // Keep:
            // - Gems
            // - Heroes (unlocked, levels)
            // - Achievements
            // - Settings
            // - Battle Pass progress
            // - Prestige upgrades

            PlayerPrefs.Save();
        }

        public void RecordWave(int wave)
        {
            if (wave > _highestWaveThisRun)
            {
                _highestWaveThisRun = wave;
                if (wave > _highestWaveEver)
                    _highestWaveEver = wave;
                SaveProgress();
            }
        }

        // === UPGRADES ===

        public bool CanPurchaseUpgrade(string id)
        {
            var upgrade = GetUpgrade(id);
            if (upgrade == null || upgrade.currentLevel >= upgrade.maxLevel) return false;
            return _prestigePoints >= GetUpgradeCost(id);
        }

        public int GetUpgradeCost(string id)
        {
            var upgrade = GetUpgrade(id);
            if (upgrade == null) return int.MaxValue;
            return Mathf.RoundToInt(upgrade.baseCost * Mathf.Pow(upgrade.costMultiplier, upgrade.currentLevel));
        }

        public bool TryPurchaseUpgrade(string id)
        {
            if (!CanPurchaseUpgrade(id)) return false;

            var upgrade = GetUpgrade(id);
            int cost = GetUpgradeCost(id);

            _prestigePoints -= cost;
            upgrade.currentLevel++;

            SaveProgress();
            OnUpgradePurchased?.Invoke(upgrade);
            OnPrestigePointsChanged?.Invoke(_prestigePoints);

            Debug.Log($"[Prestige] Purchased {upgrade.name} Lv.{upgrade.currentLevel} for {cost} points");
            return true;
        }

        public float GetUpgradeValue(string id)
        {
            var upgrade = GetUpgrade(id);
            if (upgrade == null || upgrade.currentLevel == 0) return 0f;

            if (upgrade.valueDecrement > 0)
            {
                // Value decreases (like cooldown reduction)
                return upgrade.baseValue - (upgrade.valueDecrement * (upgrade.currentLevel - 1));
            }
            return upgrade.baseValue * upgrade.currentLevel;
        }

        public PrestigeUpgrade GetUpgrade(string id)
        {
            return _upgrades.Find(u => u.id == id);
        }

        public List<PrestigeUpgrade> GetUpgradesByCategory(PrestigeCategory category)
        {
            return _upgrades.FindAll(u => u.category == category);
        }

        public List<PrestigeUpgrade> GetAllUpgrades() => new List<PrestigeUpgrade>(_upgrades);

        // === BONUS GETTERS (for gameplay integration) ===

        public float GetDamageMultiplier() => 1f + GetUpgradeValue("base_damage") / 100f;
        public float GetCritDamageBonus() => GetUpgradeValue("crit_damage");
        public float GetAttackSpeedMultiplier() => 1f + GetUpgradeValue("attack_speed") / 100f;
        public float GetHealthMultiplier() => 1f + GetUpgradeValue("health_boost") / 100f;
        public float GetDamageReduction() => GetUpgradeValue("damage_reduction") / 100f;
        public float GetRegenRate() => GetUpgradeValue("regen_rate");
        public float GetCoinMultiplier() => 1f + GetUpgradeValue("coin_multiplier") / 100f;
        public float GetGemDropChanceBonus() => GetUpgradeValue("gem_chance") / 100f;
        public float GetXPMultiplier() => 1f + GetUpgradeValue("xp_multiplier") / 100f;
        public int GetStartingWave() => 1 + (int)GetUpgradeValue("starting_wave");
        public int GetExtraUnitSlots() => (int)GetUpgradeValue("extra_unit_slot");
        public float GetAutoMergeInterval() => GetUpgradeValue("auto_merge"); // Returns seconds, 0 if not unlocked
        public float GetOfflineProgressRate() => GetUpgradeValue("offline_progress") / 100f;

        // === GETTERS ===

        public int PrestigeLevel => _prestigeLevel;
        public int PrestigePoints => _prestigePoints;
        public int TotalPrestigePoints => _totalPrestigePoints;
        public int HighestWaveThisRun => _highestWaveThisRun;
        public int HighestWaveEver => _highestWaveEver;

        public string GetPrestigeTitle()
        {
            if (_prestigeLevel == 0) return "Novice";
            if (_prestigeLevel < 3) return "Apprentice";
            if (_prestigeLevel < 5) return "Warrior";
            if (_prestigeLevel < 10) return "Champion";
            if (_prestigeLevel < 15) return "Hero";
            if (_prestigeLevel < 25) return "Legend";
            if (_prestigeLevel < 50) return "Mythic";
            return "Transcendent";
        }

        public Color GetPrestigeColor()
        {
            if (_prestigeLevel == 0) return new Color(0.5f, 0.5f, 0.5f);
            if (_prestigeLevel < 3) return new Color(0.2f, 0.8f, 0.3f);
            if (_prestigeLevel < 5) return new Color(0.3f, 0.6f, 1f);
            if (_prestigeLevel < 10) return new Color(0.8f, 0.3f, 0.8f);
            if (_prestigeLevel < 15) return new Color(1f, 0.6f, 0.2f);
            if (_prestigeLevel < 25) return new Color(1f, 0.85f, 0.2f);
            if (_prestigeLevel < 50) return new Color(1f, 0.2f, 0.3f);
            return new Color(1f, 1f, 1f);
        }

        public int GetTotalUpgradesPurchased()
        {
            int total = 0;
            foreach (var upgrade in _upgrades)
                total += upgrade.currentLevel;
            return total;
        }

        // === PERSISTENCE ===

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("Prestige_Level", _prestigeLevel);
            PlayerPrefs.SetInt("Prestige_Points", _prestigePoints);
            PlayerPrefs.SetInt("Prestige_TotalPoints", _totalPrestigePoints);
            PlayerPrefs.SetInt("Prestige_HighWaveRun", _highestWaveThisRun);
            PlayerPrefs.SetInt("Prestige_HighWaveEver", _highestWaveEver);

            foreach (var upgrade in _upgrades)
            {
                PlayerPrefs.SetInt($"Prestige_Upgrade_{upgrade.id}", upgrade.currentLevel);
            }

            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _prestigeLevel = PlayerPrefs.GetInt("Prestige_Level", 0);
            _prestigePoints = PlayerPrefs.GetInt("Prestige_Points", 0);
            _totalPrestigePoints = PlayerPrefs.GetInt("Prestige_TotalPoints", 0);
            _highestWaveThisRun = PlayerPrefs.GetInt("Prestige_HighWaveRun", 0);
            _highestWaveEver = PlayerPrefs.GetInt("Prestige_HighWaveEver", 0);

            foreach (var upgrade in _upgrades)
            {
                upgrade.currentLevel = PlayerPrefs.GetInt($"Prestige_Upgrade_{upgrade.id}", 0);
            }
        }
    }

    // === DATA CLASSES ===

    public enum PrestigeCategory
    {
        Power,
        Defense,
        Economy,
        Special,
        Meta
    }

    [Serializable]
    public class PrestigeUpgrade
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public PrestigeCategory category;
        public float baseValue;
        public float valueDecrement; // For values that decrease per level (cooldowns)
        public int maxLevel;
        public int currentLevel;
        public int baseCost;
        public float costMultiplier;

        public float CurrentValue => valueDecrement > 0 
            ? baseValue - (valueDecrement * (currentLevel - 1)) 
            : baseValue * currentLevel;
        
        public bool IsMaxed => currentLevel >= maxLevel;
        public float Progress => (float)currentLevel / maxLevel;
    }
}
