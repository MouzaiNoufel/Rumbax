using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rumbax.Core.Events
{
    /// <summary>
    /// Event Bus interface for publish-subscribe pattern.
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
        void Publish<T>(T gameEvent) where T : IGameEvent;
        void Clear();
    }

    /// <summary>
    /// Event Bus implementation using publish-subscribe pattern.
    /// Enables loose coupling between game systems.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<Delegate>();
            }
            
            if (!_handlers[type].Contains(handler))
            {
                _handlers[type].Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            
            if (_handlers.ContainsKey(type))
            {
                _handlers[type].Remove(handler);
            }
        }

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var type = typeof(T);
            
            if (!_handlers.ContainsKey(type)) return;
            
            var handlers = _handlers[type].ToArray();
            
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(gameEvent);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error invoking handler for {type.Name}: {e.Message}");
                }
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }

    /// <summary>
    /// Marker interface for all game events.
    /// </summary>
    public interface IGameEvent { }
}
