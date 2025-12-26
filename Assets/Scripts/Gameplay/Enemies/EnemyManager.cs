using System.Collections.Generic;
using UnityEngine;
using Rumbax.Data;

namespace Rumbax.Gameplay.Enemies
{
    /// <summary>
    /// Manages all active enemies in the game.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        private static EnemyManager _instance;
        public static EnemyManager Instance => _instance;

        [Header("Enemy Prefab")]
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform[] waypoints;
        
        [Header("Enemy Pool")]
        [SerializeField] private int initialPoolSize = 20;
        
        private List<Enemy> _activeEnemies = new List<Enemy>();
        private Queue<GameObject> _enemyPool = new Queue<GameObject>();
        private Transform _poolParent;

        public int ActiveEnemyCount => _activeEnemies.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            InitializePool();
        }

        private void InitializePool()
        {
            _poolParent = new GameObject("EnemyPool").transform;
            _poolParent.SetParent(transform);
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledEnemy();
            }
        }

        private GameObject CreatePooledEnemy()
        {
            if (enemyPrefab == null)
            {
                var obj = new GameObject("Enemy");
                obj.AddComponent<Enemy>();
                obj.AddComponent<SpriteRenderer>();
                obj.AddComponent<CircleCollider2D>().isTrigger = true;
                obj.SetActive(false);
                obj.transform.SetParent(_poolParent);
                _enemyPool.Enqueue(obj);
                return obj;
            }
            
            var enemy = Instantiate(enemyPrefab, _poolParent);
            enemy.SetActive(false);
            _enemyPool.Enqueue(enemy);
            return enemy;
        }

        /// <summary>
        /// Get enemy from pool or create new one.
        /// </summary>
        private GameObject GetFromPool()
        {
            if (_enemyPool.Count == 0)
            {
                CreatePooledEnemy();
            }
            
            var enemy = _enemyPool.Dequeue();
            enemy.SetActive(true);
            return enemy;
        }

        /// <summary>
        /// Return enemy to pool.
        /// </summary>
        public void ReturnToPool(GameObject enemy)
        {
            enemy.SetActive(false);
            enemy.transform.SetParent(_poolParent);
            _enemyPool.Enqueue(enemy);
        }

        /// <summary>
        /// Spawn an enemy of specified type.
        /// </summary>
        public Enemy SpawnEnemy(EnemyConfig config, int waveNumber)
        {
            if (config == null)
            {
                Debug.LogError("[EnemyManager] Cannot spawn enemy - config is null");
                return null;
            }

            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            
            var enemyObj = GetFromPool();
            enemyObj.transform.position = spawnPos;
            enemyObj.transform.SetParent(transform);
            
            var enemy = enemyObj.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = enemyObj.AddComponent<Enemy>();
            }
            
            enemy.Initialize(config, waveNumber, waypoints);
            _activeEnemies.Add(enemy);
            
            return enemy;
        }

        /// <summary>
        /// Called when an enemy dies.
        /// </summary>
        public void OnEnemyDeath(Enemy enemy)
        {
            _activeEnemies.Remove(enemy);
            
            // Check if wave is complete
            WaveManager.Instance?.CheckWaveComplete();
        }

        /// <summary>
        /// Get all active enemies.
        /// </summary>
        public List<Enemy> GetActiveEnemies()
        {
            // Clean up null references
            _activeEnemies.RemoveAll(e => e == null);
            return _activeEnemies;
        }

        /// <summary>
        /// Get closest enemy to a position.
        /// </summary>
        public Enemy GetClosestEnemy(Vector3 position, float maxRange = float.MaxValue)
        {
            Enemy closest = null;
            float closestDistance = maxRange;
            
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
            
            return closest;
        }

        /// <summary>
        /// Get enemies in range of a position.
        /// </summary>
        public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
        {
            var inRange = new List<Enemy>();
            
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance <= range)
                {
                    inRange.Add(enemy);
                }
            }
            
            return inRange;
        }

        /// <summary>
        /// Clear all active enemies.
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    ReturnToPool(enemy.gameObject);
                }
            }
            _activeEnemies.Clear();
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
