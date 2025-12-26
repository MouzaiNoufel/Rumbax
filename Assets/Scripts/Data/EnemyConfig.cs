using UnityEngine;

namespace Rumbax.Data
{
    /// <summary>
    /// Scriptable object defining enemy types and properties.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Rumbax/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Identification")]
        public string EnemyId;
        public string EnemyName;
        public EnemyType Type;
        
        [Header("Visual")]
        public Sprite Icon;
        public Color TintColor = Color.white;
        public RuntimeAnimatorController AnimatorController;
        public float Scale = 1f;
        
        [Header("Base Stats")]
        public float BaseHealth = 50f;
        public float BaseDamage = 10f;
        public float MoveSpeed = 2f;
        public float AttackSpeed = 1f;
        public float AttackRange = 1f;
        
        [Header("Scaling")]
        public float HealthPerWave = 10f;
        public float DamagePerWave = 2f;
        
        [Header("Rewards")]
        public int BaseCoinDrop = 5;
        public float CoinDropVariance = 0.2f;
        public float GemDropChance = 0.01f;
        
        [Header("Behavior")]
        public EnemyBehavior Behavior;
        public float SpecialAbilityCooldown = 5f;
        
        [Header("Effects")]
        public bool IsBoss;
        public float BossHealthMultiplier = 5f;
        public float BossRewardMultiplier = 10f;
        
        [Header("Audio")]
        public AudioClip SpawnSound;
        public AudioClip AttackSound;
        public AudioClip DeathSound;
        public AudioClip SpecialSound;

        /// <summary>
        /// Calculate health for a specific wave.
        /// </summary>
        public float GetHealthAtWave(int wave)
        {
            float health = BaseHealth + (HealthPerWave * (wave - 1));
            return IsBoss ? health * BossHealthMultiplier : health;
        }

        /// <summary>
        /// Calculate damage for a specific wave.
        /// </summary>
        public float GetDamageAtWave(int wave)
        {
            return BaseDamage + (DamagePerWave * (wave - 1));
        }

        /// <summary>
        /// Calculate coin drop with variance.
        /// </summary>
        public int GetCoinDrop()
        {
            float variance = Random.Range(-CoinDropVariance, CoinDropVariance);
            int coins = Mathf.RoundToInt(BaseCoinDrop * (1f + variance));
            return IsBoss ? Mathf.RoundToInt(coins * BossRewardMultiplier) : coins;
        }

        /// <summary>
        /// Check if gems should be dropped.
        /// </summary>
        public bool ShouldDropGem()
        {
            float chance = IsBoss ? GemDropChance * BossRewardMultiplier : GemDropChance;
            return Random.value < chance;
        }
    }

    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Flying,
        Healer,
        Splitter,
        Shielded,
        Boss
    }

    public enum EnemyBehavior
    {
        Standard,
        Aggressive,
        Defensive,
        Evasive,
        Support
    }
}
