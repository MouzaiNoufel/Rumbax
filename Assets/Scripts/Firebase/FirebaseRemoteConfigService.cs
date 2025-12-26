using System;
using UnityEngine;
using Rumbax.Core.Services;

namespace Rumbax.Firebase
{
    /// <summary>
    /// Firebase Remote Config for dynamic configuration updates.
    /// </summary>
    public class FirebaseRemoteConfigService : MonoBehaviour, IRemoteConfigService
    {
        [Header("Settings")]
        [SerializeField] private float fetchInterval = 3600f; // 1 hour
        [SerializeField] private float minimumFetchInterval = 300f; // 5 minutes for development
        
        private bool _isInitialized;
        private float _lastFetchTime;

        public bool IsInitialized => _isInitialized;

        public event Action OnConfigFetched;
        public event Action<string> OnConfigFetchFailed;

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[RemoteConfig] Initializing...");

            // Actual Firebase implementation:
            /*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                if (task.Result == Firebase.DependencyStatus.Available)
                {
                    SetDefaults();
                    _isInitialized = true;
                    FetchConfig();
                }
            });
            */

            SetDefaults();
            _isInitialized = true;
            
            Debug.Log("[RemoteConfig] Initialized (simulated)");
        }

        private void SetDefaults()
        {
            // Actual implementation:
            /*
            var defaults = new Dictionary<string, object>
            {
                { "spawn_cost_base", 50 },
                { "spawn_cost_multiplier", 1.2f },
                { "interstitial_frequency", 3 },
                { "rewarded_ad_coins", 100 },
                { "rewarded_ad_gems", 5 },
                { "max_offline_hours", 8 },
                { "offline_earnings_rate", 0.5f },
                { "daily_reward_multiplier", 1.0f },
                { "event_active", false },
                { "event_name", "" },
                { "event_multiplier", 1.0f },
                { "maintenance_mode", false },
                { "force_update_version", "1.0.0" },
                { "review_prompt_level", 5 }
            };
            
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                .SetDefaultsAsync(defaults);
            */
            
            Debug.Log("[RemoteConfig] Defaults set");
        }

        public void FetchConfig(Action<bool> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[RemoteConfig] Not initialized");
                callback?.Invoke(false);
                return;
            }

            Debug.Log("[RemoteConfig] Fetching config...");

            // Actual Firebase implementation:
            /*
            var configSettings = new ConfigSettings
            {
                MinimumFetchIntervalInMilliseconds = (ulong)(minimumFetchInterval * 1000)
            };
            
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                .SetConfigSettingsAsync(configSettings)
                .ContinueWith(_ => {
                    return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                        .FetchAsync(TimeSpan.FromSeconds(fetchInterval));
                })
                .Unwrap()
                .ContinueWith(fetchTask => {
                    if (fetchTask.IsFaulted)
                    {
                        Debug.LogError($"[RemoteConfig] Fetch failed: {fetchTask.Exception}");
                        OnConfigFetchFailed?.Invoke(fetchTask.Exception.Message);
                        callback?.Invoke(false);
                        return;
                    }
                    
                    return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                        .ActivateAsync();
                })
                .Unwrap()
                .ContinueWith(activateTask => {
                    if (activateTask.IsFaulted)
                    {
                        OnConfigFetchFailed?.Invoke(activateTask.Exception.Message);
                        callback?.Invoke(false);
                        return;
                    }
                    
                    _lastFetchTime = Time.time;
                    OnConfigFetched?.Invoke();
                    callback?.Invoke(true);
                    Debug.Log("[RemoteConfig] Config fetched and activated");
                });
            */

            // Simulated fetch
            _lastFetchTime = Time.time;
            Debug.Log("[RemoteConfig] Config fetched (simulated)");
            OnConfigFetched?.Invoke();
            callback?.Invoke(true);
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (!_isInitialized) return defaultValue;

            // Actual implementation:
            // return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
            //     .GetValue(key).StringValue;

            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (!_isInitialized) return defaultValue;

            // Actual implementation:
            // return (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
            //     .GetValue(key).LongValue;

            // Return some defaults for common keys
            return key switch
            {
                "spawn_cost_base" => 50,
                "interstitial_frequency" => 3,
                "rewarded_ad_coins" => 100,
                "rewarded_ad_gems" => 5,
                "max_offline_hours" => 8,
                "review_prompt_level" => 5,
                _ => defaultValue
            };
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (!_isInitialized) return defaultValue;

            // Actual implementation:
            // return (float)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
            //     .GetValue(key).DoubleValue;

            return key switch
            {
                "spawn_cost_multiplier" => 1.2f,
                "offline_earnings_rate" => 0.5f,
                "daily_reward_multiplier" => 1.0f,
                "event_multiplier" => 1.0f,
                _ => defaultValue
            };
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!_isInitialized) return defaultValue;

            // Actual implementation:
            // return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
            //     .GetValue(key).BooleanValue;

            return key switch
            {
                "event_active" => false,
                "maintenance_mode" => false,
                _ => defaultValue
            };
        }

        public T GetJson<T>(string key, T defaultValue = default)
        {
            if (!_isInitialized) return defaultValue;

            try
            {
                string json = GetString(key);
                if (string.IsNullOrEmpty(json)) return defaultValue;
                
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RemoteConfig] Failed to parse JSON for {key}: {e.Message}");
                return defaultValue;
            }
        }
    }
}
