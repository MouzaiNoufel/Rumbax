using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Gameplay;
using Rumbax.Gameplay.Enemies;
using Rumbax.Gameplay.Defenders;

namespace Rumbax.UI
{
    /// <summary>
    /// Main game HUD displaying currencies, wave info, and player health.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        
        [Header("Wave Info")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private Slider waveProgressSlider;
        
        [Header("Player Health")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("Spawn Button")]
        [SerializeField] private Button spawnButton;
        [SerializeField] private TextMeshProUGUI spawnCostText;
        
        [Header("Control Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button fastForwardButton;
        
        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Colors")]
        [SerializeField] private Color healthHighColor = Color.green;
        [SerializeField] private Color healthMediumColor = Color.yellow;
        [SerializeField] private Color healthLowColor = Color.red;
        
        private ICurrencyService _currencyService;
        private IEventBus _eventBus;
        private LevelController _levelController;
        private DefenderSpawner _defenderSpawner;
        private WaveManager _waveManager;
        private bool _isFastForward;

        private void Start()
        {
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _levelController = FindObjectOfType<LevelController>();
            _defenderSpawner = FindObjectOfType<DefenderSpawner>();
            _waveManager = WaveManager.Instance;
            
            SetupButtons();
            SubscribeToEvents();
            UpdateAllUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (spawnButton != null)
                spawnButton.onClick.AddListener(OnSpawnButtonClicked);
            
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            
            if (fastForwardButton != null)
                fastForwardButton.onClick.AddListener(OnFastForwardClicked);
        }

        private void SubscribeToEvents()
        {
            if (_currencyService != null)
            {
                _currencyService.OnCoinsChanged += UpdateCoinsDisplay;
                _currencyService.OnGemsChanged += UpdateGemsDisplay;
            }
            
            if (_eventBus != null)
            {
                _eventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
                _eventBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
                _eventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            }
            
            if (_waveManager != null)
            {
                _waveManager.OnPlayerHealthChanged += UpdateHealthDisplay;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_currencyService != null)
            {
                _currencyService.OnCoinsChanged -= UpdateCoinsDisplay;
                _currencyService.OnGemsChanged -= UpdateGemsDisplay;
            }
            
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
                _eventBus.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
                _eventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            }
            
            if (_waveManager != null)
            {
                _waveManager.OnPlayerHealthChanged -= UpdateHealthDisplay;
            }
        }

        private void Update()
        {
            UpdateSpawnButton();
            UpdateScore();
        }

        private void UpdateAllUI()
        {
            UpdateCoinsDisplay(_currencyService?.Coins ?? 0);
            UpdateGemsDisplay(_currencyService?.Gems ?? 0);
            UpdateHealthDisplay(_waveManager?.PlayerHealth ?? 100f);
            UpdateWaveDisplay();
            UpdateSpawnButton();
        }

        private void UpdateCoinsDisplay(long coins)
        {
            if (coinsText != null)
            {
                coinsText.text = FormatNumber(coins);
            }
        }

        private void UpdateGemsDisplay(int gems)
        {
            if (gemsText != null)
            {
                gemsText.text = gems.ToString();
            }
        }

        private void UpdateHealthDisplay(float health)
        {
            if (_waveManager == null) return;
            
            float percent = health / _waveManager.MaxPlayerHealth;
            
            if (healthSlider != null)
            {
                healthSlider.value = percent;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(health)}/{Mathf.CeilToInt(_waveManager.MaxPlayerHealth)}";
            }
            
            if (healthFillImage != null)
            {
                if (percent > 0.5f)
                    healthFillImage.color = healthHighColor;
                else if (percent > 0.25f)
                    healthFillImage.color = healthMediumColor;
                else
                    healthFillImage.color = healthLowColor;
            }
        }

        private void UpdateWaveDisplay()
        {
            if (_waveManager == null) return;
            
            if (waveText != null)
            {
                waveText.text = $"Wave {_waveManager.CurrentWave}/{_waveManager.TotalWaves}";
            }
            
            if (waveProgressSlider != null)
            {
                waveProgressSlider.value = (float)_waveManager.CurrentWave / _waveManager.TotalWaves;
            }
        }

        private void UpdateSpawnButton()
        {
            if (_defenderSpawner == null || spawnButton == null) return;
            
            int cost = _defenderSpawner.CurrentSpawnCost;
            bool canAfford = _currencyService?.CanAffordCoins(cost) ?? false;
            bool hasSpace = _defenderSpawner.CanSpawn;
            
            spawnButton.interactable = canAfford && hasSpace;
            
            if (spawnCostText != null)
            {
                spawnCostText.text = FormatNumber(cost);
                spawnCostText.color = canAfford ? Color.white : Color.red;
            }
        }

        private void UpdateScore()
        {
            if (scoreText != null && _levelController != null)
            {
                scoreText.text = FormatNumber(_levelController.Score);
            }
        }

        private void OnWaveStarted(WaveStartedEvent evt)
        {
            UpdateWaveDisplay();
        }

        private void OnWaveCompleted(WaveCompletedEvent evt)
        {
            UpdateWaveDisplay();
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            // Could add visual feedback here
        }

        private void OnSpawnButtonClicked()
        {
            _levelController?.OnSpawnButtonPressed();
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }

        private void OnPauseButtonClicked()
        {
            _levelController?.PauseGame();
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }

        private void OnFastForwardClicked()
        {
            _isFastForward = !_isFastForward;
            Time.timeScale = _isFastForward ? 2f : 1f;
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }

        /// <summary>
        /// Format large numbers with K, M, B suffixes.
        /// </summary>
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
