using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Service Locator pattern implementation for dependency management.
    /// Provides centralized access to game services.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized) return;
            
            _services.Clear();
            _isInitialized = true;
            
            Debug.Log("[ServiceLocator] Initialized");
        }

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Replacing...");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
            }
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            
            Debug.LogError($"[ServiceLocator] Service {type.Name} not found!");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }
            
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
        }

        public static void Cleanup()
        {
            foreach (var service in _services.Values)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _services.Clear();
            _isInitialized = false;
            
            Debug.Log("[ServiceLocator] Cleaned up");
        }
    }
}
