using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Events;

namespace Rumbax.VFX
{
    /// <summary>
    /// Types of visual effects.
    /// </summary>
    public enum VFXType
    {
        // Combat Effects
        DefenderAttack,
        DefenderMerge,
        DefenderSpawn,
        DefenderUpgrade,
        
        // Projectile Effects
        ProjectileTrail,
        ProjectileImpact,
        ProjectileExplosion,
        
        // Enemy Effects
        EnemyHit,
        EnemyDeath,
        EnemySlow,
        EnemyPoison,
        EnemyStun,
        
        // Boss Effects
        BossSpawn,
        BossDeath,
        BossAttack,
        
        // Currency Effects
        CoinCollect,
        GemCollect,
        CoinBurst,
        GemBurst,
        
        // UI Effects
        ButtonClick,
        LevelUp,
        StarUnlock,
        AchievementUnlock,
        
        // Game State Effects
        WaveStart,
        WaveComplete,
        Victory,
        Defeat,
        
        // Environment
        GridHighlight,
        DropZone
    }

    /// <summary>
    /// VFX configuration.
    /// </summary>
    [System.Serializable]
    public class VFXConfig
    {
        public VFXType Type;
        public GameObject Prefab;
        public int PoolSize = 5;
        public float Duration = 2f;
        public bool AttachToTarget = false;
    }

    /// <summary>
    /// Manages visual effects with object pooling.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("VFX Library")]
        [SerializeField] private List<VFXConfig> _vfxConfigs = new List<VFXConfig>();

        [Header("Pool Settings")]
        [SerializeField] private Transform _poolParent;

        private Dictionary<VFXType, Queue<VFXInstance>> _pools = new Dictionary<VFXType, Queue<VFXInstance>>();
        private Dictionary<VFXType, VFXConfig> _configLookup = new Dictionary<VFXType, VFXConfig>();

        private static VFXManager _instance;
        public static VFXManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (_poolParent == null)
            {
                _poolParent = new GameObject("VFX_Pool").transform;
                _poolParent.SetParent(transform);
            }

            InitializePools();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void InitializePools()
        {
            foreach (var config in _vfxConfigs)
            {
                if (config.Prefab == null) continue;

                _configLookup[config.Type] = config;
                _pools[config.Type] = new Queue<VFXInstance>();

                for (int i = 0; i < config.PoolSize; i++)
                {
                    CreateVFXInstance(config);
                }
            }
        }

        private VFXInstance CreateVFXInstance(VFXConfig config)
        {
            GameObject obj = Instantiate(config.Prefab, _poolParent);
            VFXInstance instance = obj.AddComponent<VFXInstance>();
            instance.Initialize(config.Type, config.Duration);
            obj.SetActive(false);
            _pools[config.Type].Enqueue(instance);
            return instance;
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DefenderSpawnedEvent>(e => Play(VFXType.DefenderSpawn, e.Position));
            EventBus.Subscribe<DefenderMergedEvent>(e => Play(VFXType.DefenderMerge, e.Position));
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyHitEvent>(e => Play(VFXType.EnemyHit, e.Position));
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            EventBus.Subscribe<WaveStartedEvent>(e => Play(VFXType.WaveStart, Vector3.zero));
            EventBus.Subscribe<WaveCompletedEvent>(e => Play(VFXType.WaveComplete, Vector3.zero));
            EventBus.Subscribe<AchievementUnlockedEvent>(e => Play(VFXType.AchievementUnlock, Vector3.zero));
        }

        /// <summary>
        /// Play a VFX at a position.
        /// </summary>
        public void Play(VFXType type, Vector3 position)
        {
            Play(type, position, Quaternion.identity, null);
        }

        /// <summary>
        /// Play a VFX at a position with rotation.
        /// </summary>
        public void Play(VFXType type, Vector3 position, Quaternion rotation)
        {
            Play(type, position, rotation, null);
        }

        /// <summary>
        /// Play a VFX attached to a target.
        /// </summary>
        public void PlayAttached(VFXType type, Transform target)
        {
            if (target == null) return;
            Play(type, target.position, Quaternion.identity, target);
        }

        /// <summary>
        /// Play a VFX.
        /// </summary>
        public void Play(VFXType type, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!_pools.TryGetValue(type, out Queue<VFXInstance> pool))
            {
                Debug.LogWarning($"[VFX] No pool for type: {type}");
                return;
            }

            VFXInstance instance;

            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
            }
            else
            {
                // Pool exhausted, create new instance
                if (_configLookup.TryGetValue(type, out VFXConfig config))
                {
                    instance = CreateVFXInstance(config);
                    _pools[type].Dequeue(); // Remove it since we'll use it immediately
                }
                else
                {
                    return;
                }
            }

            if (instance == null) return;

            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.Play(OnVFXComplete);
        }

        private void OnVFXComplete(VFXInstance instance)
        {
            instance.transform.SetParent(_poolParent);
            instance.gameObject.SetActive(false);

            if (_pools.TryGetValue(instance.Type, out Queue<VFXInstance> pool))
            {
                pool.Enqueue(instance);
            }
        }

        /// <summary>
        /// Play burst of coins at position.
        /// </summary>
        public void PlayCoinBurst(Vector3 position, int count)
        {
            for (int i = 0; i < Mathf.Min(count / 10, 10); i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0
                );
                Play(VFXType.CoinCollect, position + offset);
            }
        }

        /// <summary>
        /// Play burst of gems at position.
        /// </summary>
        public void PlayGemBurst(Vector3 position, int count)
        {
            for (int i = 0; i < Mathf.Min(count, 5); i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0
                );
                Play(VFXType.GemCollect, position + offset);
            }
        }

        // Event handlers
        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (evt.IsBoss)
            {
                Play(VFXType.BossDeath, evt.Position);
                PlayCoinBurst(evt.Position, evt.CoinReward);
            }
            else
            {
                Play(VFXType.EnemyDeath, evt.Position);
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Amount <= 0) return;
            
            // Currency pickup VFX is typically handled by the entity that grants it
            // This is for UI feedback
        }
    }

    /// <summary>
    /// Individual VFX instance that handles its own lifecycle.
    /// </summary>
    public class VFXInstance : MonoBehaviour
    {
        public VFXType Type { get; private set; }
        
        private float _duration;
        private float _timer;
        private bool _isPlaying;
        private ParticleSystem _particleSystem;
        private System.Action<VFXInstance> _onComplete;

        public void Initialize(VFXType type, float duration)
        {
            Type = type;
            _duration = duration;
            _particleSystem = GetComponent<ParticleSystem>();
            
            if (_particleSystem == null)
            {
                _particleSystem = GetComponentInChildren<ParticleSystem>();
            }
        }

        public void Play(System.Action<VFXInstance> onComplete)
        {
            _onComplete = onComplete;
            _timer = 0f;
            _isPlaying = true;
            
            gameObject.SetActive(true);

            if (_particleSystem != null)
            {
                _particleSystem.Clear();
                _particleSystem.Play();
            }
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer += Time.deltaTime;

            // Check if particle system is done or duration exceeded
            bool particleDone = _particleSystem == null || !_particleSystem.isPlaying;
            bool durationExceeded = _timer >= _duration;

            if (particleDone || durationExceeded)
            {
                Stop();
            }
        }

        public void Stop()
        {
            _isPlaying = false;

            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            _onComplete?.Invoke(this);
        }
    }

    // Additional event for enemy hit
    public class EnemyHitEvent : Rumbax.Core.Events.IGameEvent
    {
        public Vector3 Position;
        public int Damage;
    }
}
