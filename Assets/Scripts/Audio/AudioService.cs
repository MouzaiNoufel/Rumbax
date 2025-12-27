using System;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Events;
using Rumbax.Core.Services;
using Rumbax.Data;

namespace Rumbax.Audio
{
    /// <summary>
    /// Sound effect types.
    /// </summary>
    public enum SoundType
    {
        // UI Sounds
        ButtonClick,
        ButtonBack,
        PopupOpen,
        PopupClose,
        TabSwitch,
        
        // Currency
        CoinCollect,
        GemCollect,
        Purchase,
        NotEnoughFunds,
        
        // Gameplay
        DefenderSpawn,
        DefenderMerge,
        DefenderUpgrade,
        DefenderAttack,
        DefenderSpecial,
        
        // Combat
        ProjectileFire,
        ProjectileHit,
        EnemyHit,
        EnemyDeath,
        BossDeath,
        
        // Game State
        WaveStart,
        WaveComplete,
        LevelComplete,
        LevelFailed,
        Victory,
        GameOver,
        
        // Rewards
        RewardClaim,
        AchievementUnlock,
        ChallengeComplete,
        LevelUp,
        
        // Special
        Countdown,
        Notification,
        Error
    }

    /// <summary>
    /// Music track types.
    /// </summary>
    public enum MusicType
    {
        MainMenu,
        Gameplay,
        BossFight,
        Victory,
        GameOver,
        Shop
    }

