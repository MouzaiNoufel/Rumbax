using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services;

namespace Rumbax.UI
{
    /// <summary>
    /// Main menu screen controller.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button dailyRewardButton;
        
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image avatarImage;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject dailyRewardPanel;
        
        [Header("Daily Reward Indicator")]
        [SerializeField] private GameObject dailyRewardNotification;
        
        private ICurrencyService _currencyService;

        private void Start()
        {
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            
            SetupButtons();
            UpdateUI();
            CheckDailyReward();
            
            // Play menu music
            ServiceLocator.Get<IAudioService>()?.PlayMusic("menu_music");
        }

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (shopButton != null)
                shopButton.onClick.AddListener(OnShopClicked);
            
            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            
            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(OnAchievementsClicked);
            
            if (dailyRewardButton != null)
                dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
        }

        private void UpdateUI()
        {
            // Update currencies
            if (coinsText != null && _currencyService != null)
                coinsText.text = FormatNumber(_currencyService.Coins);
            
            if (gemsText != null && _currencyService != null)
                gemsText.text = _currencyService.Gems.ToString();
            
            // Update player info
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                if (playerNameText != null)
                    playerNameText.text = playerData.PlayerName ?? "Player";
                
                if (levelText != null)
                    levelText.text = $"Level {playerData.HighestLevel}";
            }
        }

        private void CheckDailyReward()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null && dailyRewardNotification != null)
            {
                var lastClaim = playerData.LastDailyRewardTime;
                var now = System.DateTime.UtcNow;
                bool canClaim = (now - lastClaim).TotalHours >= 24;
                
                dailyRewardNotification.SetActive(canClaim);
            }
        }

        private void OnPlayClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.LoadScene("Game");
        }

        private void OnSettingsClicked()
        {
            PlayButtonSound();
            settingsPanel?.SetActive(true);
        }

        private void OnShopClicked()
        {
            PlayButtonSound();
            shopPanel?.SetActive(true);
        }

        private void OnLeaderboardClicked()
        {
            PlayButtonSound();
            ServiceLocator.Get<ILeaderboardService>()?.ShowLeaderboardUI();
        }

        private void OnAchievementsClicked()
        {
            PlayButtonSound();
            ServiceLocator.Get<IAchievementService>()?.ShowAchievementsUI();
        }

        private void OnDailyRewardClicked()
        {
            PlayButtonSound();
            dailyRewardPanel?.SetActive(true);
        }

        private void PlayButtonSound()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }

        private string FormatNumber(long number)
        {
            if (number >= 1000000000)
                return (number / 1000000000f).ToString("0.##") + "B";
            if (number >= 1000000)
                return (number / 1000000f).ToString("0.##") + "M";
            if (number >= 1000)
                return (number / 1000f).ToString("0.#") + "K";
            return number.ToString();
        }
    }
}
