using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core.Services;

namespace Rumbax.UI
{
    /// <summary>
    /// Settings panel for audio and game options.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        
        [Header("Game Settings")]
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Toggle notificationsToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        
        [Header("Account")]
        [SerializeField] private Button signInButton;
        [SerializeField] private Button signOutButton;
        [SerializeField] private TextMeshProUGUI accountStatusText;
        
        [Header("Other")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsButton;
        [SerializeField] private TextMeshProUGUI versionText;
        
        private IAudioService _audioService;
        private ISaveService _saveService;
        private ICloudSaveService _cloudService;

        private void OnEnable()
        {
            _audioService = ServiceLocator.Get<IAudioService>();
            _saveService = ServiceLocator.Get<ISaveService>();
            _cloudService = ServiceLocator.Get<ICloudSaveService>();
            
            LoadSettings();
            SetupListeners();
            UpdateAccountUI();
        }

        private void LoadSettings()
        {
            var playerData = _saveService?.GetPlayerData();
            if (playerData?.Settings == null) return;
            
            var settings = playerData.Settings;
            
            if (musicSlider != null)
                musicSlider.value = settings.MusicVolume;
            
            if (sfxSlider != null)
                sfxSlider.value = settings.SfxVolume;
            
            if (musicToggle != null)
                musicToggle.isOn = settings.MusicEnabled;
            
            if (sfxToggle != null)
                sfxToggle.isOn = settings.SfxEnabled;
            
            if (vibrationToggle != null)
                vibrationToggle.isOn = settings.VibrationEnabled;
            
            if (notificationsToggle != null)
                notificationsToggle.isOn = settings.NotificationsEnabled;
            
            if (qualityDropdown != null)
                qualityDropdown.value = settings.GraphicsQuality;
            
            if (versionText != null)
                versionText.text = $"Version {Application.version}";
        }

        private void SetupListeners()
        {
            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
            if (musicToggle != null)
                musicToggle.onValueChanged.AddListener(OnMusicToggled);
            
            if (sfxToggle != null)
                sfxToggle.onValueChanged.AddListener(OnSfxToggled);
            
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
            
            if (notificationsToggle != null)
                notificationsToggle.onValueChanged.AddListener(OnNotificationsToggled);
            
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            if (signInButton != null)
                signInButton.onClick.AddListener(OnSignInClicked);
            
            if (signOutButton != null)
                signOutButton.onClick.AddListener(OnSignOutClicked);
            
            if (restorePurchasesButton != null)
                restorePurchasesButton.onClick.AddListener(OnRestorePurchasesClicked);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_audioService != null)
                _audioService.MusicVolume = value;
            
            SaveSettings();
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_audioService != null)
                _audioService.SfxVolume = value;
            
            SaveSettings();
        }

        private void OnMusicToggled(bool enabled)
        {
            if (_audioService != null)
                _audioService.IsMusicMuted = !enabled;
            
            SaveSettings();
        }

        private void OnSfxToggled(bool enabled)
        {
            if (_audioService != null)
                _audioService.IsSfxMuted = !enabled;
            
            SaveSettings();
        }

        private void OnVibrationToggled(bool enabled)
        {
            var playerData = _saveService?.GetPlayerData();
            if (playerData?.Settings != null)
            {
                playerData.Settings.VibrationEnabled = enabled;
            }
            SaveSettings();
        }

        private void OnNotificationsToggled(bool enabled)
        {
            var playerData = _saveService?.GetPlayerData();
            if (playerData?.Settings != null)
            {
                playerData.Settings.NotificationsEnabled = enabled;
            }
            SaveSettings();
        }

        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
            
            var playerData = _saveService?.GetPlayerData();
            if (playerData?.Settings != null)
            {
                playerData.Settings.GraphicsQuality = index;
            }
            SaveSettings();
        }

        private void OnCloseClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
            gameObject.SetActive(false);
        }

        private void OnSignInClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
            _cloudService?.SignIn(success => {
                UpdateAccountUI();
            });
        }

        private void OnSignOutClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
            _cloudService?.SignOut();
            UpdateAccountUI();
        }

        private void OnRestorePurchasesClicked()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
            ServiceLocator.Get<ISubscriptionService>()?.RestorePurchases();
        }

        private void UpdateAccountUI()
        {
            bool isAuthenticated = _cloudService?.IsAuthenticated ?? false;
            
            if (signInButton != null)
                signInButton.gameObject.SetActive(!isAuthenticated);
            
            if (signOutButton != null)
                signOutButton.gameObject.SetActive(isAuthenticated);
            
            if (accountStatusText != null)
            {
                var playerData = _saveService?.GetPlayerData();
                accountStatusText.text = isAuthenticated ? 
                    $"Signed in as {playerData?.PlayerName ?? "Player"}" : 
                    "Not signed in";
            }
        }

        private void SaveSettings()
        {
            _saveService?.SaveGame();
        }
    }
}
