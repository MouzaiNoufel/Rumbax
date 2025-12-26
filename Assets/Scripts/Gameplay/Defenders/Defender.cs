using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core.Services;
using Rumbax.Data;
using Rumbax.Gameplay.Grid;
using Rumbax.Gameplay.Enemies;

namespace Rumbax.Gameplay.Defenders
{
    /// <summary>
    /// Base defender class that attacks enemies.
    /// </summary>
    public class Defender : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DefenderConfig config;
        
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform projectileSpawnPoint;
        
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        
        public string DefenderId => config?.DefenderId ?? "";
        public string DefenderType => config?.DefenderName ?? "";
        public int Level { get; private set; } = 1;
        public GridCell CurrentCell { get; private set; }
        public bool IsAttacking { get; private set; }
        
        private float _damage;
        private float _attackSpeed;
        private float _range;
        private float _attackTimer;
        private Enemy _currentTarget;
        private bool _isDragging;
        private Vector3 _originalPosition;
        private int _originalSortingOrder;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int IdleTrigger = Animator.StringToHash("Idle");

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

        private void Start()
        {
            UpdateStats();
        }

        private void Update()
        {
            if (_isDragging) return;
            
            _attackTimer += Time.deltaTime;
            
            // Find target if we don't have one
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                FindTarget();
            }
            
            // Attack if ready
            if (_currentTarget != null && _attackTimer >= 1f / _attackSpeed)
            {
                Attack();
                _attackTimer = 0f;
            }
        }

        /// <summary>
        /// Initialize defender with configuration.
        /// </summary>
        public void Initialize(DefenderConfig defenderConfig, int level = 1)
        {
            config = defenderConfig;
            Level = level;
            UpdateStats();
            UpdateVisual();
        }

        /// <summary>
        /// Set the defender's level.
        /// </summary>
        public void SetLevel(int level)
        {
            Level = level;
            UpdateStats();
            UpdateVisual();
        }

        /// <summary>
        /// Update stats based on level.
        /// </summary>
        private void UpdateStats()
        {
            if (config == null) return;
            
            _damage = config.GetDamageAtLevel(Level);
            _attackSpeed = config.GetAttackSpeedAtLevel(Level);
            _range = config.GetRangeAtLevel(Level);
        }

        /// <summary>
        /// Update visual appearance based on level.
        /// </summary>
        private void UpdateVisual()
        {
            if (config == null || spriteRenderer == null) return;
            
            spriteRenderer.sprite = config.GetSpriteForLevel(Level);
            
            // Apply tier color tint
            spriteRenderer.color = config.TierColor;
        }

        /// <summary>
        /// Find the nearest enemy in range.
        /// </summary>
        private void FindTarget()
        {
            _currentTarget = null;
            float closestDistance = _range;

            var enemies = EnemyManager.Instance?.GetActiveEnemies();
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    _currentTarget = enemy;
                }
            }
        }

        /// <summary>
        /// Attack the current target.
        /// </summary>
        private void Attack()
        {
            if (_currentTarget == null) return;
            
            IsAttacking = true;
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger(AttackTrigger);
            }
            
            // Play attack sound
            if (config?.AttackSound != null)
            {
                ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot(config.AttackSound.name);
            }
            
            // Spawn projectile or deal direct damage
            if (projectilePrefab != null)
            {
                SpawnProjectile();
            }
            else
            {
                DealDamage();
            }
            
            StartCoroutine(ResetAttackState());
        }

        /// <summary>
        /// Spawn a projectile towards the target.
        /// </summary>
        private void SpawnProjectile()
        {
            Vector3 spawnPos = projectileSpawnPoint != null ? 
                projectileSpawnPoint.position : transform.position;
            
            var projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var projectile = projectileObj.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Initialize(_currentTarget, _damage, config.Type);
            }
        }

        /// <summary>
        /// Deal direct damage to target.
        /// </summary>
        private void DealDamage()
        {
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                _currentTarget.TakeDamage(_damage);
            }
        }

        private IEnumerator ResetAttackState()
        {
            yield return new WaitForSeconds(0.3f);
            IsAttacking = false;
        }

        /// <summary>
        /// Set the cell this defender occupies.
        /// </summary>
        public void SetCell(GridCell cell)
        {
            CurrentCell = cell;
        }

        /// <summary>
        /// Move to a new position with animation.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            StartCoroutine(MoveAnimation(position));
        }

        private IEnumerator MoveAnimation(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float duration = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // Smoothstep
                
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            transform.position = targetPosition;
        }

        /// <summary>
        /// Start dragging this defender.
        /// </summary>
        public void StartDrag()
        {
            _isDragging = true;
            _originalPosition = transform.position;
            _originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 100;
            
            transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// Update position while dragging.
        /// </summary>
        public void UpdateDragPosition(Vector3 position)
        {
            if (_isDragging)
            {
                transform.position = position;
            }
        }

        /// <summary>
        /// End dragging this defender.
        /// </summary>
        public void EndDrag()
        {
            _isDragging = false;
            spriteRenderer.sortingOrder = _originalSortingOrder;
            transform.localScale = Vector3.one;
            
            if (CurrentCell != null)
            {
                transform.position = CurrentCell.transform.position;
            }
        }

        /// <summary>
        /// Get current stats for UI display.
        /// </summary>
        public DefenderStats GetStats()
        {
            return new DefenderStats
            {
                Damage = _damage,
                AttackSpeed = _attackSpeed,
                Range = _range,
                Level = Level
            };
        }

        private void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _range > 0 ? _range : 3f);
        }
    }

    [System.Serializable]
    public struct DefenderStats
    {
        public float Damage;
        public float AttackSpeed;
        public float Range;
        public int Level;
    }
}
