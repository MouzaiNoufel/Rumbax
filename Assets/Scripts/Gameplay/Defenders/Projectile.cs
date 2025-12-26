using UnityEngine;
using Rumbax.Data;
using Rumbax.Gameplay.Enemies;

namespace Rumbax.Gameplay.Defenders
{
    /// <summary>
    /// Projectile fired by defenders towards enemies.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool isHoming = true;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem hitParticles;
        
        private Enemy _target;
        private float _damage;
        private DefenderType _defenderType;
        private Vector3 _direction;
        private float _timer;
        private bool _hasHit;

        private void Update()
        {
            if (_hasHit) return;
            
            _timer += Time.deltaTime;
            
            if (_timer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            MoveTowardsTarget();
        }

        /// <summary>
        /// Initialize projectile with target and damage.
        /// </summary>
        public void Initialize(Enemy target, float damage, DefenderType type)
        {
            _target = target;
            _damage = damage;
            _defenderType = type;
            
            if (target != null)
            {
                _direction = (target.transform.position - transform.position).normalized;
                
                // Rotate to face target
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// Move towards target or in initial direction.
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (isHoming && _target != null && _target.IsAlive)
            {
                _direction = (_target.transform.position - transform.position).normalized;
                
                // Update rotation for homing
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            
            transform.position += _direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;
            
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                Hit(enemy);
            }
        }

        /// <summary>
        /// Handle hit on enemy.
        /// </summary>
        private void Hit(Enemy enemy)
        {
            _hasHit = true;
            
            // Deal damage
            enemy.TakeDamage(_damage);
            
            // Apply special effects based on defender type
            ApplySpecialEffect(enemy);
            
            // Play hit effect
            PlayHitEffect();
            
            // Destroy projectile
            if (trailRenderer != null)
            {
                trailRenderer.transform.SetParent(null);
                Destroy(trailRenderer.gameObject, trailRenderer.time);
            }
            
            Destroy(gameObject);
        }

        /// <summary>
        /// Apply special effects based on defender type.
        /// </summary>
        private void ApplySpecialEffect(Enemy enemy)
        {
            switch (_defenderType)
            {
                case DefenderType.Mage:
                    // Mages could apply slow
                    enemy.ApplyEffect(StatusEffect.Slow, 2f, 0.5f);
                    break;
                case DefenderType.Assassin:
                    // Assassins have critical chance
                    if (Random.value < 0.2f)
                    {
                        enemy.TakeDamage(_damage); // Double damage on crit
                    }
                    break;
            }
        }

        /// <summary>
        /// Play hit visual effect.
        /// </summary>
        private void PlayHitEffect()
        {
            if (hitParticles != null)
            {
                var particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, particles.main.duration);
            }
        }
    }
}