    /// <summary>
    /// Sound effect configuration.
    /// </summary>
    [System.Serializable]
    public class SoundConfig
    {
        public SoundType Type;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.5f, 2f)] public float MinPitch = 0.95f;
        [Range(0.5f, 2f)] public float MaxPitch = 1.05f;
        public bool RandomizePitch = true;
    }

    /// <summary>
    /// Music track configuration.
    /// </summary>
    [System.Serializable]
    public class MusicConfig
    {
        public MusicType Type;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.5f;
        public bool Loop = true;
        public float FadeInDuration = 1f;
        public float FadeOutDuration = 1f;
    }

    /// <summary>
    /// Audio service implementing sound effects and music playback.
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private int _sfxPoolSize = 10;

        [Header("Sound Configuration")]
        [SerializeField] private List<SoundConfig> _soundConfigs = new List<SoundConfig>();
        [SerializeField] private List<MusicConfig> _musicConfigs = new List<MusicConfig>();

        [Header("Settings")]
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1f;
        private bool _isMusicMuted;
        private bool _isSfxMuted;

        // IAudioService interface properties
        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = Mathf.Clamp01(value); UpdateVolumes(); SaveSettings(); }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = Mathf.Clamp01(value); SaveSettings(); }
        }

        public bool IsMusicMuted
        {
            get => _isMusicMuted;
            set { _isMusicMuted = value; _musicSource.mute = value; SaveSettings(); }
        }

        public bool IsSfxMuted
        {
            get => _isSfxMuted;
            set { _isSfxMuted = value; SaveSettings(); }
        }

        private Dictionary<SoundType, SoundConfig> _soundLookup = new Dictionary<SoundType, SoundConfig>();
        private Dictionary<MusicType, MusicConfig> _musicLookup = new Dictionary<MusicType, MusicConfig>();
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        
        private MusicType _currentMusic;
        private bool _isMusicFading;
        private float _targetMusicVolume;
        private ISaveService _saveService;

        private void Awake()
        {
            ServiceLocator.Register<IAudioService>(this);
            
            InitializeAudioSources();
            BuildLookupTables();
            CreateSFXPool();
        }

        private void Start()
        {
            _saveService = ServiceLocator.Get<ISaveService>();
            LoadSettings();
            SubscribeToEvents();
        }

        private void InitializeAudioSources()
        {
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
            }

            if (_sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                _sfxSource = sfxObj.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
            }
        }

        private void BuildLookupTables()
        {
            _soundLookup.Clear();
            foreach (var config in _soundConfigs)
            {
                _soundLookup[config.Type] = config;
            }

            _musicLookup.Clear();
            foreach (var config in _musicConfigs)
            {
                _musicLookup[config.Type] = config;
            }
        }

        private void CreateSFXPool()
        {
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFX_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxPool.Add(source);
            }
        }

        private void LoadSettings()
        {
            PlayerData data = _saveService?.GetPlayerData();
            if (data?.Settings != null)
            {
                _musicVolume = data.Settings.MusicVolume;
                _sfxVolume = data.Settings.SfxVolume;
            }
            
            UpdateVolumes();
        }

        private void SubscribeToEvents()
        {
            // UI Events
            EventBus.Subscribe<ButtonClickEvent>(e => PlaySound(SoundType.ButtonClick));
            
            // Currency Events
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            
            // Gameplay Events
            EventBus.Subscribe<DefenderSpawnedEvent>(e => PlaySound(SoundType.DefenderSpawn));
            EventBus.Subscribe<DefenderMergedEvent>(e => PlaySound(SoundType.DefenderMerge));
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            
            // Wave Events
            EventBus.Subscribe<WaveStartedEvent>(e => PlaySound(SoundType.WaveStart));
            EventBus.Subscribe<WaveCompletedEvent>(e => PlaySound(SoundType.WaveComplete));
            
            // Level Events
            EventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            
            // Achievement Events
            EventBus.Subscribe<AchievementUnlockedEvent>(e => PlaySound(SoundType.AchievementUnlock));
            EventBus.Subscribe<Systems.ChallengeCompletedEvent>(e => PlaySound(SoundType.ChallengeComplete));
        }

        /// <summary>
        /// Play a sound effect.
        /// </summary>
        public void PlaySound(SoundType type)
        {
            if (_sfxVolume <= 0) return;

            if (!_soundLookup.TryGetValue(type, out SoundConfig config))
            {
                Debug.LogWarning($"[Audio] Sound not found: {type}");
                return;
            }

            if (config.Clips == null || config.Clips.Length == 0)
            {
                return;
            }

            // Get random clip
            AudioClip clip = config.Clips[UnityEngine.Random.Range(0, config.Clips.Length)];
            if (clip == null) return;

            // Get available audio source from pool
            AudioSource source = GetAvailableSFXSource();
            if (source == null) return;

            source.clip = clip;
            source.volume = config.Volume * _sfxVolume * _masterVolume;
            
            if (config.RandomizePitch)
            {
                source.pitch = UnityEngine.Random.Range(config.MinPitch, config.MaxPitch);
            }
            else
            {
                source.pitch = 1f;
            }

            source.Play();
        }

        /// <summary>
        /// Play a sound at a specific position (3D audio).
        /// </summary>
        public void PlaySoundAtPosition(SoundType type, Vector3 position)
        {
            if (_sfxVolume <= 0) return;

            if (!_soundLookup.TryGetValue(type, out SoundConfig config))
            {
                return;
            }

            if (config.Clips == null || config.Clips.Length == 0)
            {
                return;
            }

            AudioClip clip = config.Clips[UnityEngine.Random.Range(0, config.Clips.Length)];
            if (clip == null) return;

            float volume = config.Volume * _sfxVolume * _masterVolume;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return _sfxPool[0]; // Reuse first if all busy
        }

        /// <summary>
        /// Play music track.
        /// </summary>
        public void PlayMusic(MusicType type, bool fade = true)
        {
            if (_currentMusic == type && _musicSource.isPlaying)
            {
                return;
            }

            if (!_musicLookup.TryGetValue(type, out MusicConfig config))
            {
                Debug.LogWarning($"[Audio] Music not found: {type}");
                return;
            }

            if (config.Clip == null) return;

            _currentMusic = type;

            if (fade && _musicSource.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(config));
            }
            else
            {
                _musicSource.clip = config.Clip;
                _musicSource.volume = config.Volume * _musicVolume * _masterVolume;
                _musicSource.loop = config.Loop;
                _musicSource.Play();
            }
        }

        private System.Collections.IEnumerator CrossfadeMusic(MusicConfig newConfig)
        {
            _isMusicFading = true;

            // Fade out current
            float startVolume = _musicSource.volume;
            float elapsed = 0f;
            float fadeOutDuration = 0.5f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeOutDuration);
                yield return null;
            }

            // Switch tracks
            _musicSource.clip = newConfig.Clip;
            _musicSource.loop = newConfig.Loop;
            _musicSource.Play();

            // Fade in new
            float targetVolume = newConfig.Volume * _musicVolume * _masterVolume;
            elapsed = 0f;

            while (elapsed < newConfig.FadeInDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0, targetVolume, elapsed / newConfig.FadeInDuration);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _isMusicFading = false;
        }

        /// <summary>
        /// Stop current music.
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                _musicSource.Stop();
            }
        }

        private System.Collections.IEnumerator FadeOutMusic()
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;
            float fadeDuration = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeDuration);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.volume = startVolume;
        }

        /// <summary>
        /// Pause music.
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        /// <summary>
        /// Resume paused music.
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }

        // IAudioService interface implementations (string-based)
        void IAudioService.PlayMusic(string clipName)
        {
            if (Enum.TryParse<MusicType>(clipName, true, out var musicType))
            {
                PlayMusic(musicType, true);
            }
            else
            {
                Debug.LogWarning($"[Audio] Unknown music type: {clipName}");
            }
        }

        void IAudioService.StopMusic()
        {
            StopMusic(true);
        }

        void IAudioService.PlaySfx(string clipName)
        {
            if (Enum.TryParse<SoundType>(clipName, true, out var soundType))
            {
                PlaySound(soundType);
            }
            else
            {
                Debug.LogWarning($"[Audio] Unknown sound type: {clipName}");
            }
        }

        void IAudioService.PlaySfxOneShot(string clipName)
        {
            if (Enum.TryParse<SoundType>(clipName, true, out var soundType))
            {
                PlaySound(soundType);
            }
        }

        /// <summary>
        /// Set master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            SaveSettings();
        }

        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public float GetMasterVolume() => _masterVolume;
        public float GetMusicVolume() => _musicVolume;
        public float GetSFXVolume() => _sfxVolume;

        private void UpdateVolumes()
        {
            if (!_isMusicFading && _musicSource != null && _musicLookup.TryGetValue(_currentMusic, out MusicConfig config))
            {
                _musicSource.volume = config.Volume * _musicVolume * _masterVolume;
            }
        }

        private void SaveSettings()
        {
            PlayerData data = _saveService?.GetPlayerData();
            if (data?.Settings != null)
            {
                data.Settings.MusicVolume = _musicVolume;
                data.Settings.SfxVolume = _sfxVolume;
                _saveService.UpdatePlayerData(data);
            }
        }

        // Event handlers
        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (evt.Delta > 0)
            {
                if (evt.Type == CurrencyType.Coins)
                {
                    PlaySound(SoundType.CoinCollect);
                }
                else if (evt.Type == CurrencyType.Gems)
                {
                    PlaySound(SoundType.GemCollect);
                }
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (evt.IsBoss)
            {
                PlaySound(SoundType.BossDeath);
            }
            else
            {
                PlaySound(SoundType.EnemyDeath);
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            PlaySound(SoundType.LevelComplete);
            PlayMusic(MusicType.Victory, true);
        }

        private void OnGameOver(GameOverEvent evt)
        {
            PlaySound(SoundType.GameOver);
            PlayMusic(MusicType.GameOver, true);
        }
    }

    // Additional events for audio triggers
    public class ButtonClickEvent : Core.Events.IGameEvent { }
}
