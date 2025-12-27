using UnityEngine;

namespace Rumbax.Data
{
    /// <summary>
    /// Scriptable object defining defender types and properties.
    /// </summary>
    [CreateAssetMenu(fileName = "DefenderConfig", menuName = "Rumbax/Defender Config")]
    public class DefenderConfig : ScriptableObject
    {
        [Header("Identification")]
        public string DefenderId;
        public string DefenderName;
        public DefenderType Type;
        
        [Header("Visual")]
        public Sprite Icon;
        public Sprite[] LevelSprites;
        public Color TierColor = Color.white;
        public RuntimeAnimatorController AnimatorController;
        
        [Header("Base Stats")]
        public float BaseDamage = 10f;
        public float BaseAttackSpeed = 1f;
        public float BaseRange = 3f;
        public float BaseHealth = 100f;
        
        [Header("Scaling")]
        public float DamagePerLevel = 5f;
        public float AttackSpeedPerLevel = 0.05f;
        public float RangePerLevel = 0.1f;
        public float HealthPerLevel = 20f;
        
        [Header("Special Ability")]
        public bool HasSpecialAbility;
        public SpecialAbilityType AbilityType;
        public float AbilityCooldown = 10f;
        public float AbilityDuration = 3f;
        public float AbilityValue = 1.5f;
        
        [Header("Unlock")]
        public bool UnlockedByDefault;
        public int UnlockLevel;
        public int UnlockCost;
        public Rumbax.Core.Events.CurrencyType UnlockCurrencyType;
        
        [Header("Audio")]
        public AudioClip AttackSound;
        public AudioClip AbilitySound;
        public AudioClip DeathSound;

        /// <summary>
        /// Calculate damage at a specific level.
        /// </summary>
        public float GetDamageAtLevel(int level)
        {
            return BaseDamage + (DamagePerLevel * (level - 1));
        }

        /// <summary>
        /// Calculate attack speed at a specific level.
        /// </summary>
        public float GetAttackSpeedAtLevel(int level)
        {
            return BaseAttackSpeed + (AttackSpeedPerLevel * (level - 1));
        }

        /// <summary>
        /// Calculate range at a specific level.
        /// </summary>
        public float GetRangeAtLevel(int level)
        {
            return BaseRange + (RangePerLevel * (level - 1));
        }

        /// <summary>
        /// Calculate health at a specific level.
        /// </summary>
        public float GetHealthAtLevel(int level)
        {
            return BaseHealth + (HealthPerLevel * (level - 1));
        }

        /// <summary>
        /// Get sprite for specific level.
        /// </summary>
        public Sprite GetSpriteForLevel(int level)
        {
            if (LevelSprites == null || LevelSprites.Length == 0)
                return Icon;
            
            int index = Mathf.Clamp(level - 1, 0, LevelSprites.Length - 1);
            return LevelSprites[index] ?? Icon;
        }
    }

    public enum DefenderType
    {
        Warrior,
        Archer,
        Mage,
        Tank,
        Support,
        Assassin
    }

    public enum SpecialAbilityType
    {
        None,
        AreaDamage,
        Stun,
        Slow,
        Heal,
        Buff,
        DoubleDamage,
        Shield,
        Poison,
        Chain
    }

    // CurrencyType is defined in Rumbax.Core.Events namespace
}
