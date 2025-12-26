using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Gameplay;

namespace Rumbax.UI
{
    /// <summary>
    /// Victory panel shown when player completes all waves.
    /// </summary>
    public class VictoryPanel : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI levelCompleteText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        [SerializeField] private TextMeshProUGUI gemsEarnedText;
        
        [Header("Stars")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;
        
        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button tripleCoinsButton;
        
        [Header("Animation")]
        [SerializeField] private float starAnimationDelay = 0.3f;
        
        private LevelController _levelController;
        private IAdService _adService;
        private int _starsEarned;
        private bool _bonusClaimed;

        private void OnEnable()
        {
            _levelController = FindObjectOfType<LevelController>();
            _adService = ServiceLocator.Get<IAdService>();
            
            SetupButtons();
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            
            if (replayButton != null)
                replayButton.onClick.AddListener(OnReplayClicked);
            
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);
            
            if (tripleCoinsButton != null)
                tripleCoinsButton.onClick.AddListener(OnTripleCoinsClicked);
        }

        private void SubscribeToEvents()
        {
            ServiceLocator.Get<IEventBus>()?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void UnsubscribeFromEvents()
        {
            ServiceLocator.Get<IEventBus>()?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            _starsEarned = evt.Stars;
            _bonusClaimed = false;
            
            UpdateDisplay(evt);
            AnimateStars(evt.Stars);
        }

        private void UpdateDisplay(LevelCompletedEvent evt)
        {
            if (levelCompleteText != null)
                levelCompleteText.text = $"Level {evt.LevelNumber} Complete!";
            
            if (scoreText != null)
                scoreText.text = evt.Score.ToString();
            
            if (coinsEarnedText != null)
                coinsEarnedText.text = evt.CoinsEarned.ToString();
            
            // Update gems display
            var gameManager = GameManager.Instance;
            int gemsEarned = gameManager?.Config?.BaseWinGems ?? 1;
            
            if (gemsEarnedText != null)
                gemsEarnedText.text = gemsEarned.ToString();
            
            // Update triple coins button
            if (tripleCoinsButton != null)
                tripleCoinsButton.interactable = _adService?.IsRewardedAdReady == true;
        }

        private void AnimateStars(int stars)
        {
            if (starImages == null) return;
            
            // Reset all stars
            foreach (var star in starImages)
            {
                if (star != null && starEmptySprite != null)
                    star.sprite = starEmptySprite;
            }
            
            // Animate earned stars
            StartCoroutine(AnimateStarsCoroutine(stars));
        }

        private System.Collections.IEnumerator AnimateStarsCoroutine(int stars)
        {
            for (int i = 0; i < stars && i < starImages.Length; i++)
            {
                yield return new WaitForSecondsRealtime(starAnimationDelay);
                
                if (starImages[i] != null && starFilledSprite != null)
                {
                    starImages[i].sprite = starFilledSprite;
                    
                    // Scale animation
                    starImages[i].transform.localScale = Vector3.one * 1.3f;
                    LeanTweenHelper.Scale(starImages[i].gameObject, Vector3.one, 0.2f);
                    
                    ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("star_earned");
                }
            }
        }

        private void OnNextLevelClicked()
        {
            PlayButtonSound();
            
            // Show interstitial between levels
            if (_adService?.IsInterstitialReady == true && !_adService.AdsRemoved)
            {
                _adService.ShowInterstitial(() => {
                    LoadNextLevel();
                });
            }
            else
            {
                LoadNextLevel();
            }
        }

        private void LoadNextLevel()
        {
            // Load next level scene
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                playerData.CurrentLevel++;
                saveService.SaveGame();
            }
            
            GameManager.Instance?.LoadScene("Game");
        }

        private void OnReplayClicked()
        {
            PlayButtonSound();
            _levelController?.RestartLevel();
            gameObject.SetActive(false);
        }

        private void OnMenuClicked()
        {
            PlayButtonSound();
            _levelController?.ReturnToMenu();
        }

        private void OnTripleCoinsClicked()
        {
            if (_bonusClaimed) return;
            
            PlayButtonSound();
            
            if (_adService?.IsRewardedAdReady == true)
            {
                _adService.ShowRewardedAd(
                    onReward: () => {
                        _bonusClaimed = true;
                        
                        // Give 2x more coins (total 3x)
                        int bonusCoins = (_levelController?.CoinsEarned ?? 0) * 2;
                        ServiceLocator.Get<ICurrencyService>()?.AddCoins(bonusCoins);
                        
                        if (coinsEarnedText != null)
                        {
                            int total = (_levelController?.CoinsEarned ?? 0) * 3;
                            coinsEarnedText.text = total.ToString();
                        }
                        
                        if (tripleCoinsButton != null)
                            tripleCoinsButton.interactable = false;
                        
                        ServiceLocator.Get<IAnalyticsService>()?.LogAdWatched("rewarded", "triple_coins");
                    }
                );
            }
        }

        private void PlayButtonSound()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }
    }

    /// <summary>
    /// Helper class for simple tweening without external dependencies.
    /// </summary>
    public static class LeanTweenHelper
    {
        public static void Scale(GameObject obj, Vector3 target, float duration)
        {
            if (obj == null) return;
            
            var routine = obj.GetComponent<MonoBehaviour>();
            if (routine != null)
            {
                routine.StartCoroutine(ScaleCoroutine(obj.transform, target, duration));
            }
        }

        private static System.Collections.IEnumerator ScaleCoroutine(Transform t, Vector3 target, float duration)
        {
            Vector3 start = t.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                t.localScale = Vector3.Lerp(start, target, progress);
                yield return null;
            }
            
            t.localScale = target;
        }
    }
}
