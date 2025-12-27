using UnityEngine;
using System.Collections.Generic;

namespace Rumbax.Testing
{
    /// <summary>
    /// Simple audio manager that generates procedural sounds for testing.
    /// No external audio files needed - everything is synthesized at runtime.
    /// </summary>
    public class SimpleAudioManager : MonoBehaviour
    {
        public static SimpleAudioManager Instance { get; private set; }

        // Settings
        private float _musicVolume = 0.5f;
        private float _sfxVolume = 0.8f;
        private bool _musicEnabled = true;
        private bool _sfxEnabled = true;

        // Audio sources
        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private AudioSource _uiSource;

        // Generated clips
        private Dictionary<string, AudioClip> _cachedClips = new Dictionary<string, AudioClip>();

        // Constants
        private const int SAMPLE_RATE = 44100;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Create audio sources
            _musicSource = CreateAudioSource("MusicSource", true, 0.3f);
            _sfxSource = CreateAudioSource("SFXSource", false, 0.8f);
            _uiSource = CreateAudioSource("UISource", false, 0.6f);

            // Pre-generate common clips
            GenerateAllClips();

            // Start ambient music
            PlayMusic();
        }

        private AudioSource CreateAudioSource(string name, bool loop, float volume)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            return source;
        }

        private void GenerateAllClips()
        {
            // UI Sounds
            _cachedClips["click"] = GenerateClick();
            _cachedClips["success"] = GenerateSuccess();
            _cachedClips["error"] = GenerateError();
            _cachedClips["coin"] = GenerateCoin();
            _cachedClips["upgrade"] = GenerateUpgrade();

            // Game Sounds
            _cachedClips["spawn"] = GenerateSpawn();
            _cachedClips["merge"] = GenerateMerge();
            _cachedClips["shoot"] = GenerateShoot();
            _cachedClips["hit"] = GenerateHit();
            _cachedClips["enemyDeath"] = GenerateEnemyDeath();
            _cachedClips["wave_start"] = GenerateWaveStart();
            _cachedClips["victory"] = GenerateVictory();
            _cachedClips["defeat"] = GenerateDefeat();

            // Music
            _cachedClips["music_ambient"] = GenerateAmbientMusic();

            UnityEngine.Debug.Log("[SimpleAudioManager] Generated all audio clips");
        }

        // === SOUND GENERATORS ===

        private AudioClip GenerateClick()
        {
            int samples = SAMPLE_RATE / 20; // 50ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = 1f - (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 800 * t) * envelope * 0.3f;
            }

            return CreateClip("click", data);
        }

        private AudioClip GenerateSuccess()
        {
            int samples = SAMPLE_RATE / 4; // 250ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 440 + 220 * ((float)i / samples); // Rising pitch
                float envelope = Mathf.Clamp01(1f - (float)i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            return CreateClip("success", data);
        }

        private AudioClip GenerateError()
        {
            int samples = SAMPLE_RATE / 5; // 200ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 200 - 50 * ((float)i / samples); // Falling pitch
                float envelope = 1f - (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            return CreateClip("error", data);
        }

        private AudioClip GenerateCoin()
        {
            int samples = SAMPLE_RATE / 8; // 125ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq1 = 1200;
                float freq2 = 1500;
                float blend = (float)i / samples;
                float freq = Mathf.Lerp(freq1, freq2, blend);
                float envelope = 1f - blend;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.35f;
            }

            return CreateClip("coin", data);
        }

        private AudioClip GenerateUpgrade()
        {
            int samples = SAMPLE_RATE / 2; // 500ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float freq = 300 + 500 * progress;
                float envelope = Mathf.Sin(Mathf.PI * progress);
                float harmonics = Mathf.Sin(2f * Mathf.PI * freq * t) +
                                  0.5f * Mathf.Sin(2f * Mathf.PI * freq * 2 * t) +
                                  0.25f * Mathf.Sin(2f * Mathf.PI * freq * 3 * t);
                data[i] = harmonics * envelope * 0.2f;
            }

            return CreateClip("upgrade", data);
        }

        private AudioClip GenerateSpawn()
        {
            int samples = SAMPLE_RATE / 6; // ~166ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float freq = 600 + 200 * Mathf.Sin(Mathf.PI * progress * 2);
                float envelope = Mathf.Sin(Mathf.PI * progress);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.35f;
            }

            return CreateClip("spawn", data);
        }

        private AudioClip GenerateMerge()
        {
            int samples = SAMPLE_RATE / 3; // ~333ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;

                // Two tones that harmonize
                float freq1 = 400 + 300 * progress;
                float freq2 = 600 + 450 * progress;
                float envelope = 1f - progress * progress;

                data[i] = (Mathf.Sin(2f * Mathf.PI * freq1 * t) +
                           0.7f * Mathf.Sin(2f * Mathf.PI * freq2 * t)) * envelope * 0.25f;
            }

            return CreateClip("merge", data);
        }

        private AudioClip GenerateShoot()
        {
            int samples = SAMPLE_RATE / 12; // ~83ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float freq = 1000 - 400 * progress;
                float envelope = 1f - progress;
                // Add some noise
                float noise = (Random.value - 0.5f) * 0.1f * envelope;
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.9f + noise) * envelope * 0.25f;
            }

            return CreateClip("shoot", data);
        }

        private AudioClip GenerateHit()
        {
            int samples = SAMPLE_RATE / 10; // 100ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float envelope = Mathf.Exp(-progress * 8);

                // Impact sound
                float impact = Mathf.Sin(2f * Mathf.PI * 150 * t) +
                               0.5f * Mathf.Sin(2f * Mathf.PI * 80 * t);
                // Noise burst
                float noise = (Random.value - 0.5f) * Mathf.Exp(-progress * 15);

                data[i] = (impact * 0.7f + noise * 0.3f) * envelope * 0.4f;
            }

            return CreateClip("hit", data);
        }

        private AudioClip GenerateEnemyDeath()
        {
            int samples = SAMPLE_RATE / 4; // 250ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float freq = 400 - 300 * progress;
                float envelope = Mathf.Exp(-progress * 4);

                // Descending tone with wobble
                float wobble = Mathf.Sin(Mathf.PI * progress * 8) * 50;
                data[i] = Mathf.Sin(2f * Mathf.PI * (freq + wobble) * t) * envelope * 0.35f;
            }

            return CreateClip("enemyDeath", data);
        }

        private AudioClip GenerateWaveStart()
        {
            int samples = SAMPLE_RATE; // 1 second
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;

                // War horn sound
                float freq = 200 + 50 * Mathf.Sin(Mathf.PI * progress);
                float envelope = Mathf.Sin(Mathf.PI * progress);

                float harmonics = Mathf.Sin(2f * Mathf.PI * freq * t) +
                                  0.6f * Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) +
                                  0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2 * t);

                data[i] = harmonics * envelope * 0.3f;
            }

            return CreateClip("wave_start", data);
        }

        private AudioClip GenerateVictory()
        {
            int samples = SAMPLE_RATE * 2; // 2 seconds
            float[] data = new float[samples];

            // Victory fanfare - three rising chords
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f }; // C5, E5, G5, C6

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;

                int noteIndex = Mathf.Min((int)(progress * notes.Length), notes.Length - 1);
                float freq = notes[noteIndex];

                float localProgress = (progress * notes.Length) % 1f;
                float envelope = Mathf.Sin(Mathf.PI * localProgress) * (1f - progress * 0.5f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.25f;
            }

            return CreateClip("victory", data);
        }

        private AudioClip GenerateDefeat()
        {
            int samples = SAMPLE_RATE * 2; // 2 seconds
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;

                // Sad descending minor chord
                float freq = 400 - 200 * progress;
                float envelope = 1f - progress;

                float harmonics = Mathf.Sin(2f * Mathf.PI * freq * t) +
                                  0.5f * Mathf.Sin(2f * Mathf.PI * freq * 1.2f * t) + // Minor third
                                  0.3f * Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t);

                data[i] = harmonics * envelope * 0.2f;
            }

            return CreateClip("defeat", data);
        }

        private AudioClip GenerateAmbientMusic()
        {
            int samples = SAMPLE_RATE * 8; // 8 second loop
            float[] data = new float[samples];

            // Simple ambient pad
            float[] chord = { 130.81f, 164.81f, 196f, 261.63f }; // C3, E3, G3, C4

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float loopProgress = (float)(i % (SAMPLE_RATE * 4)) / (SAMPLE_RATE * 4);

                float sample = 0;
                for (int n = 0; n < chord.Length; n++)
                {
                    float notePhase = (n * 0.5f + loopProgress) % 1f;
                    float noteEnvelope = Mathf.Sin(Mathf.PI * notePhase) * 0.5f + 0.5f;
                    sample += Mathf.Sin(2f * Mathf.PI * chord[n] * t) * noteEnvelope * 0.1f;
                }

                // Add subtle movement
                float lfo = Mathf.Sin(2f * Mathf.PI * 0.2f * t) * 0.1f + 0.9f;
                data[i] = sample * lfo;
            }

            return CreateClip("music_ambient", data);
        }

        private AudioClip CreateClip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        // === PUBLIC API ===

        public void PlaySFX(string soundName, float volumeScale = 1f)
        {
            if (!_sfxEnabled) return;
            if (_cachedClips.TryGetValue(soundName, out AudioClip clip))
            {
                _sfxSource.PlayOneShot(clip, _sfxVolume * volumeScale);
            }
        }

        public void PlayUI(string soundName, float volumeScale = 1f)
        {
            if (!_sfxEnabled) return;
            if (_cachedClips.TryGetValue(soundName, out AudioClip clip))
            {
                _uiSource.PlayOneShot(clip, volumeScale);
            }
        }

        public void PlayClick() => PlayUI("click");
        public void PlaySuccess() => PlayUI("success");
        public void PlayError() => PlayUI("error");
        public void PlayCoin() => PlayUI("coin");
        public void PlayUpgrade() => PlayUI("upgrade");

        public void PlaySpawn() => PlaySFX("spawn");
        public void PlayMerge() => PlaySFX("merge");
        public void PlayShoot() => PlaySFX("shoot", 0.5f);
        public void PlayHit() => PlaySFX("hit");
        public void PlayEnemyDeath() => PlaySFX("enemyDeath");
        public void PlayWaveStart() => PlaySFX("wave_start");
        public void PlayVictory() => PlaySFX("victory");
        public void PlayDefeat() => PlaySFX("defeat");

        public void PlayMusic()
        {
            if (!_musicEnabled) return;
            if (_cachedClips.TryGetValue("music_ambient", out AudioClip clip))
            {
                _musicSource.clip = clip;
                _musicSource.volume = _musicVolume;
                _musicSource.Play();
            }
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            if (enabled)
                PlayMusic();
            else
                StopMusic();
        }

        public void SetSFXEnabled(bool enabled)
        {
            _sfxEnabled = enabled;
        }

        public float GetMusicVolume() => _musicVolume;
        public float GetSFXVolume() => _sfxVolume;
        public bool IsMusicEnabled() => _musicEnabled;
        public bool IsSFXEnabled() => _sfxEnabled;
    }
}
