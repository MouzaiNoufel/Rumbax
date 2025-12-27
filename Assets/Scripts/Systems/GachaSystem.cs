using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Rumbax.Systems
{
    /// <summary>
    /// Lucky Wheel and Gacha System - Spin for rewards and summon heroes.
    /// Professional mobile game feature with pity system.
    /// </summary>
    [Serializable]
    public class WheelSlot
    {
        public string name;
        public RewardType type;
        public int amount;
        public string heroId;
        public float weight;
        public Color color;
        public bool isJackpot;
    }

    [Serializable]
    public class GachaBanner
    {
        public string id;
        public string name;
        public string description;
        public string featuredHeroId;
        public int costPerPull;
        public bool usesGems;
        public float[] rarityRates; // Common, Rare, Epic, Legendary, Mythic
        public int pityCounter;
        public int hardPity; // Guaranteed at this pull
        public int softPity; // Increased rates start here
        public DateTime endDate;
        public bool isLimited;
    }

    [Serializable]
    public class GachaResult
    {
        public string heroId;
        public HeroRarity rarity;
        public bool isNew;
        public bool isPity;
        public int fragments;
    }

    public class GachaSystem : MonoBehaviour
    {
        public static GachaSystem Instance { get; private set; }

        private List<WheelSlot> _wheelSlots = new List<WheelSlot>();
        private List<GachaBanner> _banners = new List<GachaBanner>();
        private float _totalWheelWeight;

        // Events
        public event Action<WheelSlot> OnWheelSpun;
        public event Action<GachaResult> OnHeroSummoned;
        public event Action<GachaResult[]> OnMultiSummon;

        // Pity tracking
        private Dictionary<string, int> _bannerPity = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeWheel();
                InitializeBanners();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeWheel()
        {
            _wheelSlots.Clear();

            // Coins - Common
            _wheelSlots.Add(new WheelSlot
            {
                name = "100 Coins",
                type = RewardType.Coins,
                amount = 100,
                weight = 25f,
                color = new Color(0.6f, 0.5f, 0.3f),
                isJackpot = false
            });

            _wheelSlots.Add(new WheelSlot
            {
                name = "250 Coins",
                type = RewardType.Coins,
                amount = 250,
                weight = 20f,
                color = new Color(0.7f, 0.6f, 0.3f),
                isJackpot = false
            });

            _wheelSlots.Add(new WheelSlot
            {
                name = "500 Coins",
                type = RewardType.Coins,
                amount = 500,
                weight = 12f,
                color = new Color(0.8f, 0.7f, 0.2f),
                isJackpot = false
            });

            _wheelSlots.Add(new WheelSlot
            {
                name = "1000 Coins",
                type = RewardType.Coins,
                amount = 1000,
                weight = 5f,
                color = new Color(1f, 0.85f, 0.2f),
                isJackpot = false
            });

            // Gems
            _wheelSlots.Add(new WheelSlot
            {
                name = "5 Gems",
                type = RewardType.Gems,
                amount = 5,
                weight = 15f,
                color = new Color(0.3f, 0.6f, 0.9f),
                isJackpot = false
            });

            _wheelSlots.Add(new WheelSlot
            {
                name = "15 Gems",
                type = RewardType.Gems,
                amount = 15,
                weight = 8f,
                color = new Color(0.4f, 0.7f, 1f),
                isJackpot = false
            });

            _wheelSlots.Add(new WheelSlot
            {
                name = "50 Gems",
                type = RewardType.Gems,
                amount = 50,
                weight = 3f,
                color = new Color(0.5f, 0.8f, 1f),
                isJackpot = false
            });

            // Power-ups
            _wheelSlots.Add(new WheelSlot
            {
                name = "Power-up x3",
                type = RewardType.PowerUp,
                amount = 3,
                weight = 10f,
                color = new Color(1f, 0.5f, 0.2f),
                isJackpot = false
            });

            // Hero Fragments
            _wheelSlots.Add(new WheelSlot
            {
                name = "Hero Fragment",
                type = RewardType.HeroFragment,
                amount = 1,
                heroId = "random_rare",
                weight = 5f,
                color = new Color(0.6f, 0.3f, 0.9f),
                isJackpot = false
            });

            // Lucky Spins
            _wheelSlots.Add(new WheelSlot
            {
                name = "Extra Spin!",
                type = RewardType.LuckySpins,
                amount = 1,
                weight = 5f,
                color = new Color(0.2f, 0.8f, 0.4f),
                isJackpot = false
            });

            // JACKPOT!
            _wheelSlots.Add(new WheelSlot
            {
                name = "💎 JACKPOT! 💎",
                type = RewardType.Gems,
                amount = 200,
                weight = 0.5f,
                color = new Color(1f, 0.4f, 0.6f),
                isJackpot = true
            });

            // Calculate total weight
            _totalWheelWeight = 0f;
            foreach (var slot in _wheelSlots)
            {
                _totalWheelWeight += slot.weight;
            }
        }

        private void InitializeBanners()
        {
            _banners.Clear();

            // Standard Banner - Always available
            _banners.Add(new GachaBanner
            {
                id = "standard",
                name = "Standard Summon",
                description = "The classic summon pool with all heroes.",
                featuredHeroId = "",
                costPerPull = 100,
                usesGems = true,
                rarityRates = new float[] { 60f, 25f, 10f, 4f, 1f }, // C, R, E, L, M
                pityCounter = 0,
                hardPity = 90,
                softPity = 75,
                isLimited = false
            });

            // Epic Guaranteed Banner
            _banners.Add(new GachaBanner
            {
                id = "epic_up",
                name = "Epic Rate Up",
                description = "Increased Epic hero rates!",
                featuredHeroId = "shadow_blade",
                costPerPull = 150,
                usesGems = true,
                rarityRates = new float[] { 45f, 25f, 20f, 8f, 2f },
                pityCounter = 0,
                hardPity = 50,
                softPity = 40,
                isLimited = false
            });

            // Legendary Banner
            _banners.Add(new GachaBanner
            {
                id = "legendary_up",
                name = "Legendary Focus",
                description = "Your best chance at Legendary heroes!",
                featuredHeroId = "dragon_knight",
                costPerPull = 200,
                usesGems = true,
                rarityRates = new float[] { 35f, 25f, 20f, 15f, 5f },
                pityCounter = 0,
                hardPity = 80,
                softPity = 60,
                isLimited = true,
                endDate = DateTime.Today.AddDays(7)
            });

            // Mythic Banner (rare)
            _banners.Add(new GachaBanner
            {
                id = "mythic_banner",
                name = "✨ Mythic Summon ✨",
                description = "The ultimate summon! Guaranteed Mythic at 100 pulls!",
                featuredHeroId = "celestial_guardian",
                costPerPull = 300,
                usesGems = true,
                rarityRates = new float[] { 30f, 25f, 20f, 15f, 10f },
                pityCounter = 0,
                hardPity = 100,
                softPity = 80,
                isLimited = true,
                endDate = DateTime.Today.AddDays(3)
            });
        }

        // === LUCKY WHEEL ===

        public List<WheelSlot> GetWheelSlots() => _wheelSlots;

        public bool CanSpin()
        {
            return DailyRewardsSystem.Instance != null && DailyRewardsSystem.Instance.GetLuckySpins() > 0;
        }

        public WheelSlot SpinWheel()
        {
            if (!CanSpin()) return null;

            DailyRewardsSystem.Instance.UseLuckySpin();

            // Weighted random selection
            float random = UnityEngine.Random.Range(0f, _totalWheelWeight);
            float cumulative = 0f;

            foreach (var slot in _wheelSlots)
            {
                cumulative += slot.weight;
                if (random <= cumulative)
                {
                    OnWheelSpun?.Invoke(slot);
                    
                    // Track jackpot for achievement
                    if (slot.isJackpot && AchievementSystem.Instance != null)
                    {
                        AchievementSystem.Instance.UpdateProgress("lucky_spin", 1);
                    }
                    
                    return slot;
                }
            }

            // Fallback
            return _wheelSlots[0];
        }

        public void ApplyWheelReward(WheelSlot slot, ref int coins, ref int gems)
        {
            switch (slot.type)
            {
                case RewardType.Coins:
                    coins += slot.amount;
                    break;
                case RewardType.Gems:
                    gems += slot.amount;
                    break;
                case RewardType.PowerUp:
                    int current = PlayerPrefs.GetInt("StoredPowerUps", 0);
                    PlayerPrefs.SetInt("StoredPowerUps", current + slot.amount);
                    break;
                case RewardType.HeroFragment:
                    // Random rare hero fragment
                    string[] rareHeroes = { "paladin", "ranger", "pyromancer", "cleric" };
                    string heroId = rareHeroes[UnityEngine.Random.Range(0, rareHeroes.Length)];
                    int fragments = PlayerPrefs.GetInt($"Fragments_{heroId}", 0);
                    PlayerPrefs.SetInt($"Fragments_{heroId}", fragments + slot.amount);
                    break;
                case RewardType.LuckySpins:
                    // Already handled - gives free spin
                    int spins = PlayerPrefs.GetInt("LuckySpins", 0);
                    PlayerPrefs.SetInt("LuckySpins", spins + slot.amount);
                    break;
            }
            PlayerPrefs.Save();
        }

        public float GetSlotAngle(int index)
        {
            float anglePerSlot = 360f / _wheelSlots.Count;
            return index * anglePerSlot;
        }

        // === GACHA SYSTEM ===

        public List<GachaBanner> GetBanners() => _banners;

        public List<GachaBanner> GetActiveBanners()
        {
            return _banners.FindAll(b => !b.isLimited || b.endDate > DateTime.Now);
        }

        public GachaBanner GetBanner(string id)
        {
            return _banners.Find(b => b.id == id);
        }

        public int GetPityCount(string bannerId)
        {
            return _bannerPity.ContainsKey(bannerId) ? _bannerPity[bannerId] : 0;
        }

        public bool CanPull(string bannerId, int gems)
        {
            GachaBanner banner = GetBanner(bannerId);
            return banner != null && gems >= banner.costPerPull;
        }

        public GachaResult Pull(string bannerId, ref int gems)
        {
            GachaBanner banner = GetBanner(bannerId);
            if (banner == null || gems < banner.costPerPull) return null;

            gems -= banner.costPerPull;

            // Increment pity
            if (!_bannerPity.ContainsKey(bannerId))
                _bannerPity[bannerId] = 0;
            _bannerPity[bannerId]++;

            int currentPity = _bannerPity[bannerId];
            bool isPity = currentPity >= banner.hardPity;

            // Determine rarity
            HeroRarity rarity = DetermineRarity(banner, currentPity, isPity);

            // Get hero of that rarity
            GachaResult result = GetHeroOfRarity(banner, rarity);
            result.isPity = isPity;

            // Reset pity if high rarity
            if ((int)rarity >= (int)HeroRarity.Legendary)
            {
                _bannerPity[bannerId] = 0;
            }

            SaveProgress();
            OnHeroSummoned?.Invoke(result);
            return result;
        }

        public GachaResult[] MultiPull(string bannerId, int count, ref int gems)
        {
            GachaBanner banner = GetBanner(bannerId);
            int totalCost = banner.costPerPull * count;
            if (gems < totalCost) return null;

            GachaResult[] results = new GachaResult[count];
            for (int i = 0; i < count; i++)
            {
                results[i] = Pull(bannerId, ref gems);
            }

            OnMultiSummon?.Invoke(results);
            return results;
        }

        private HeroRarity DetermineRarity(GachaBanner banner, int pity, bool isPity)
        {
            if (isPity)
            {
                // Guaranteed high rarity at hard pity
                return banner.id.Contains("mythic") ? HeroRarity.Mythic : HeroRarity.Legendary;
            }

            float[] rates = (float[])banner.rarityRates.Clone();

            // Soft pity - increase legendary/mythic rates
            if (pity >= banner.softPity)
            {
                float pityBonus = (pity - banner.softPity) * 5f;
                rates[3] += pityBonus; // Legendary
                rates[4] += pityBonus * 0.5f; // Mythic
                // Reduce common rates
                rates[0] = Mathf.Max(10f, rates[0] - pityBonus * 1.5f);
            }

            float total = 0f;
            foreach (float rate in rates) total += rate;

            float random = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < rates.Length; i++)
            {
                cumulative += rates[i];
                if (random <= cumulative)
                {
                    return (HeroRarity)i;
                }
            }

            return HeroRarity.Common;
        }

        private GachaResult GetHeroOfRarity(GachaBanner banner, HeroRarity rarity)
        {
            // Get heroes of this rarity from HeroSystem
            List<string> heroPool = GetHeroPoolByRarity(rarity);

            // Featured hero has higher chance on banner
            if (!string.IsNullOrEmpty(banner.featuredHeroId) && UnityEngine.Random.value < 0.5f)
            {
                HeroData featuredHero = GetHeroData(banner.featuredHeroId);
                if (featuredHero != null && featuredHero.rarity == rarity)
                {
                    return CreateResult(banner.featuredHeroId, rarity);
                }
            }

            // Random from pool
            if (heroPool.Count > 0)
            {
                string heroId = heroPool[UnityEngine.Random.Range(0, heroPool.Count)];
                return CreateResult(heroId, rarity);
            }

            // Fallback - give fragments instead
            return new GachaResult
            {
                heroId = "",
                rarity = rarity,
                isNew = false,
                fragments = GetFragmentsByRarity(rarity)
            };
        }

        private List<string> GetHeroPoolByRarity(HeroRarity rarity)
        {
            List<string> pool = new List<string>();
            
            // Hero IDs by rarity (from HeroSystem)
            switch (rarity)
            {
                case HeroRarity.Common:
                    pool.AddRange(new[] { "knight", "archer", "apprentice" });
                    break;
                case HeroRarity.Rare:
                    pool.AddRange(new[] { "paladin", "ranger", "pyromancer", "cleric" });
                    break;
                case HeroRarity.Epic:
                    pool.AddRange(new[] { "shadow_blade", "frost_mage", "berserker" });
                    break;
                case HeroRarity.Legendary:
                    pool.AddRange(new[] { "dragon_knight", "archmage", "void_hunter" });
                    break;
                case HeroRarity.Mythic:
                    pool.AddRange(new[] { "celestial_guardian", "omega_destroyer" });
                    break;
            }

            return pool;
        }

        private GachaResult CreateResult(string heroId, HeroRarity rarity)
        {
            bool isUnlocked = PlayerPrefs.GetInt($"Hero_{heroId}_Unlocked", 0) == 1;
            
            GachaResult result = new GachaResult
            {
                heroId = heroId,
                rarity = rarity,
                isNew = !isUnlocked,
                fragments = isUnlocked ? GetFragmentsByRarity(rarity) : 0
            };

            if (!isUnlocked)
            {
                // Unlock hero
                PlayerPrefs.SetInt($"Hero_{heroId}_Unlocked", 1);
                PlayerPrefs.SetInt($"Hero_{heroId}_Level", 1);
                PlayerPrefs.Save();

                // Achievement
                if (AchievementSystem.Instance != null)
                {
                    AchievementSystem.Instance.OnHeroUnlocked();
                    
                    switch (rarity)
                    {
                        case HeroRarity.Rare:
                            AchievementSystem.Instance.UpdateProgress("rare_hero", 1);
                            break;
                        case HeroRarity.Epic:
                            AchievementSystem.Instance.UpdateProgress("epic_hero", 1);
                            break;
                        case HeroRarity.Legendary:
                            AchievementSystem.Instance.UpdateProgress("legendary_hero", 1);
                            break;
                        case HeroRarity.Mythic:
                            AchievementSystem.Instance.UpdateProgress("mythic_hero", 1);
                            break;
                    }
                }
            }
            else
            {
                // Convert to fragments
                int fragments = GetFragmentsByRarity(rarity);
                int current = PlayerPrefs.GetInt($"Fragments_{heroId}", 0);
                PlayerPrefs.SetInt($"Fragments_{heroId}", current + fragments);
                PlayerPrefs.Save();
            }

            return result;
        }

        private int GetFragmentsByRarity(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => 5,
                HeroRarity.Rare => 10,
                HeroRarity.Epic => 20,
                HeroRarity.Legendary => 50,
                HeroRarity.Mythic => 100,
                _ => 5
            };
        }

        private HeroData GetHeroData(string heroId)
        {
            if (HeroSystem.Instance != null)
            {
                return HeroSystem.Instance.GetAllHeroes().Find(h => h.id == heroId);
            }
            return null;
        }

        public string GetRarityAnimation(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => "common_reveal",
                HeroRarity.Rare => "rare_reveal",
                HeroRarity.Epic => "epic_reveal",
                HeroRarity.Legendary => "legendary_reveal",
                HeroRarity.Mythic => "mythic_reveal",
                _ => "common_reveal"
            };
        }

        public Color GetRarityGlowColor(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => new Color(0.8f, 0.8f, 0.8f),
                HeroRarity.Rare => new Color(0.3f, 0.6f, 1f),
                HeroRarity.Epic => new Color(0.7f, 0.3f, 1f),
                HeroRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                HeroRarity.Mythic => Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1f)),
                _ => Color.white
            };
        }

        private void SaveProgress()
        {
            foreach (var kvp in _bannerPity)
            {
                PlayerPrefs.SetInt($"BannerPity_{kvp.Key}", kvp.Value);
            }
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _bannerPity.Clear();
            foreach (var banner in _banners)
            {
                int pity = PlayerPrefs.GetInt($"BannerPity_{banner.id}", 0);
                _bannerPity[banner.id] = pity;
            }
        }
    }
}
