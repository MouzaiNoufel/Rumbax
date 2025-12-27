using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

namespace Rumbax.Systems
{
    /// <summary>
    /// Hero System - Unlockable heroes with unique abilities, stats, and rarity tiers.
    /// Professional mobile game feature.
    /// </summary>
    [Serializable]
    public class HeroData
    {
        public string id;
        public string name;
        public string description;
        public HeroRarity rarity;
        public HeroClass heroClass;
        public int baseDamage;
        public float baseAttackSpeed;
        public float baseCritChance;
        public float baseRange;
        public string abilityName;
        public string abilityDescription;
        public float abilityCooldown;
        public int unlockCost;
        public int unlockGems;
        public int level;
        public int experience;
        public bool isUnlocked;
        public Color primaryColor;
        public Color secondaryColor;
    }

    public enum HeroRarity
    {
        Common,     // White - easy to get
        Rare,       // Blue - 20% chance
        Epic,       // Purple - 10% chance
        Legendary,  // Gold - 3% chance
        Mythic      // Rainbow - 0.5% chance
    }

    public enum HeroClass
    {
        Warrior,    // Balanced stats
        Archer,     // High range, fast attack
        Mage,       // High damage, slow attack
        Tank,       // Slow but powerful
        Assassin,   // High crit, low health
        Support     // Buffs nearby defenders
    }

    public class HeroSystem : MonoBehaviour
    {
        public static HeroSystem Instance { get; private set; }

        // All available heroes
        private List<HeroData> _allHeroes = new List<HeroData>();
        private HeroData _selectedHero;

        // Rarity colors
        public static readonly Dictionary<HeroRarity, Color> RarityColors = new Dictionary<HeroRarity, Color>
        {
            { HeroRarity.Common, new Color(0.7f, 0.7f, 0.7f) },
            { HeroRarity.Rare, new Color(0.3f, 0.6f, 1f) },
            { HeroRarity.Epic, new Color(0.7f, 0.3f, 1f) },
            { HeroRarity.Legendary, new Color(1f, 0.8f, 0.2f) },
            { HeroRarity.Mythic, new Color(1f, 0.4f, 0.6f) }
        };

        // Rarity names
        public static readonly Dictionary<HeroRarity, string> RarityNames = new Dictionary<HeroRarity, string>
        {
            { HeroRarity.Common, "★ Common" },
            { HeroRarity.Rare, "★★ Rare" },
            { HeroRarity.Epic, "★★★ Epic" },
            { HeroRarity.Legendary, "★★★★ Legendary" },
            { HeroRarity.Mythic, "★★★★★ Mythic" }
        };

