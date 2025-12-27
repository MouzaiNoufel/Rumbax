using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Rumbax.Systems
{
    /// <summary>
    /// Settings System - Complete settings panel with audio, graphics, and gameplay options.
    /// Professional mobile game feature.
    /// </summary>
    public class SettingsSystem : MonoBehaviour
    {
        public static SettingsSystem Instance { get; private set; }

        // Audio Settings
        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 0.7f;
        public float SFXVolume { get; private set; } = 1f;
        public bool MusicEnabled { get; private set; } = true;
        public bool SFXEnabled { get; private set; } = true;

        // Gameplay Settings
        public bool HapticFeedback { get; private set; } = true;
        public bool ShowDamageNumbers { get; private set; } = true;
        public bool ShowTutorialHints { get; private set; } = true;
        public bool AutoStartWaves { get; private set; } = false;
        public int DefaultGameSpeed { get; private set; } = 1;
        public bool ConfirmMerge { get; private set; } = false;

        // Graphics Settings
        public int GraphicsQuality { get; private set; } = 2; // 0=Low, 1=Med, 2=High
        public bool ShowParticles { get; private set; } = true;
        public bool ScreenShakeEnabled { get; private set; } = true;
        public int TargetFrameRate { get; private set; } = 60;
        public bool BatteryOptimization { get; private set; } = false;

        // Accessibility
        public float UIScale { get; private set; } = 1f;
        public bool HighContrastMode { get; private set; } = false;
        public bool LargeText { get; private set; } = false;
        public bool ReducedMotion { get; private set; } = false;

        // Language
        public string Language { get; private set; } = "en";

        // Notifications
        public bool NotificationsEnabled { get; private set; } = true;
        public bool DailyRewardReminder { get; private set; } = true;
        public bool EnergyFullReminder { get; private set; } = true;

        // Events
        public event Action OnSettingsChanged;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<int> OnGraphicsQualityChanged;
        public event Action<string> OnLanguageChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
                ApplySettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // === AUDIO SETTINGS ===

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            OnMasterVolumeChanged?.Invoke(MasterVolume);
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            OnMusicVolumeChanged?.Invoke(MusicVolume);
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            OnSFXVolumeChanged?.Invoke(SFXVolume);
            SaveSettings();
        }

        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            if (Testing.SimpleAudioManager.Instance != null)
            {
                Testing.SimpleAudioManager.Instance.SetMusicEnabled(enabled);
            }
            SaveSettings();
        }

        public void SetSFXEnabled(bool enabled)
        {
            SFXEnabled = enabled;
            if (Testing.SimpleAudioManager.Instance != null)
            {
                Testing.SimpleAudioManager.Instance.SetSFXEnabled(enabled);
            }
            SaveSettings();
        }

        // === GAMEPLAY SETTINGS ===

        public void SetHapticFeedback(bool enabled)
        {
            HapticFeedback = enabled;
            SaveSettings();
        }

        public void SetShowDamageNumbers(bool show)
        {
            ShowDamageNumbers = show;
            SaveSettings();
        }

        public void SetShowTutorialHints(bool show)
        {
            ShowTutorialHints = show;
            SaveSettings();
        }

        public void SetAutoStartWaves(bool auto)
        {
            AutoStartWaves = auto;
            SaveSettings();
        }

        public void SetDefaultGameSpeed(int speed)
        {
            DefaultGameSpeed = Mathf.Clamp(speed, 1, 3);
            SaveSettings();
        }

        public void SetConfirmMerge(bool confirm)
        {
            ConfirmMerge = confirm;
            SaveSettings();
        }

        // === GRAPHICS SETTINGS ===

        public void SetGraphicsQuality(int quality)
        {
            GraphicsQuality = Mathf.Clamp(quality, 0, 2);
            ApplyGraphicsQuality();
            OnGraphicsQualityChanged?.Invoke(GraphicsQuality);
            SaveSettings();
        }

        public void SetShowParticles(bool show)
        {
            ShowParticles = show;
            SaveSettings();
        }

        public void SetScreenShakeEnabled(bool enabled)
        {
            ScreenShakeEnabled = enabled;
            SaveSettings();
        }

        public void SetTargetFrameRate(int fps)
        {
            TargetFrameRate = Mathf.Clamp(fps, 30, 120);
            Application.targetFrameRate = TargetFrameRate;
            SaveSettings();
        }

        public void SetBatteryOptimization(bool enabled)
        {
            BatteryOptimization = enabled;
            if (enabled)
            {
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                Application.targetFrameRate = TargetFrameRate;
                QualitySettings.vSyncCount = 0;
            }
            SaveSettings();
        }

        // === ACCESSIBILITY ===

        public void SetUIScale(float scale)
        {
            UIScale = Mathf.Clamp(scale, 0.8f, 1.5f);
            SaveSettings();
        }

        public void SetHighContrastMode(bool enabled)
        {
            HighContrastMode = enabled;
            SaveSettings();
        }

        public void SetLargeText(bool enabled)
        {
            LargeText = enabled;
            SaveSettings();
        }

        public void SetReducedMotion(bool enabled)
        {
            ReducedMotion = enabled;
            ScreenShakeEnabled = !enabled;
            ShowParticles = !enabled;
            SaveSettings();
        }

        // === LANGUAGE ===

        public void SetLanguage(string lang)
        {
            Language = lang;
            OnLanguageChanged?.Invoke(lang);
            SaveSettings();
        }

        public string[] GetAvailableLanguages()
        {
            return new[] { "en", "es", "fr", "de", "pt", "ru", "ja", "ko", "zh" };
        }

        public string GetLanguageName(string code)
        {
            return code switch
            {
                "en" => "English",
                "es" => "Español",
                "fr" => "Français",
                "de" => "Deutsch",
                "pt" => "Português",
                "ru" => "Русский",
                "ja" => "日本語",
                "ko" => "한국어",
                "zh" => "中文",
                _ => "English"
            };
        }

        // === NOTIFICATIONS ===

        public void SetNotificationsEnabled(bool enabled)
        {
            NotificationsEnabled = enabled;
            SaveSettings();
        }

        public void SetDailyRewardReminder(bool enabled)
        {
            DailyRewardReminder = enabled;
            SaveSettings();
        }

        public void SetEnergyFullReminder(bool enabled)
        {
            EnergyFullReminder = enabled;
            SaveSettings();
        }

        // === APPLY/RESET ===

        private void ApplySettings()
        {
            // Apply framerate
            Application.targetFrameRate = BatteryOptimization ? 30 : TargetFrameRate;

            // Apply graphics
            ApplyGraphicsQuality();

            // Apply audio
            if (Testing.SimpleAudioManager.Instance != null)
            {
                Testing.SimpleAudioManager.Instance.SetMusicEnabled(MusicEnabled);
                Testing.SimpleAudioManager.Instance.SetSFXEnabled(SFXEnabled);
            }

            OnSettingsChanged?.Invoke();
        }

        private void ApplyGraphicsQuality()
        {
            QualitySettings.SetQualityLevel(GraphicsQuality);

            switch (GraphicsQuality)
            {
                case 0: // Low
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.softParticles = false;
                    break;
                case 1: // Medium
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.softParticles = false;
                    break;
                case 2: // High
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.softParticles = true;
                    break;
            }
        }

        public void ResetToDefaults()
        {
            // Audio
            MasterVolume = 1f;
            MusicVolume = 0.7f;
            SFXVolume = 1f;
            MusicEnabled = true;
            SFXEnabled = true;

            // Gameplay
            HapticFeedback = true;
            ShowDamageNumbers = true;
            ShowTutorialHints = true;
            AutoStartWaves = false;
            DefaultGameSpeed = 1;
            ConfirmMerge = false;

            // Graphics
            GraphicsQuality = 2;
            ShowParticles = true;
            ScreenShakeEnabled = true;
            TargetFrameRate = 60;
            BatteryOptimization = false;

            // Accessibility
            UIScale = 1f;
            HighContrastMode = false;
            LargeText = false;
            ReducedMotion = false;

            // Language
            Language = "en";

            // Notifications
            NotificationsEnabled = true;
            DailyRewardReminder = true;
            EnergyFullReminder = true;

            ApplySettings();
            SaveSettings();
        }

        // === SAVE/LOAD ===

        private void SaveSettings()
        {
            // Audio
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            PlayerPrefs.SetInt("MusicEnabled", MusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("SFXEnabled", SFXEnabled ? 1 : 0);

            // Gameplay
            PlayerPrefs.SetInt("HapticFeedback", HapticFeedback ? 1 : 0);
            PlayerPrefs.SetInt("ShowDamageNumbers", ShowDamageNumbers ? 1 : 0);
            PlayerPrefs.SetInt("ShowTutorialHints", ShowTutorialHints ? 1 : 0);
            PlayerPrefs.SetInt("AutoStartWaves", AutoStartWaves ? 1 : 0);
            PlayerPrefs.SetInt("DefaultGameSpeed", DefaultGameSpeed);
            PlayerPrefs.SetInt("ConfirmMerge", ConfirmMerge ? 1 : 0);

            // Graphics
            PlayerPrefs.SetInt("GraphicsQuality", GraphicsQuality);
            PlayerPrefs.SetInt("ShowParticles", ShowParticles ? 1 : 0);
            PlayerPrefs.SetInt("ScreenShakeEnabled", ScreenShakeEnabled ? 1 : 0);
            PlayerPrefs.SetInt("TargetFrameRate", TargetFrameRate);
            PlayerPrefs.SetInt("BatteryOptimization", BatteryOptimization ? 1 : 0);

            // Accessibility
            PlayerPrefs.SetFloat("UIScale", UIScale);
            PlayerPrefs.SetInt("HighContrastMode", HighContrastMode ? 1 : 0);
            PlayerPrefs.SetInt("LargeText", LargeText ? 1 : 0);
            PlayerPrefs.SetInt("ReducedMotion", ReducedMotion ? 1 : 0);

            // Language
            PlayerPrefs.SetString("Language", Language);

            // Notifications
            PlayerPrefs.SetInt("NotificationsEnabled", NotificationsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("DailyRewardReminder", DailyRewardReminder ? 1 : 0);
            PlayerPrefs.SetInt("EnergyFullReminder", EnergyFullReminder ? 1 : 0);

            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            // Audio
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            MusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            SFXEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

            // Gameplay
            HapticFeedback = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
            ShowDamageNumbers = PlayerPrefs.GetInt("ShowDamageNumbers", 1) == 1;
            ShowTutorialHints = PlayerPrefs.GetInt("ShowTutorialHints", 1) == 1;
            AutoStartWaves = PlayerPrefs.GetInt("AutoStartWaves", 0) == 1;
            DefaultGameSpeed = PlayerPrefs.GetInt("DefaultGameSpeed", 1);
            ConfirmMerge = PlayerPrefs.GetInt("ConfirmMerge", 0) == 1;

            // Graphics
            GraphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
            ShowParticles = PlayerPrefs.GetInt("ShowParticles", 1) == 1;
            ScreenShakeEnabled = PlayerPrefs.GetInt("ScreenShakeEnabled", 1) == 1;
            TargetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
            BatteryOptimization = PlayerPrefs.GetInt("BatteryOptimization", 0) == 1;

            // Accessibility
            UIScale = PlayerPrefs.GetFloat("UIScale", 1f);
            HighContrastMode = PlayerPrefs.GetInt("HighContrastMode", 0) == 1;
            LargeText = PlayerPrefs.GetInt("LargeText", 0) == 1;
            ReducedMotion = PlayerPrefs.GetInt("ReducedMotion", 0) == 1;

            // Language
            Language = PlayerPrefs.GetString("Language", "en");

            // Notifications
            NotificationsEnabled = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
            DailyRewardReminder = PlayerPrefs.GetInt("DailyRewardReminder", 1) == 1;
            EnergyFullReminder = PlayerPrefs.GetInt("EnergyFullReminder", 1) == 1;
        }

        // === HAPTIC FEEDBACK ===

        public void TriggerHaptic(HapticType type = HapticType.Light)
        {
            if (!HapticFeedback) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    long duration = type switch
                    {
                        HapticType.Light => 10,
                        HapticType.Medium => 25,
                        HapticType.Heavy => 50,
                        HapticType.Success => 30,
                        HapticType.Error => 100,
                        _ => 10
                    };
                    vibrator.Call("vibrate", duration);
                }
            }
            catch (Exception) { }
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS haptic feedback
            // Handheld.Vibrate(); // Basic vibration
#endif
        }

        // === UI HELPER METHODS ===

        public string GetQualityName(int quality)
        {
            return quality switch
            {
                0 => "Low",
                1 => "Medium",
                2 => "High",
                _ => "Medium"
            };
        }

        public string GetFrameRateName(int fps)
        {
            return fps switch
            {
                30 => "30 FPS",
                60 => "60 FPS",
                120 => "120 FPS",
                _ => $"{fps} FPS"
            };
        }
    }

    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Error
    }
}
