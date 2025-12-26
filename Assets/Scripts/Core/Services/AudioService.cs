using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Service for managing audio (music and sound effects).
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] musicClips;
        [SerializeField] private AudioClip[] sfxClips;

        private Dictionary<string, AudioClip> _musicLibrary = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _sfxLibrary = new Dictionary<string, AudioClip>();
        
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 1f;
        private bool _isMusicMuted;
        private bool _isSfxMuted;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (musicSource != null && !_isMusicMuted)
                {
                    musicSource.volume = _musicVolume;
                }
                SaveSettings();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (sfxSource != null && !_isSfxMuted)
                {
                    sfxSource.volume = _sfxVolume;
                }
                SaveSettings();
            }
        }

        public bool IsMusicMuted
        {
            get => _isMusicMuted;
            set
            {
                _isMusicMuted = value;
                if (musicSource != null)
                {
                    musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
                }
                SaveSettings();
            }
        }

        public bool IsSfxMuted
        {
            get => _isSfxMuted;
            set
            {
                _isSfxMuted = value;
                SaveSettings();
            }
        }

        private void Awake()
        {
            InitializeAudioSources();
            BuildLibraries();
            LoadSettings();
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                var musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                var sfxObj = new GameObject("SfxSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void BuildLibraries()
        {
            if (musicClips != null)
            {
                foreach (var clip in musicClips)
                {
                    if (clip != null && !_musicLibrary.ContainsKey(clip.name))
                    {
                        _musicLibrary.Add(clip.name, clip);
                    }
                }
            }

            if (sfxClips != null)
            {
                foreach (var clip in sfxClips)
                {
                    if (clip != null && !_sfxLibrary.ContainsKey(clip.name))
                    {
                        _sfxLibrary.Add(clip.name, clip);
                    }
                }
            }
        }

        private void LoadSettings()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                var data = saveService.GetPlayerData();
                if (data != null && data.Settings != null)
                {
                    _musicVolume = data.Settings.MusicVolume;
                    _sfxVolume = data.Settings.SfxVolume;
                    _isMusicMuted = !data.Settings.MusicEnabled;
                    _isSfxMuted = !data.Settings.SfxEnabled;
                    
                    ApplySettings();
                }
            }
        }

        private void SaveSettings()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                var data = saveService.GetPlayerData();
                if (data != null && data.Settings != null)
                {
                    data.Settings.MusicVolume = _musicVolume;
                    data.Settings.SfxVolume = _sfxVolume;
                    data.Settings.MusicEnabled = !_isMusicMuted;
                    data.Settings.SfxEnabled = !_isSfxMuted;
                }
            }
        }

        private void ApplySettings()
        {
            if (musicSource != null)
            {
                musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = _isSfxMuted ? 0f : _sfxVolume;
            }
        }

        public void PlayMusic(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;

            if (_musicLibrary.TryGetValue(clipName, out var clip))
            {
                if (musicSource.clip != clip)
                {
                    musicSource.clip = clip;
                    musicSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"[AudioService] Music clip not found: {clipName}");
            }
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void PlaySfx(string clipName)
        {
            if (string.IsNullOrEmpty(clipName) || _isSfxMuted) return;

            if (_sfxLibrary.TryGetValue(clipName, out var clip))
            {
                sfxSource.clip = clip;
                sfxSource.Play();
            }
            else
            {
                Debug.LogWarning($"[AudioService] SFX clip not found: {clipName}");
            }
        }

        public void PlaySfxOneShot(string clipName)
        {
            if (string.IsNullOrEmpty(clipName) || _isSfxMuted) return;

            if (_sfxLibrary.TryGetValue(clipName, out var clip))
            {
                sfxSource.PlayOneShot(clip, _sfxVolume);
            }
            else
            {
                Debug.LogWarning($"[AudioService] SFX clip not found: {clipName}");
            }
        }

        /// <summary>
        /// Register an audio clip at runtime.
        /// </summary>
        public void RegisterClip(string name, AudioClip clip, bool isMusic)
        {
            if (string.IsNullOrEmpty(name) || clip == null) return;

            var library = isMusic ? _musicLibrary : _sfxLibrary;
            
            if (!library.ContainsKey(name))
            {
                library.Add(name, clip);
            }
        }
    }
}
