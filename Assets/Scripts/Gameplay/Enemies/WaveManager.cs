using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;

namespace Rumbax.Gameplay.Enemies
{
    /// <summary>
    /// Manages wave spawning and progression.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        private static WaveManager _instance;
        public static WaveManager Instance => _instance;

        [Header("Wave Configuration")]
        [SerializeField] private int totalWaves = 10;
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private int enemiesIncreasePerWave = 2;
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private float enemySpawnInterval = 1f;
        
        [Header("Enemy Types")]
        [SerializeField] private List<EnemyConfig> enemyConfigs;
        [SerializeField] private EnemyConfig bossConfig;
        
        [Header("Player Health")]
        [SerializeField] private float maxPlayerHealth = 100f;
        
        private int _currentWave;
        private int _enemiesRemainingToSpawn;
        private int _enemiesRemainingInWave;
        private float _playerHealth;
        private bool _isWaveActive;
        private bool _isSpawning;
        private Coroutine _waveCoroutine;
        
        public int CurrentWave => _currentWave;
        public int TotalWaves => totalWaves;
        public float PlayerHealth => _playerHealth;
        public float MaxPlayerHealth => maxPlayerHealth;
        public float PlayerHealthPercent => _playerHealth / maxPlayerHealth;
        public bool IsWaveActive => _isWaveActive;
        public bool AllWavesComplete => _currentWave > totalWaves;

        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        public event System.Action OnAllWavesCompleted;
        public event System.Action<float> OnPlayerHealthChanged;
        public event System.Action OnPlayerDeath;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            InitializeFromConfig();
            _playerHealth = maxPlayerHealth;
        }

        private void InitializeFromConfig()
        {
            var gameManager = GameManager.Instance;
            if (gameManager?.Config != null)
            {
                var config = gameManager.Config;
                baseEnemiesPerWave = config.EnemiesPerWave;
                enemiesIncreasePerWave = config.EnemyIncreasePerWave;
                timeBetweenWaves = config.TimeBetweenWaves;
                enemySpawnInterval = config.EnemySpawnInterval;
            }
        }

        /// <summary>
        /// Start the wave system.
        /// </summary>
        public void StartWaves()
        {
            _currentWave = 0;
            _playerHealth = maxPlayerHealth;
            StartNextWave();
        }

        /// <summary>
        /// Start the next wave.
        /// </summary>
        public void StartNextWave()
        {
            if (_isWaveActive) return;
            if (_currentWave >= totalWaves)
            {
                CompleteAllWaves();
                return;
            }

            _currentWave++;
            _isWaveActive = true;
            
            // Calculate enemies for this wave
            _enemiesRemainingToSpawn = CalculateEnemiesForWave(_currentWave);
            _enemiesRemainingInWave = _enemiesRemainingToSpawn;
            
            // Publish event
            ServiceLocator.Get<IEventBus>()?.Publish(new WaveStartedEvent(_currentWave, totalWaves));
            OnWaveStarted?.Invoke(_currentWave);
            
            // Start spawning
            _waveCoroutine = StartCoroutine(SpawnWaveEnemies());
            
            Debug.Log($"[WaveManager] Wave {_currentWave} started with {_enemiesRemainingToSpawn} enemies");
        }

        /// <summary>
        /// Calculate number of enemies for a wave.
        /// </summary>
        private int CalculateEnemiesForWave(int wave)
        {
            return baseEnemiesPerWave + (wave - 1) * enemiesIncreasePerWave;
        }

        /// <summary>
        /// Coroutine to spawn enemies over time.
        /// </summary>
        private IEnumerator SpawnWaveEnemies()
        {
            _isSpawning = true;
            
            while (_enemiesRemainingToSpawn > 0)
            {
                SpawnEnemy();
                _enemiesRemainingToSpawn--;
                
                yield return new WaitForSeconds(enemySpawnInterval);
            }
            
            _isSpawning = false;
        }

        /// <summary>
        /// Spawn a single enemy.
        /// </summary>
        private void SpawnEnemy()
        {
            // Check if should spawn boss (last wave, last enemy)
            bool spawnBoss = _currentWave == totalWaves && 
                            _enemiesRemainingToSpawn == 1 && 
                            bossConfig != null;
            
            EnemyConfig config = spawnBoss ? bossConfig : GetRandomEnemyConfig();
            
            if (config != null)
            {
                EnemyManager.Instance?.SpawnEnemy(config, _currentWave);
            }
        }

        /// <summary>
        /// Get random enemy config weighted by wave number.
        /// </summary>
        private EnemyConfig GetRandomEnemyConfig()
        {
            if (enemyConfigs == null || enemyConfigs.Count == 0) return null;
            
            // Simple random for now - can add weighted selection
            int index = Random.Range(0, enemyConfigs.Count);
            return enemyConfigs[index];
        }

        /// <summary>
        /// Check if current wave is complete.
        /// </summary>
        public void CheckWaveComplete()
        {
            if (!_isWaveActive) return;
            
            _enemiesRemainingInWave--;
            
            if (_enemiesRemainingInWave <= 0 && !_isSpawning)
            {
                CompleteWave();
            }
        }

        /// <summary>
        /// Complete current wave.
        /// </summary>
        private void CompleteWave()
        {
            _isWaveActive = false;
            
            ServiceLocator.Get<IEventBus>()?.Publish(new WaveCompletedEvent(_currentWave));
            OnWaveCompleted?.Invoke(_currentWave);
            
            Debug.Log($"[WaveManager] Wave {_currentWave} completed");
            
            // Check if all waves complete
            if (_currentWave >= totalWaves)
            {
                CompleteAllWaves();
            }
            else
            {
                // Start countdown for next wave
                StartCoroutine(WaitAndStartNextWave());
            }
        }

        private IEnumerator WaitAndStartNextWave()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNextWave();
        }

        /// <summary>
        /// Complete all waves (victory).
        /// </summary>
        private void CompleteAllWaves()
        {
            Debug.Log("[WaveManager] All waves completed - Victory!");
            
            OnAllWavesCompleted?.Invoke();
            GameManager.Instance?.ChangeState(GameState.Victory);
        }

        /// <summary>
        /// Called when an enemy reaches the player's base.
        /// </summary>
        public void EnemyReachedEnd(Enemy enemy, float damage)
        {
            _playerHealth -= damage;
            _playerHealth = Mathf.Max(0, _playerHealth);
            
            OnPlayerHealthChanged?.Invoke(_playerHealth);
            
            if (_playerHealth <= 0)
            {
                PlayerDeath();
            }
            
            // Also decrement enemy count
            _enemiesRemainingInWave--;
            
            if (_enemiesRemainingInWave <= 0 && !_isSpawning && _isWaveActive)
            {
                CompleteWave();
            }
        }

        /// <summary>
        /// Handle player death (game over).
        /// </summary>
        private void PlayerDeath()
        {
            Debug.Log("[WaveManager] Player died - Game Over");
            
            // Stop spawning
            if (_waveCoroutine != null)
            {
                StopCoroutine(_waveCoroutine);
            }
            _isWaveActive = false;
            _isSpawning = false;
            
            OnPlayerDeath?.Invoke();
            
            ServiceLocator.Get<IEventBus>()?.Publish(new LevelFailedEvent(
                GameManager.Instance?.CurrentLevel ?? 1, "Player health depleted"));
            
            GameManager.Instance?.ChangeState(GameState.GameOver);
        }

        /// <summary>
        /// Revive player (from watching ad).
        /// </summary>
        public void RevivePlayer(float healthPercent = 0.5f)
        {
            _playerHealth = maxPlayerHealth * healthPercent;
            OnPlayerHealthChanged?.Invoke(_playerHealth);
            
            // Resume if was game over
            if (!_isWaveActive && _currentWave <= totalWaves)
            {
                StartNextWave();
            }
            
            GameManager.Instance?.ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Skip current wave (for testing).
        /// </summary>
        public void SkipWave()
        {
            EnemyManager.Instance?.ClearAllEnemies();
            _enemiesRemainingInWave = 0;
            _enemiesRemainingToSpawn = 0;
            
            if (_waveCoroutine != null)
            {
                StopCoroutine(_waveCoroutine);
            }
            
            CompleteWave();
        }

        /// <summary>
        /// Reset wave manager for new game.
        /// </summary>
        public void Reset()
        {
            if (_waveCoroutine != null)
            {
                StopCoroutine(_waveCoroutine);
            }
            
            _currentWave = 0;
            _playerHealth = maxPlayerHealth;
            _isWaveActive = false;
            _isSpawning = false;
            
            EnemyManager.Instance?.ClearAllEnemies();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
