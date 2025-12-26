using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Gameplay;

namespace Rumbax.UI
{
    /// <summary>
    /// Pause menu panel with resume, restart, and quit options.
    /// </summary>
    public class PausePanel : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsText;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        
        private LevelController _levelController;

        private void OnEnable()
        {
            _levelController = FindObjectOfType<LevelController>();
            SetupButtons();
            UpdateStats();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void UpdateStats()
        {
            var waveManager = Gameplay.Enemies.WaveManager.Instance;
            
            if (waveText != null && waveManager != null)
                waveText.text = $"Wave: {waveManager.CurrentWave}/{waveManager.TotalWaves}";
            
            if (scoreText != null && _levelController != null)
                scoreText.text = $"Score: {_levelController.Score}";
            
            if (coinsText != null && _levelController != null)
                coinsText.text = $"Coins: {_levelController.CoinsEarned}";
        }

        private void OnResumeClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.ResumeGame();
            gameObject.SetActive(false);
        }

        private void OnRestartClicked()
        {
            PlayButtonSound();
            GameManager.Instance?.ResumeGame();
            _levelController?.RestartLevel();
            gameObject.SetActive(false);
        }

        private void OnSettingsClicked()
        {
            PlayButtonSound();
            settingsPanel?.SetActive(true);
        }

        private void OnQuitClicked()
        {
            PlayButtonSound();
            _levelController?.ReturnToMenu();
        }

        private void PlayButtonSound()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }
    }
}
