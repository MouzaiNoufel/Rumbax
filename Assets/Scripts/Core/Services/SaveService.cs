using System;
using System.IO;
using UnityEngine;
using Rumbax.Data;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Service for saving and loading game data.
    /// Uses JSON serialization with PlayerPrefs backup.
    /// </summary>
    public class SaveService : ISaveService, IDisposable
    {
        private const string SAVE_KEY = "RumbaxPlayerData";
        private const string SAVE_FILE = "player_data.json";
        private const int CURRENT_DATA_VERSION = 1;

        private PlayerData _playerData;
        private readonly string _savePath;

        public SaveService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        }

        public void SaveGame()
        {
            if (_playerData == null)
            {
                Debug.LogWarning("[SaveService] No player data to save");
                return;
            }

            try
            {
                _playerData.LastPlayTime = DateTime.UtcNow;
                _playerData.DataVersion = CURRENT_DATA_VERSION;
                
                string json = JsonUtility.ToJson(_playerData, true);
                
                // Save to file
                File.WriteAllText(_savePath, json);
                
                // Backup to PlayerPrefs
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
                
                Debug.Log("[SaveService] Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save game: {e.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                string json = null;
                
                // Try to load from file first
                if (File.Exists(_savePath))
                {
                    json = File.ReadAllText(_savePath);
                }
                // Fallback to PlayerPrefs
                else if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    json = PlayerPrefs.GetString(SAVE_KEY);
                }
                
                if (!string.IsNullOrEmpty(json))
                {
                    _playerData = JsonUtility.FromJson<PlayerData>(json);
                    MigrateDataIfNeeded();
                    Debug.Log("[SaveService] Game loaded successfully");
                }
                else
                {
                    CreateNewPlayerData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to load game: {e.Message}");
                CreateNewPlayerData();
            }
        }

        private void CreateNewPlayerData()
        {
            _playerData = new PlayerData();
            
            // Apply default values from GameConfig if available
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.Config != null)
            {
                _playerData.Coins = gameManager.Config.StartingCoins;
                _playerData.Gems = gameManager.Config.StartingGems;
                _playerData.CoinsPerMinute = gameManager.Config.BaseCoinsPerMinute;
            }
            else
            {
                _playerData.Coins = 100;
                _playerData.Gems = 10;
                _playerData.CoinsPerMinute = 10f;
            }
            
            Debug.Log("[SaveService] Created new player data");
            SaveGame();
        }

        private void MigrateDataIfNeeded()
        {
            if (_playerData == null) return;
            
            // Handle data migration between versions
            if (_playerData.DataVersion < CURRENT_DATA_VERSION)
            {
                // Migration logic here
                _playerData.DataVersion = CURRENT_DATA_VERSION;
                SaveGame();
                Debug.Log("[SaveService] Player data migrated to version " + CURRENT_DATA_VERSION);
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                }
                
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    PlayerPrefs.DeleteKey(SAVE_KEY);
                    PlayerPrefs.Save();
                }
                
                _playerData = null;
                Debug.Log("[SaveService] Save data deleted");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to delete save: {e.Message}");
            }
        }

        public PlayerData GetPlayerData()
        {
            if (_playerData == null)
            {
                LoadGame();
            }
            return _playerData;
        }

        public void UpdatePlayerData(PlayerData data)
        {
            _playerData = data;
            SaveGame();
        }

        public bool HasSaveData()
        {
            return File.Exists(_savePath) || PlayerPrefs.HasKey(SAVE_KEY);
        }

        public void Dispose()
        {
            SaveGame();
        }
    }
}
