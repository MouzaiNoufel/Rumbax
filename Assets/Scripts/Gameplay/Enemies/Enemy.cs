using System.Collections;
using UnityEngine;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;

namespace Rumbax.Gameplay.Enemies
{
    /// <summary>
    /// Base enemy class that moves towards the goal and attacks.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfig config;
        
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform healthBarPivot;
        [SerializeField] private SpriteRenderer healthBarFill;
        
        public string EnemyId => config?.EnemyId ?? "";
        public bool IsAlive => _currentHealth > 0;
        public bool IsBoss => config?.IsBoss ?? false;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        
        private float _maxHealth;
        private float _currentHealth;
        private float _damage;
        private float _moveSpeed;
        private float _baseMoveSpeed;
        private int _waveNumber;
        private Transform _target;
        private int _currentWaypointIndex;
        private Transform[] _waypoints;
        private bool _isMoving = true;
        
        // Status effects
        private float _slowDuration;
        private float _slowAmount;
        private bool _isStunned;
        private float _stunDuration;
        private bool _isPoisoned;
        private float _poisonDamage;
        private float _poisonDuration;

        private static readonly int WalkBool = Animator.StringToHash("IsWalking");
        private static readonly int DieTrigger = Animator.StringToHash("Die");
        private static readonly int HitTrigger = Animator.StringToHash("Hit");

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            if (!IsAlive) return;
            
            UpdateStatusEffects();
            
            if (_isMoving && !_isStunned)
            {
                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// Initialize enemy with configuration.
        /// </summary>
        public void Initialize(EnemyConfig enemyConfig, int wave, Transform[] waypoints)
        {
            config = enemyConfig;
            _waveNumber = wave;
            _waypoints = waypoints;
            _currentWaypointIndex = 0;
            
            if (_waypoints != null && _waypoints.Length > 0)
            {
                _target = _waypoints[0];
            }
            
            SetupStats();
            UpdateVisual();
            
            _isMoving = true;
            
            if (animator != null)
            {
                animator.SetBool(WalkBool, true);
            }
        }

        /// <summary>
        /// Setup stats based on wave number.
        /// </summary>
        private void SetupStats()
        {
            if (config == null) return;
            
            _maxHealth = config.GetHealthAtWave(_waveNumber);
            _currentHealth = _maxHealth;
            _damage = config.GetDamageAtWave(_waveNumber);
            _baseMoveSpeed = config.MoveSpeed;
            _moveSpeed = _baseMoveSpeed;
            
            UpdateHealthBar();
        }

        /// <summary>
        /// Update visual appearance.
        /// </summary>
        private void UpdateVisual()
        {
            if (config == null || spriteRenderer == null) return;
            
            spriteRenderer.sprite = config.Icon;
            spriteRenderer.color = config.TintColor;
            transform.localScale = Vector3.one * config.Scale;
        }

        /// <summary>
        /// Move towards current target waypoint.
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (_target == null) return;
            
            Vector3 direction = (_target.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
            
            // Flip sprite based on movement direction
            if (direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
            
            // Check if reached waypoint
            float distance = Vector3.Distance(transform.position, _target.position);
            if (distance < 0.1f)
            {
                ReachWaypoint();
            }
        }

        /// <summary>
        /// Handle reaching a waypoint.
        /// </summary>
        private void ReachWaypoint()
        {
            _currentWaypointIndex++;
            
            if (_waypoints != null && _currentWaypointIndex < _waypoints.Length)
            {
                _target = _waypoints[_currentWaypointIndex];
            }
            else
            {
                // Reached end - deal damage to player
                ReachEnd();
            }
        }

        /// <summary>
        /// Handle reaching the end (player base).
        /// </summary>
        private void ReachEnd()
        {
            _isMoving = false;
            
            // Notify game manager about damage to player
            WaveManager.Instance?.EnemyReachedEnd(this, _damage);
            
            // Destroy enemy
            Destroy(gameObject);
        }

        /// <summary>
        /// Take damage from defenders.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            _currentHealth -= damage;
            
            // Visual feedback
            StartCoroutine(DamageFlash());
            UpdateHealthBar();
            
            if (animator != null)
            {
                animator.SetTrigger(HitTrigger);
            }
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Flash sprite on damage.
        /// </summary>
        private IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;
            
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// Update health bar visual.
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBarFill != null)
            {
                healthBarFill.transform.localScale = new Vector3(HealthPercent, 1f, 1f);
                
                // Color based on health
                if (HealthPercent > 0.5f)
                    healthBarFill.color = Color.green;
                else if (HealthPercent > 0.25f)
                    healthBarFill.color = Color.yellow;
                else
                    healthBarFill.color = Color.red;
            }
        }

        /// <summary>
        /// Handle enemy death.
        /// </summary>
        private void Die()
        {
            _isMoving = false;
            
            if (animator != null)
            {
                animator.SetBool(WalkBool, false);
                animator.SetTrigger(DieTrigger);
            }
            
            // Drop rewards
            DropRewards();
            
            // Play death sound
            if (config?.DeathSound != null)
            {
                ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot(config.DeathSound.name);
            }
            
            // Notify manager
            EnemyManager.Instance?.OnEnemyDeath(this);
            
            // Publish event
            int coinsDrop = config?.GetCoinDrop() ?? 5;
            ServiceLocator.Get<IEventBus>()?.Publish(new EnemyDefeatedEvent(EnemyId, coinsDrop));
            
            // Destroy after animation
            StartCoroutine(DestroyAfterDelay(0.5f));
        }

        /// <summary>
        /// Drop coins and gems on death.
        /// </summary>
        private void DropRewards()
        {
            if (config == null) return;
            
            var currencyService = ServiceLocator.Get<ICurrencyService>();
            if (currencyService == null) return;
            
            // Drop coins
            int coins = config.GetCoinDrop();
            currencyService.AddCoins(coins);
            
            // Chance to drop gems
            if (config.ShouldDropGem())
            {
                currencyService.AddGems(1);
            }
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        /// <summary>
        /// Apply status effect to enemy.
        /// </summary>
        public void ApplyEffect(StatusEffect effect, float duration, float value)
        {
            switch (effect)
            {
                case StatusEffect.Slow:
                    _slowDuration = duration;
                    _slowAmount = value;
                    _moveSpeed = _baseMoveSpeed * (1f - _slowAmount);
                    break;
                    
                case StatusEffect.Stun:
                    _isStunned = true;
                    _stunDuration = duration;
                    break;
                    
                case StatusEffect.Poison:
                    _isPoisoned = true;
                    _poisonDuration = duration;
                    _poisonDamage = value;
                    break;
            }
        }

        /// <summary>
        /// Update active status effects.
        /// </summary>
        private void UpdateStatusEffects()
        {
            float dt = Time.deltaTime;
            
            // Slow
            if (_slowDuration > 0)
            {
                _slowDuration -= dt;
                if (_slowDuration <= 0)
                {
                    _moveSpeed = _baseMoveSpeed;
                }
            }
            
            // Stun
            if (_isStunned)
            {
                _stunDuration -= dt;
                if (_stunDuration <= 0)
                {
                    _isStunned = false;
                }
            }
            
            // Poison
            if (_isPoisoned)
            {
                _poisonDuration -= dt;
                TakeDamage(_poisonDamage * dt);
                
                if (_poisonDuration <= 0)
                {
                    _isPoisoned = false;
                }
            }
        }
    }

    public enum StatusEffect
    {
        None,
        Slow,
        Stun,
        Poison,
        Freeze,
        Burn
    }
}
