using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;
using Rumbax.Gameplay.Grid;

namespace Rumbax.Gameplay.Defenders
{
    /// <summary>
    /// Handles spawning new defenders on the grid.
    /// </summary>
    public class DefenderSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GameObject defenderPrefab;
        
        [Header("Spawn Configuration")]
        [SerializeField] private List<DefenderConfig> availableDefenders;
        [SerializeField] private int baseSpawnCost = 50;
        [SerializeField] private float spawnCostMultiplier = 1.2f;
        
        private int _spawnCount;
        private ICurrencyService _currencyService;

        public int CurrentSpawnCost => CalculateSpawnCost();
        public bool CanSpawn => gridManager.HasEmptyCell && 
                                _currencyService.CanAffordCoins(CurrentSpawnCost);

        private void Start()
        {
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            
            if (GameManager.Instance?.Config != null)
            {
                baseSpawnCost = GameManager.Instance.Config.BaseSpawnCost;
                spawnCostMultiplier = GameManager.Instance.Config.SpawnCostMultiplier;
            }
        }

        /// <summary>
        /// Calculate spawn cost based on number of spawns.
        /// </summary>
        private int CalculateSpawnCost()
        {
            return Mathf.RoundToInt(baseSpawnCost * Mathf.Pow(spawnCostMultiplier, _spawnCount));
        }

        /// <summary>
        /// Try to spawn a new defender.
        /// </summary>
        public bool TrySpawn()
        {
            if (!CanSpawn)
            {
                Debug.Log("[DefenderSpawner] Cannot spawn - no empty cells or insufficient funds");
                return false;
            }

            int cost = CurrentSpawnCost;
            
            if (!_currencyService.SpendCoins(cost))
            {
                Debug.Log("[DefenderSpawner] Cannot spawn - failed to spend coins");
                return false;
            }

            var cell = gridManager.GetRandomEmptyCell();
            if (cell == null)
            {
                // Refund if no cell available
                _currencyService.AddCoins(cost);
                return false;
            }

            SpawnDefender(cell);
            _spawnCount++;
            
            return true;
        }

        /// <summary>
        /// Spawn a defender at specific cell.
        /// </summary>
        private void SpawnDefender(GridCell cell)
        {
            if (defenderPrefab == null || availableDefenders.Count == 0)
            {
                Debug.LogError("[DefenderSpawner] Missing prefab or defender configs");
                return;
            }

            // Get random defender config (weighted towards lower tier initially)
            var config = GetRandomDefenderConfig();
            
            // Instantiate defender
            var defenderObj = Instantiate(defenderPrefab, cell.transform.position, Quaternion.identity);
            defenderObj.name = $"Defender_{config.DefenderName}_Lv1";
            
            var defender = defenderObj.GetComponent<Defender>();
            if (defender == null)
            {
                defender = defenderObj.AddComponent<Defender>();
            }
            
            defender.Initialize(config, 1);
            
            // Place on grid
            cell.SetDefender(defender);
            gridManager.SetCellOccupied(cell, true);
            
            // Play spawn effect
            PlaySpawnEffect(cell.transform.position);
            
            // Publish event
            ServiceLocator.Get<IEventBus>()?.Publish(new DefenderSpawnedEvent(config.DefenderId, 1));
            
            Debug.Log($"[DefenderSpawner] Spawned {config.DefenderName} at ({cell.X}, {cell.Y})");
        }

        /// <summary>
        /// Get random defender config with weighting.
        /// </summary>
        private DefenderConfig GetRandomDefenderConfig()
        {
            // Simple random selection - can be enhanced with rarity weights
            int index = Random.Range(0, availableDefenders.Count);
            return availableDefenders[index];
        }

        /// <summary>
        /// Play visual effect at spawn location.
        /// </summary>
        private void PlaySpawnEffect(Vector3 position)
        {
            // Spawn particles or effect here
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("spawn");
        }

        /// <summary>
        /// Spawn multiple defenders at once (for ads reward or special events).
        /// </summary>
        public int SpawnMultiple(int count, bool free = false)
        {
            int spawned = 0;
            
            for (int i = 0; i < count; i++)
            {
                if (!gridManager.HasEmptyCell) break;
                
                if (free)
                {
                    var cell = gridManager.GetRandomEmptyCell();
                    if (cell != null)
                    {
                        SpawnDefender(cell);
                        spawned++;
                    }
                }
                else if (TrySpawn())
                {
                    spawned++;
                }
            }
            
            return spawned;
        }

        /// <summary>
        /// Reset spawn count (for new level).
        /// </summary>
        public void ResetSpawnCount()
        {
            _spawnCount = 0;
        }
    }
}
