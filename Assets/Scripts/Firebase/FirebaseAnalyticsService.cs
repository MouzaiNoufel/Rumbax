using System;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core.Services;

namespace Rumbax.Firebase
{
    /// <summary>
    /// Firebase Analytics service for tracking game events.
    /// </summary>
    public class FirebaseAnalyticsService : MonoBehaviour, IAnalyticsService
    {
        private bool _isInitialized;
        private Dictionary<string, object> _userProperties = new Dictionary<string, object>();

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[FirebaseAnalytics] Initializing...");

            // Actual Firebase implementation:
            /*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    _isInitialized = true;
                    Debug.Log("[FirebaseAnalytics] Initialized successfully");
                }
                else
                {
                    Debug.LogError($"[FirebaseAnalytics] Could not resolve dependencies: {dependencyStatus}");
                }
            });
            */

            _isInitialized = true;
            Debug.Log("[FirebaseAnalytics] Initialized (simulated)");
        }

        public void LogEvent(string eventName)
        {
            LogEvent(eventName, null);
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[FirebaseAnalytics] Not initialized, skipping event: {eventName}");
                return;
            }

            Debug.Log($"[FirebaseAnalytics] Event: {eventName}");
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Debug.Log($"  - {param.Key}: {param.Value}");
                }
            }

            // Actual implementation:
            /*
            if (parameters == null)
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            else
            {
                var firebaseParams = new List<Parameter>();
                foreach (var param in parameters)
                {
                    if (param.Value is int intVal)
                        firebaseParams.Add(new Parameter(param.Key, intVal));
                    else if (param.Value is long longVal)
                        firebaseParams.Add(new Parameter(param.Key, longVal));
                    else if (param.Value is double doubleVal)
                        firebaseParams.Add(new Parameter(param.Key, doubleVal));
                    else
                        firebaseParams.Add(new Parameter(param.Key, param.Value.ToString()));
                }
                FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
            }
            */
        }

        public void LogLevelStart(int level)
        {
            LogEvent("level_start", new Dictionary<string, object>
            {
                { "level", level },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }

        public void LogLevelComplete(int level, int score, float duration)
        {
            LogEvent("level_complete", new Dictionary<string, object>
            {
                { "level", level },
                { "score", score },
                { "duration_seconds", (int)duration },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }

        public void LogLevelFail(int level, string reason)
        {
            LogEvent("level_fail", new Dictionary<string, object>
            {
                { "level", level },
                { "reason", reason },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }

        public void LogPurchase(string productId, decimal amount, string currency)
        {
            LogEvent("purchase", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "value", (double)amount },
                { "currency", currency }
            });

            // Also log to Firebase Analytics ecommerce
            /*
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase,
                new Parameter(FirebaseAnalytics.ParameterItemId, productId),
                new Parameter(FirebaseAnalytics.ParameterValue, (double)amount),
                new Parameter(FirebaseAnalytics.ParameterCurrency, currency));
            */
        }

        public void LogAdWatched(string adType, string placement)
        {
            LogEvent("ad_watched", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "placement", placement }
            });
        }

        public void SetUserProperty(string property, string value)
        {
            _userProperties[property] = value;
            
            Debug.Log($"[FirebaseAnalytics] User property: {property} = {value}");

            // Actual implementation:
            // FirebaseAnalytics.SetUserProperty(property, value);
        }
    }
}
