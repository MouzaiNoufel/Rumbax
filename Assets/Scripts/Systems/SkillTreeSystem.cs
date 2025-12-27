using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Skill Tree / Permanent Upgrades System - Buy permanent stat boosts with coins/gems.
    /// Professional mobile game feature.
    /// </summary>
    [Serializable]
    public class SkillNode
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public SkillCategory category;
        public int maxLevel;
        public int currentLevel;
        public int[] costsPerLevel;
        public bool usesGems;
        public float[] valuesPerLevel;
        public string[] prerequisiteIds;
    }

    public enum SkillCategory
    {
        Attack,
        Defense,
        Economy,
        Special,
        Ultimate
    }

    public class SkillTreeSystem : MonoBehaviour
    {
        public static SkillTreeSystem Instance { get; private set; }

        private List<SkillNode> _skills = new List<SkillNode>();

        // Events
        public event Action<SkillNode> OnSkillUpgraded;
        public event Action OnSkillsReset;

        // Category colors
        public static readonly Dictionary<SkillCategory, Color> CategoryColors = new Dictionary<SkillCategory, Color>
        {
            { SkillCategory.Attack, new Color(1f, 0.3f, 0.3f) },
            { SkillCategory.Defense, new Color(0.3f, 0.7f, 1f) },
            { SkillCategory.Economy, new Color(1f, 0.85f, 0.2f) },
            { SkillCategory.Special, new Color(0.7f, 0.3f, 1f) },
            { SkillCategory.Ultimate, new Color(1f, 0.5f, 0.8f) }
        };

        public static readonly Dictionary<SkillCategory, string> CategoryIcons = new Dictionary<SkillCategory, string>
        {
            { SkillCategory.Attack, "⚔️" },
            { SkillCategory.Defense, "🛡️" },
            { SkillCategory.Economy, "💰" },
            { SkillCategory.Special, "✨" },
            { SkillCategory.Ultimate, "⚡" }
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSkills();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSkills()
        {
            _skills.Clear();

            // === ATTACK SKILLS ===
            _skills.Add(new SkillNode
            {
                id = "damage_boost",
                name = "Damage Boost",
                description = "Increase all defender damage by {0}%",
                icon = "🗡️",
                category = SkillCategory.Attack,
                maxLevel = 20,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(100, 50, 20),
                usesGems = false,
                valuesPerLevel = GenerateValues(5f, 5f, 20),
                prerequisiteIds = new string[] { }
            });

            _skills.Add(new SkillNode
            {
                id = "attack_speed",
                name = "Attack Speed",
                description = "Increase attack speed by {0}%",
                icon = "⚡",
                category = SkillCategory.Attack,
                maxLevel = 15,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(150, 75, 15),
                usesGems = false,
                valuesPerLevel = GenerateValues(3f, 3f, 15),
                prerequisiteIds = new string[] { "damage_boost" }
            });

            _skills.Add(new SkillNode
            {
                id = "critical_chance",
                name = "Critical Chance",
                description = "Increase critical hit chance by {0}%",
                icon = "💥",
                category = SkillCategory.Attack,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(200, 100, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(2f, 2f, 10),
                prerequisiteIds = new string[] { "damage_boost" }
            });

            _skills.Add(new SkillNode
            {
                id = "critical_damage",
                name = "Critical Damage",
                description = "Increase critical damage multiplier by {0}%",
                icon = "💢",
                category = SkillCategory.Attack,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(250, 125, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 10),
                prerequisiteIds = new string[] { "critical_chance" }
            });

            _skills.Add(new SkillNode
            {
                id = "attack_range",
                name = "Attack Range",
                description = "Increase attack range by {0}%",
                icon = "🎯",
                category = SkillCategory.Attack,
                maxLevel = 8,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(300, 150, 8),
                usesGems = false,
                valuesPerLevel = GenerateValues(5f, 5f, 8),
                prerequisiteIds = new string[] { "attack_speed" }
            });

            _skills.Add(new SkillNode
            {
                id = "multi_shot",
                name = "Multi-Shot",
                description = "Chance to hit {0} additional targets",
                icon = "🎆",
                category = SkillCategory.Attack,
                maxLevel = 3,
                currentLevel = 0,
                costsPerLevel = new int[] { 1000, 2500, 5000 },
                usesGems = false,
                valuesPerLevel = new float[] { 1f, 2f, 3f },
                prerequisiteIds = new string[] { "attack_range", "critical_damage" }
            });

            // === DEFENSE SKILLS ===
            _skills.Add(new SkillNode
            {
                id = "extra_lives",
                name = "Extra Lives",
                description = "Start with {0} additional lives",
                icon = "❤️",
                category = SkillCategory.Defense,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 500, 1000, 2000, 4000, 8000 },
                usesGems = false,
                valuesPerLevel = new float[] { 1f, 2f, 3f, 4f, 5f },
                prerequisiteIds = new string[] { }
            });

            _skills.Add(new SkillNode
            {
                id = "armor",
                name = "Armor",
                description = "Reduce damage to lives by {0}%",
                icon = "🛡️",
                category = SkillCategory.Defense,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(200, 100, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(5f, 5f, 10),
                prerequisiteIds = new string[] { "extra_lives" }
            });

            _skills.Add(new SkillNode
            {
                id = "shield",
                name = "Energy Shield",
                description = "{0}% chance to block enemy damage",
                icon = "🔮",
                category = SkillCategory.Defense,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 1000, 2000, 4000, 8000, 15000 },
                usesGems = false,
                valuesPerLevel = new float[] { 5f, 10f, 15f, 20f, 25f },
                prerequisiteIds = new string[] { "armor" }
            });

            _skills.Add(new SkillNode
            {
                id = "regen",
                name = "Regeneration",
                description = "Recover 1 life every {0} waves",
                icon = "💚",
                category = SkillCategory.Defense,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 2000, 4000, 8000, 15000, 30000 },
                usesGems = false,
                valuesPerLevel = new float[] { 10f, 8f, 6f, 4f, 3f },
                prerequisiteIds = new string[] { "shield" }
            });

            // === ECONOMY SKILLS ===
            _skills.Add(new SkillNode
            {
                id = "coin_boost",
                name = "Coin Boost",
                description = "Earn {0}% more coins",
                icon = "💰",
                category = SkillCategory.Economy,
                maxLevel = 20,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(100, 50, 20),
                usesGems = false,
                valuesPerLevel = GenerateValues(5f, 5f, 20),
                prerequisiteIds = new string[] { }
            });

            _skills.Add(new SkillNode
            {
                id = "gem_finder",
                name = "Gem Finder",
                description = "{0}% chance to find gems from enemies",
                icon = "💎",
                category = SkillCategory.Economy,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(300, 150, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(1f, 1f, 10),
                prerequisiteIds = new string[] { "coin_boost" }
            });

            _skills.Add(new SkillNode
            {
                id = "starting_coins",
                name = "Starting Bonus",
                description = "Start with {0} extra coins",
                icon = "🏦",
                category = SkillCategory.Economy,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(200, 100, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 10),
                prerequisiteIds = new string[] { "coin_boost" }
            });

            _skills.Add(new SkillNode
            {
                id = "wave_bonus",
                name = "Wave Bonus",
                description = "Earn {0}% more coins per wave",
                icon = "🌊",
                category = SkillCategory.Economy,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(250, 125, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 10),
                prerequisiteIds = new string[] { "gem_finder", "starting_coins" }
            });

            _skills.Add(new SkillNode
            {
                id = "discount",
                name = "Discount",
                description = "Reduce defender cost by {0}%",
                icon = "🏷️",
                category = SkillCategory.Economy,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 500, 1000, 2000, 4000, 8000 },
                usesGems = false,
                valuesPerLevel = new float[] { 5f, 10f, 15f, 20f, 25f },
                prerequisiteIds = new string[] { "wave_bonus" }
            });

            // === SPECIAL SKILLS ===
            _skills.Add(new SkillNode
            {
                id = "power_up_duration",
                name = "Extended Power",
                description = "Power-ups last {0}% longer",
                icon = "⏰",
                category = SkillCategory.Special,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(200, 100, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 10),
                prerequisiteIds = new string[] { }
            });

            _skills.Add(new SkillNode
            {
                id = "power_up_spawn",
                name = "Power Up Magnet",
                description = "{0}% more power-up spawns",
                icon = "🧲",
                category = SkillCategory.Special,
                maxLevel = 8,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(300, 150, 8),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 8),
                prerequisiteIds = new string[] { "power_up_duration" }
            });

            _skills.Add(new SkillNode
            {
                id = "combo_duration",
                name = "Combo Keeper",
                description = "Combo timer extended by {0}s",
                icon = "🔢",
                category = SkillCategory.Special,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 400, 800, 1600, 3200, 6400 },
                usesGems = false,
                valuesPerLevel = new float[] { 0.5f, 1f, 1.5f, 2f, 3f },
                prerequisiteIds = new string[] { "power_up_duration" }
            });

            _skills.Add(new SkillNode
            {
                id = "fever_boost",
                name = "Fever Power",
                description = "Fever mode gives {0}x multiplier",
                icon = "🔥",
                category = SkillCategory.Special,
                maxLevel = 3,
                currentLevel = 0,
                costsPerLevel = new int[] { 2000, 5000, 10000 },
                usesGems = false,
                valuesPerLevel = new float[] { 2.5f, 3f, 4f },
                prerequisiteIds = new string[] { "combo_duration", "power_up_spawn" }
            });

            // === ULTIMATE SKILLS ===
            _skills.Add(new SkillNode
            {
                id = "ultimate_charge",
                name = "Fast Charge",
                description = "Ultimate charges {0}% faster",
                icon = "⚡",
                category = SkillCategory.Ultimate,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(300, 150, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(10f, 10f, 10),
                prerequisiteIds = new string[] { }
            });

            _skills.Add(new SkillNode
            {
                id = "ultimate_power",
                name = "Ultimate Power",
                description = "Ultimate deals {0}% more damage",
                icon = "💥",
                category = SkillCategory.Ultimate,
                maxLevel = 10,
                currentLevel = 0,
                costsPerLevel = GenerateCosts(400, 200, 10),
                usesGems = false,
                valuesPerLevel = GenerateValues(20f, 20f, 10),
                prerequisiteIds = new string[] { "ultimate_charge" }
            });

            _skills.Add(new SkillNode
            {
                id = "ultimate_freeze",
                name = "Ultimate Freeze",
                description = "Ultimate also freezes enemies for {0}s",
                icon = "❄️",
                category = SkillCategory.Ultimate,
                maxLevel = 5,
                currentLevel = 0,
                costsPerLevel = new int[] { 1000, 2000, 4000, 8000, 15000 },
                usesGems = false,
                valuesPerLevel = new float[] { 1f, 2f, 3f, 4f, 5f },
                prerequisiteIds = new string[] { "ultimate_power" }
            });

            _skills.Add(new SkillNode
            {
                id = "double_ultimate",
                name = "Double Ultimate",
                description = "{0}% chance for free second ultimate",
                icon = "⚡⚡",
                category = SkillCategory.Ultimate,
                maxLevel = 3,
                currentLevel = 0,
                costsPerLevel = new int[] { 10, 25, 50 },
                usesGems = true, // Costs gems!
                valuesPerLevel = new float[] { 10f, 20f, 33f },
                prerequisiteIds = new string[] { "ultimate_freeze" }
            });
        }

        private int[] GenerateCosts(int baseCost, int increment, int levels)
        {
            int[] costs = new int[levels];
            for (int i = 0; i < levels; i++)
            {
                costs[i] = baseCost + increment * i;
            }
            return costs;
        }

        private float[] GenerateValues(float baseValue, float increment, int levels)
        {
            float[] values = new float[levels];
            for (int i = 0; i < levels; i++)
            {
                values[i] = baseValue + increment * i;
            }
            return values;
        }

        public List<SkillNode> GetAllSkills() => _skills;

        public List<SkillNode> GetSkillsByCategory(SkillCategory category)
        {
            return _skills.FindAll(s => s.category == category);
        }

        public SkillNode GetSkill(string id)
        {
            return _skills.Find(s => s.id == id);
        }

        public float GetSkillValue(string id)
        {
            SkillNode skill = GetSkill(id);
            if (skill == null || skill.currentLevel <= 0) return 0f;
            return skill.valuesPerLevel[skill.currentLevel - 1];
        }

        public float GetSkillTotalValue(string id)
        {
            SkillNode skill = GetSkill(id);
            if (skill == null || skill.currentLevel <= 0) return 0f;
            
            float total = 0f;
            for (int i = 0; i < skill.currentLevel; i++)
            {
                total += skill.valuesPerLevel[i];
            }
            return total;
        }

        public bool CanUpgrade(string id, int coins, int gems)
        {
            SkillNode skill = GetSkill(id);
            if (skill == null || skill.currentLevel >= skill.maxLevel) return false;
            
            // Check prerequisites
            foreach (string prereqId in skill.prerequisiteIds)
            {
                SkillNode prereq = GetSkill(prereqId);
                if (prereq == null || prereq.currentLevel <= 0) return false;
            }

            int cost = skill.costsPerLevel[skill.currentLevel];
            return skill.usesGems ? gems >= cost : coins >= cost;
        }

        public bool TryUpgrade(string id, ref int coins, ref int gems)
        {
            if (!CanUpgrade(id, coins, gems)) return false;

            SkillNode skill = GetSkill(id);
            int cost = skill.costsPerLevel[skill.currentLevel];

            if (skill.usesGems)
                gems -= cost;
            else
                coins -= cost;

            skill.currentLevel++;
            SaveProgress();
            OnSkillUpgraded?.Invoke(skill);

            return true;
        }

        public int GetUpgradeCost(string id)
        {
            SkillNode skill = GetSkill(id);
            if (skill == null || skill.currentLevel >= skill.maxLevel) return -1;
            return skill.costsPerLevel[skill.currentLevel];
        }

        public int GetTotalSkillPoints()
        {
            int total = 0;
            foreach (var skill in _skills)
            {
                total += skill.currentLevel;
            }
            return total;
        }

        public int GetTotalSpent()
        {
            int total = 0;
            foreach (var skill in _skills)
            {
                for (int i = 0; i < skill.currentLevel; i++)
                {
                    total += skill.costsPerLevel[i];
                }
            }
            return total;
        }

        public void ResetSkills(int refundPercent = 80)
        {
            foreach (var skill in _skills)
            {
                skill.currentLevel = 0;
            }
            SaveProgress();
            OnSkillsReset?.Invoke();
        }

        public string GetSkillDescription(SkillNode skill)
        {
            if (skill.currentLevel >= skill.maxLevel)
            {
                float value = skill.valuesPerLevel[skill.maxLevel - 1];
                return string.Format(skill.description, value) + " (MAX)";
            }
            else if (skill.currentLevel > 0)
            {
                float currentValue = skill.valuesPerLevel[skill.currentLevel - 1];
                float nextValue = skill.valuesPerLevel[skill.currentLevel];
                return string.Format(skill.description, currentValue) + $" → {nextValue}";
            }
            else
            {
                float nextValue = skill.valuesPerLevel[0];
                return string.Format(skill.description, nextValue);
            }
        }

        private void SaveProgress()
        {
            foreach (var skill in _skills)
            {
                PlayerPrefs.SetInt($"Skill_{skill.id}", skill.currentLevel);
            }
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            foreach (var skill in _skills)
            {
                skill.currentLevel = PlayerPrefs.GetInt($"Skill_{skill.id}", 0);
            }
        }

        // === QUICK ACCESS METHODS FOR GAMEPLAY ===
        public float GetDamageMultiplier() => 1f + GetSkillTotalValue("damage_boost") / 100f;
        public float GetAttackSpeedMultiplier() => 1f + GetSkillTotalValue("attack_speed") / 100f;
        public float GetCritChanceBonus() => GetSkillTotalValue("critical_chance") / 100f;
        public float GetCritDamageMultiplier() => 2f + GetSkillTotalValue("critical_damage") / 100f;
        public float GetRangeMultiplier() => 1f + GetSkillTotalValue("attack_range") / 100f;
        public int GetExtraLives() => (int)GetSkillValue("extra_lives");
        public float GetArmorReduction() => GetSkillTotalValue("armor") / 100f;
        public float GetShieldChance() => GetSkillValue("shield") / 100f;
        public float GetCoinMultiplier() => 1f + GetSkillTotalValue("coin_boost") / 100f;
        public float GetGemChance() => GetSkillTotalValue("gem_finder") / 100f;
        public int GetStartingCoinsBonus() => (int)GetSkillTotalValue("starting_coins");
        public float GetWaveBonusMultiplier() => 1f + GetSkillTotalValue("wave_bonus") / 100f;
        public float GetDefenderDiscount() => GetSkillValue("discount") / 100f;
        public float GetPowerUpDurationMultiplier() => 1f + GetSkillTotalValue("power_up_duration") / 100f;
        public float GetPowerUpSpawnBonus() => GetSkillTotalValue("power_up_spawn") / 100f;
        public float GetComboDurationBonus() => GetSkillValue("combo_duration");
        public float GetFeverMultiplier() => GetSkillValue("fever_boost");
        public float GetUltimateChargeMultiplier() => 1f + GetSkillTotalValue("ultimate_charge") / 100f;
        public float GetUltimateDamageMultiplier() => 1f + GetSkillTotalValue("ultimate_power") / 100f;
        public float GetUltimateFreezeDuration() => GetSkillValue("ultimate_freeze");
        public float GetDoubleUltimateChance() => GetSkillValue("double_ultimate") / 100f;
    }
}