        // Class icons
        public static readonly Dictionary<HeroClass, string> ClassIcons = new Dictionary<HeroClass, string>
        {
            { HeroClass.Warrior, "⚔️" },
            { HeroClass.Archer, "🏹" },
            { HeroClass.Mage, "🔮" },
            { HeroClass.Tank, "🛡️" },
            { HeroClass.Assassin, "🗡️" },
            { HeroClass.Support, "💚" }
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeHeroes();
                LoadHeroProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeHeroes()
        {
            // === COMMON HEROES ===
            _allHeroes.Add(new HeroData
            {
                id = "knight",
                name = "Knight",
                description = "A balanced warrior, good for beginners.",
                rarity = HeroRarity.Common,
                heroClass = HeroClass.Warrior,
                baseDamage = 10,
                baseAttackSpeed = 1.0f,
                baseCritChance = 0.05f,
                baseRange = 2.5f,
                abilityName = "Shield Bash",
                abilityDescription = "Stuns nearby enemies for 1s",
                abilityCooldown = 15f,
                unlockCost = 0,
                unlockGems = 0,
                isUnlocked = true, // Starter hero
                primaryColor = new Color(0.4f, 0.4f, 0.5f),
                secondaryColor = new Color(0.6f, 0.6f, 0.7f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "archer",
                name = "Archer",
                description = "Fast attacks with extended range.",
                rarity = HeroRarity.Common,
                heroClass = HeroClass.Archer,
                baseDamage = 7,
                baseAttackSpeed = 1.5f,
                baseCritChance = 0.08f,
                baseRange = 4.0f,
                abilityName = "Multi-Shot",
                abilityDescription = "Fires 5 arrows at once",
                abilityCooldown = 12f,
                unlockCost = 500,
                unlockGems = 0,
                isUnlocked = false,
                primaryColor = new Color(0.3f, 0.6f, 0.3f),
                secondaryColor = new Color(0.5f, 0.8f, 0.5f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "apprentice",
                name = "Apprentice",
                description = "A young mage learning the arts.",
                rarity = HeroRarity.Common,
                heroClass = HeroClass.Mage,
                baseDamage = 15,
                baseAttackSpeed = 0.6f,
                baseCritChance = 0.03f,
                baseRange = 3.0f,
                abilityName = "Fireball",
                abilityDescription = "Deals area damage",
                abilityCooldown = 10f,
                unlockCost = 500,
                unlockGems = 0,
                isUnlocked = false,
                primaryColor = new Color(0.2f, 0.3f, 0.6f),
                secondaryColor = new Color(0.4f, 0.5f, 0.8f)
            });

            // === RARE HEROES ===
            _allHeroes.Add(new HeroData
            {
                id = "paladin",
                name = "Paladin",
                description = "Holy warrior with healing abilities.",
                rarity = HeroRarity.Rare,
                heroClass = HeroClass.Tank,
                baseDamage = 12,
                baseAttackSpeed = 0.8f,
                baseCritChance = 0.05f,
                baseRange = 2.0f,
                abilityName = "Divine Shield",
                abilityDescription = "Becomes invulnerable for 3s",
                abilityCooldown = 20f,
                unlockCost = 2000,
                unlockGems = 50,
                isUnlocked = false,
                primaryColor = new Color(1f, 0.85f, 0.4f),
                secondaryColor = new Color(0.9f, 0.9f, 0.9f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "ranger",
                name = "Ranger",
                description = "Expert marksman with poison arrows.",
                rarity = HeroRarity.Rare,
                heroClass = HeroClass.Archer,
                baseDamage = 12,
                baseAttackSpeed = 1.3f,
                baseCritChance = 0.12f,
                baseRange = 4.5f,
                abilityName = "Poison Arrow",
                abilityDescription = "Deals damage over time",
                abilityCooldown = 8f,
                unlockCost = 2000,
                unlockGems = 50,
                isUnlocked = false,
                primaryColor = new Color(0.2f, 0.5f, 0.2f),
                secondaryColor = new Color(0.6f, 0.2f, 0.6f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "pyromancer",
                name = "Pyromancer",
                description = "Master of fire magic.",
                rarity = HeroRarity.Rare,
                heroClass = HeroClass.Mage,
                baseDamage = 20,
                baseAttackSpeed = 0.7f,
                baseCritChance = 0.08f,
                baseRange = 3.5f,
                abilityName = "Inferno",
                abilityDescription = "Burns all enemies for 5s",
                abilityCooldown = 18f,
                unlockCost = 2500,
                unlockGems = 75,
                isUnlocked = false,
                primaryColor = new Color(1f, 0.3f, 0.1f),
                secondaryColor = new Color(1f, 0.6f, 0.1f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "cleric",
                name = "Cleric",
                description = "Buffs allies and weakens enemies.",
                rarity = HeroRarity.Rare,
                heroClass = HeroClass.Support,
                baseDamage = 8,
                baseAttackSpeed = 1.0f,
                baseCritChance = 0.03f,
                baseRange = 3.0f,
                abilityName = "Blessing",
                abilityDescription = "Boosts nearby defenders' damage by 50%",
                abilityCooldown = 15f,
                unlockCost = 2000,
                unlockGems = 50,
                isUnlocked = false,
                primaryColor = new Color(0.9f, 0.9f, 0.5f),
                secondaryColor = new Color(0.4f, 0.8f, 0.4f)
            });

            // === EPIC HEROES ===
            _allHeroes.Add(new HeroData
            {
                id = "shadow_blade",
                name = "Shadow Blade",
                description = "Strikes from the shadows with lethal precision.",
                rarity = HeroRarity.Epic,
                heroClass = HeroClass.Assassin,
                baseDamage = 25,
                baseAttackSpeed = 1.8f,
                baseCritChance = 0.30f,
                baseRange = 2.5f,
                abilityName = "Shadow Step",
                abilityDescription = "Teleports and deals 300% damage",
                abilityCooldown = 12f,
                unlockCost = 5000,
                unlockGems = 200,
                isUnlocked = false,
                primaryColor = new Color(0.15f, 0.1f, 0.2f),
                secondaryColor = new Color(0.5f, 0.3f, 0.7f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "frost_mage",
                name = "Frost Mage",
                description = "Freezes enemies with icy magic.",
                rarity = HeroRarity.Epic,
                heroClass = HeroClass.Mage,
                baseDamage = 18,
                baseAttackSpeed = 0.8f,
                baseCritChance = 0.10f,
                baseRange = 4.0f,
                abilityName = "Blizzard",
                abilityDescription = "Freezes all enemies for 3s",
                abilityCooldown = 25f,
                unlockCost = 6000,
                unlockGems = 250,
                isUnlocked = false,
                primaryColor = new Color(0.3f, 0.7f, 1f),
                secondaryColor = new Color(0.8f, 0.95f, 1f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "berserker",
                name = "Berserker",
                description = "Gets stronger as health drops.",
                rarity = HeroRarity.Epic,
                heroClass = HeroClass.Warrior,
                baseDamage = 30,
                baseAttackSpeed = 1.2f,
                baseCritChance = 0.15f,
                baseRange = 2.0f,
                abilityName = "Blood Rage",
                abilityDescription = "Double damage for 8s",
                abilityCooldown = 20f,
                unlockCost = 5500,
                unlockGems = 200,
                isUnlocked = false,
                primaryColor = new Color(0.7f, 0.15f, 0.1f),
                secondaryColor = new Color(0.9f, 0.3f, 0.2f)
            });

            // === LEGENDARY HEROES ===
            _allHeroes.Add(new HeroData
            {
                id = "dragon_knight",
                name = "Dragon Knight",
                description = "Wields the power of ancient dragons.",
                rarity = HeroRarity.Legendary,
                heroClass = HeroClass.Warrior,
                baseDamage = 40,
                baseAttackSpeed = 1.0f,
                baseCritChance = 0.20f,
                baseRange = 3.0f,
                abilityName = "Dragon Breath",
                abilityDescription = "Devastating fire wave damage",
                abilityCooldown = 25f,
                unlockCost = 15000,
                unlockGems = 500,
                isUnlocked = false,
                primaryColor = new Color(0.8f, 0.2f, 0.1f),
                secondaryColor = new Color(1f, 0.7f, 0.2f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "archmage",
                name = "Archmage",
                description = "Supreme master of all magical arts.",
                rarity = HeroRarity.Legendary,
                heroClass = HeroClass.Mage,
                baseDamage = 50,
                baseAttackSpeed = 0.6f,
                baseCritChance = 0.15f,
                baseRange = 5.0f,
                abilityName = "Meteor Storm",
                abilityDescription = "Rains meteors on all enemies",
                abilityCooldown = 30f,
                unlockCost = 18000,
                unlockGems = 600,
                isUnlocked = false,
                primaryColor = new Color(0.4f, 0.2f, 0.8f),
                secondaryColor = new Color(0.8f, 0.4f, 1f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "void_hunter",
                name = "Void Hunter",
                description = "Hunts from between dimensions.",
                rarity = HeroRarity.Legendary,
                heroClass = HeroClass.Assassin,
                baseDamage = 35,
                baseAttackSpeed = 2.0f,
                baseCritChance = 0.40f,
                baseRange = 3.5f,
                abilityName = "Void Rift",
                abilityDescription = "Creates a rift that damages enemies",
                abilityCooldown = 18f,
                unlockCost = 20000,
                unlockGems = 700,
                isUnlocked = false,
                primaryColor = new Color(0.1f, 0.05f, 0.15f),
                secondaryColor = new Color(0.6f, 0.1f, 0.8f)
            });

            // === MYTHIC HEROES ===
            _allHeroes.Add(new HeroData
            {
                id = "celestial_guardian",
                name = "Celestial Guardian",
                description = "A divine being from the heavens.",
                rarity = HeroRarity.Mythic,
                heroClass = HeroClass.Support,
                baseDamage = 45,
                baseAttackSpeed = 1.2f,
                baseCritChance = 0.25f,
                baseRange = 4.0f,
                abilityName = "Divine Judgment",
                abilityDescription = "Kills all enemies below 30% HP",
                abilityCooldown = 35f,
                unlockCost = 50000,
                unlockGems = 2000,
                isUnlocked = false,
                primaryColor = new Color(1f, 0.95f, 0.8f),
                secondaryColor = new Color(1f, 0.8f, 0.4f)
            });

            _allHeroes.Add(new HeroData
            {
                id = "omega_destroyer",
                name = "Omega Destroyer",
                description = "The ultimate weapon of mass destruction.",
                rarity = HeroRarity.Mythic,
                heroClass = HeroClass.Tank,
                baseDamage = 60,
                baseAttackSpeed = 0.5f,
                baseCritChance = 0.20f,
                baseRange = 3.0f,
                abilityName = "Apocalypse",
                abilityDescription = "Deals 50% HP to all enemies",
                abilityCooldown = 40f,
                unlockCost = 75000,
                unlockGems = 3000,
                isUnlocked = false,
                primaryColor = new Color(0.1f, 0.1f, 0.1f),
                secondaryColor = new Color(1f, 0.2f, 0.2f)
            });

            // Set first hero as selected
            _selectedHero = _allHeroes[0];
        }

        public List<HeroData> GetAllHeroes() => _allHeroes;
        public List<HeroData> GetUnlockedHeroes() => _allHeroes.FindAll(h => h.isUnlocked);
        public HeroData GetSelectedHero() => _selectedHero;
        
        public void SelectHero(string heroId)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            if (hero != null && hero.isUnlocked)
            {
                _selectedHero = hero;
                SaveHeroProgress();
            }
        }

        public bool TryUnlockHero(string heroId, int coins, int gems, out int newCoins, out int newGems)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            newCoins = coins;
            newGems = gems;

            if (hero == null || hero.isUnlocked) return false;
            if (coins < hero.unlockCost || gems < hero.unlockGems) return false;

            newCoins = coins - hero.unlockCost;
            newGems = gems - hero.unlockGems;
            hero.isUnlocked = true;
            
            SaveHeroProgress();
            return true;
        }

        public bool TryUpgradeHero(string heroId, int coins, out int newCoins)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            newCoins = coins;

            if (hero == null || !hero.isUnlocked) return false;
            if (hero.level >= GetMaxLevel(hero.rarity)) return false;

            int upgradeCost = GetUpgradeCost(hero);
            if (coins < upgradeCost) return false;

            newCoins = coins - upgradeCost;
            hero.level++;
            
            // Increase stats on level up
            hero.baseDamage += (int)(hero.baseDamage * 0.1f);
            hero.baseAttackSpeed *= 1.02f;
            hero.baseCritChance = Mathf.Min(0.8f, hero.baseCritChance + 0.01f);
            
            SaveHeroProgress();
            return true;
        }

        public int GetMaxLevel(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => 20,
                HeroRarity.Rare => 30,
                HeroRarity.Epic => 40,
                HeroRarity.Legendary => 50,
                HeroRarity.Mythic => 100,
                _ => 20
            };
        }

        public int GetUpgradeCost(HeroData hero)
        {
            int baseCost = (int)hero.rarity * 100 + 50;
            return baseCost * (hero.level + 1);
        }

        public int GetExperienceToNextLevel(HeroData hero)
        {
            return 100 * (hero.level + 1) * ((int)hero.rarity + 1);
        }

        public void AddExperience(string heroId, int exp)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            if (hero == null || !hero.isUnlocked) return;

            hero.experience += exp;
            int expNeeded = GetExperienceToNextLevel(hero);

            while (hero.experience >= expNeeded && hero.level < GetMaxLevel(hero.rarity))
            {
                hero.experience -= expNeeded;
                hero.level++;
                hero.baseDamage += (int)(hero.baseDamage * 0.05f);
                expNeeded = GetExperienceToNextLevel(hero);
            }

            SaveHeroProgress();
        }

        public void AddFragments(string heroId, int amount)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            if (hero == null) return;

            int currentFragments = PlayerPrefs.GetInt($"Hero_{heroId}_Fragments", 0);
            currentFragments += amount;
            PlayerPrefs.SetInt($"Hero_{heroId}_Fragments", currentFragments);

            // Auto-unlock if enough fragments (50 for common, 100 for rare, etc.)
            int requiredFragments = ((int)hero.rarity + 1) * 50;
            if (!hero.isUnlocked && currentFragments >= requiredFragments)
            {
                UnlockHero(heroId);
            }

            PlayerPrefs.Save();
        }

        public void UnlockHero(string heroId)
        {
            HeroData hero = _allHeroes.Find(h => h.id == heroId);
            if (hero == null || hero.isUnlocked) return;

            hero.isUnlocked = true;
            SaveHeroProgress();
            Debug.Log($"[HeroSystem] Hero unlocked: {hero.displayName}");
        }

        private void SaveHeroProgress()
        {
            foreach (var hero in _allHeroes)
            {
                PlayerPrefs.SetInt($"Hero_{hero.id}_Unlocked", hero.isUnlocked ? 1 : 0);
                PlayerPrefs.SetInt($"Hero_{hero.id}_Level", hero.level);
                PlayerPrefs.SetInt($"Hero_{hero.id}_Exp", hero.experience);
                PlayerPrefs.SetInt($"Hero_{hero.id}_Damage", hero.baseDamage);
            }
            
            if (_selectedHero != null)
                PlayerPrefs.SetString("SelectedHero", _selectedHero.id);
            
            PlayerPrefs.Save();
        }

        private void LoadHeroProgress()
        {
            foreach (var hero in _allHeroes)
            {
                if (PlayerPrefs.HasKey($"Hero_{hero.id}_Unlocked"))
                {
                    hero.isUnlocked = PlayerPrefs.GetInt($"Hero_{hero.id}_Unlocked") == 1;
                    hero.level = PlayerPrefs.GetInt($"Hero_{hero.id}_Level", 1);
                    hero.experience = PlayerPrefs.GetInt($"Hero_{hero.id}_Exp", 0);
                    hero.baseDamage = PlayerPrefs.GetInt($"Hero_{hero.id}_Damage", hero.baseDamage);
                }
            }

            string selectedId = PlayerPrefs.GetString("SelectedHero", "knight");
            _selectedHero = _allHeroes.Find(h => h.id == selectedId) ?? _allHeroes[0];
        }

        public float GetTotalPower(HeroData hero)
        {
            return (hero.baseDamage * hero.baseAttackSpeed) * 
                   (1 + hero.baseCritChance) * 
                   (hero.level + 1) * 
                   ((int)hero.rarity + 1);
        }
    }
}
