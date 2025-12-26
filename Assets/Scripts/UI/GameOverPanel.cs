using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Gameplay;

namespace Rumbax.UI
{
    /// <summary>
    /// Game over panel shown when player loses.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI waveReachedText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        
        [Header("Buttons")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button doubleCoinsButton;
        
        [Header("Revive")]
        [SerializeField] private GameObject reviveContainer;
        [SerializeField] private TextMeshProUGUI reviveCountdownText;
        
        private LevelController _levelController;
        private IAdService _adService;
        private float _reviveCountdown = 5f;
        private bool _reviveUsed;
        private bool _doubleCoinsUsed;

        private void OnEnable()
        {
            _levelController = FindObjectOfType<LevelController>();
            _adService = ServiceLocator.Get<IAdService>();
            
            _reviveCountdown = 5f;
            _reviveUsed = false;
            
            SetupButtons();
            UpdateDisplay();
            UpdateReviveButton();
        }

        private void Update()
        {
            if (!_reviveUsed && reviveContainer.activeSelf)
            {
                _reviveCountdown -= Time.unscaledDeltaTime;
                
                if (reviveCountdownText != null)
                    reviveCountdownText.text = Mathf.CeilToInt(_reviveCountdown).ToString();
                
                if (_reviveCountdown <= 0)
                {
                    reviveContainer.SetActive(false);
                }
            }
        }

        private void SetupButtons()
        {
            if (reviveButton != null)
                reviveButton.onClick.AddListener(OnReviveClicked);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);
            
            if (doubleCoinsButton != null)
                doubleCoinsButton.onClick.AddListener(OnDoubleCoinsClicked);
        }

        private void UpdateDisplay()
        {
            var waveManager = Gameplay.Enemies.WaveManager.Instance;
            
            if (waveReachedText != null && waveManager != null)
                waveReachedText.text = $"Wave {waveManager.CurrentWave}";
            
            if (scoreText != null && _levelController != null)
                scoreText.text = _levelController.Score.ToString();
            
            if (coinsEarnedText != null && _levelController != null)
                coinsEarnedText.text = _levelController.CoinsEarned.ToString();
        }

        private void UpdateReviveButton()
        {
            bool canRevive = _adService?.IsRewardedAdReady == true && !_reviveUsed;
            
            if (reviveContainer != null)
                reviveContainer.SetActive(canRevive);
            
            if (reviveButton != null)
                reviveButton.interactable = canRevive;
        }

        private void OnReviveClicked()
        {
            PlayButtonSound();
            
            if (_adService?.IsRewardedAdReady == true)
            {
                _adService.ShowRewardedAd(
                    onReward: () => {
                        _reviveUsed = true;
                        _levelController?.RequestRevive();
                        gameObject.SetActive(false);
                        ServiceLocator.Get<IAnalyticsService>()?.LogAdWatched("rewarded", "revive");
                    },
                    onFailed: () => {
                        Debug.Log("[GameOverPanel] Revive ad failed");
                    }
                );
            }
        }

        private void OnRestartClicked()
        {
            PlayButtonSound();
            
            // Show interstitial ad before restart
            if (_adService?.IsInterstitialReady == true && !_adService.AdsRemoved)
            {
                _adService.ShowInterstitial(() => {
                    _levelController?.RestartLevel();
                    gameObject.SetActive(false);
                });
            }
            else
            {
                _levelController?.RestartLevel();
                gameObject.SetActive(false);
            }
        }

        private void OnMenuClicked()
        {
            PlayButtonSound();
            _levelController?.ReturnToMenu();
        }

        private void OnDoubleCoinsClicked()
        {
            if (_doubleCoinsUsed) return;
            
            PlayButtonSound();
            
            if (_adService?.IsRewardedAdReady == true)
            {
                _adService.ShowRewardedAd(
                    onReward: () => {
                        _doubleCoinsUsed = true;
                        int bonusCoins = _levelController?.CoinsEarned ?? 0;
                        ServiceLocator.Get<ICurrencyService>()?.AddCoins(bonusCoins);
                        
                        if (coinsEarnedText != null)
                            coinsEarnedText.text = (bonusCoins * 2).ToString();
                        
                        if (doubleCoinsButton != null)
                            doubleCoinsButton.interactable = false;
                        
                        ServiceLocator.Get<IAnalyticsService>()?.LogAdWatched("rewarded", "double_coins");
                    }
                );
            }
        }

        private void PlayButtonSound()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }
    }
}
