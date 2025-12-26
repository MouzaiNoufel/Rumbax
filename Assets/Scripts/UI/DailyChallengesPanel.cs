using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Events;

namespace Rumbax.UI
{
    /// <summary>
    /// UI panel for displaying daily challenges.
    /// </summary>
    public class DailyChallengesPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;

        [Header("Streak Display")]
        [SerializeField] private TextMeshProUGUI _streakText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image _streakFireIcon;

        [Header("Challenge List")]
        [SerializeField] private Transform _challengeContainer;
        [SerializeField] private GameObject _challengeItemPrefab;

        [Header("Bonus Section")]
        [SerializeField] private GameObject _bonusSection;
        [SerializeField] private TextMeshProUGUI _bonusText;
        [SerializeField] private Slider _bonusProgress;

        private Systems.DailyChallengeService _challengeService;
        private List<ChallengeItemUI> _challengeItems = new List<ChallengeItemUI>();

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }
        }

        private void Start()
        {
            _challengeService = FindObjectOfType<Systems.DailyChallengeService>();
            
            SubscribeToEvents();
            Hide();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<Systems.DailyChallengesRefreshedEvent>(OnChallengesRefreshed);
            EventBus.Subscribe<Systems.ChallengeCompletedEvent>(OnChallengeCompleted);
            EventBus.Subscribe<Systems.ChallengeRewardClaimedEvent>(OnRewardClaimed);
        }

        private void Update()
        {
            if (_panelRoot != null && _panelRoot.activeSelf)
            {
                UpdateTimer();
            }
        }

        public void Show()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            RefreshDisplay();
        }

        public void Hide()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (_panelRoot != null && _panelRoot.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void RefreshDisplay()
        {
            if (_challengeService == null) return;

            UpdateStreak();
            UpdateChallengeList();
            UpdateBonusProgress();
        }

        private void UpdateStreak()
        {
            int streak = _challengeService.GetCurrentStreak();

            if (_streakText != null)
            {
                _streakText.text = streak.ToString();
            }

            if (_streakFireIcon != null)
            {
                _streakFireIcon.gameObject.SetActive(streak > 0);
                
                // Animate fire icon based on streak
                float scale = 1f + Mathf.Min(streak * 0.05f, 0.5f);
                _streakFireIcon.transform.localScale = Vector3.one * scale;
            }
        }

        private void UpdateTimer()
        {
            if (_timerText == null || _challengeService == null) return;

            TimeSpan remaining = _challengeService.GetTimeUntilRefresh();
            _timerText.text = $"New challenges in: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        private void UpdateChallengeList()
        {
            if (_challengeContainer == null || _challengeService == null) return;

            // Clear existing items
            foreach (var item in _challengeItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _challengeItems.Clear();

            // Create new items
            List<Systems.Challenge> challenges = _challengeService.GetDailyChallenges();

            foreach (var challenge in challenges)
            {
                if (_challengeItemPrefab != null)
                {
                    GameObject itemObj = Instantiate(_challengeItemPrefab, _challengeContainer);
                    ChallengeItemUI itemUI = itemObj.GetComponent<ChallengeItemUI>();

                    if (itemUI != null)
                    {
                        itemUI.Setup(challenge, OnClaimReward);
                        _challengeItems.Add(itemUI);
                    }
                }
            }
        }

        private void UpdateBonusProgress()
        {
            if (_challengeService == null) return;

            List<Systems.Challenge> challenges = _challengeService.GetDailyChallenges();
            
            int completed = 0;
            int total = challenges.Count;

            foreach (var challenge in challenges)
            {
                if (challenge.IsCompleted) completed++;
            }

            if (_bonusProgress != null)
            {
                _bonusProgress.value = total > 0 ? (float)completed / total : 0;
            }

            if (_bonusText != null)
            {
                _bonusText.text = $"Complete all ({completed}/{total})";
            }

            if (_bonusSection != null)
            {
                _bonusSection.SetActive(completed < total);
            }
        }

        private void OnClaimReward(string challengeId)
        {
            if (_challengeService != null)
            {
                _challengeService.ClaimReward(challengeId);
            }
        }

        private void OnChallengesRefreshed(Systems.DailyChallengesRefreshedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnChallengeCompleted(Systems.ChallengeCompletedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnRewardClaimed(Systems.ChallengeRewardClaimedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<Systems.DailyChallengesRefreshedEvent>(OnChallengesRefreshed);
            EventBus.Unsubscribe<Systems.ChallengeCompletedEvent>(OnChallengeCompleted);
            EventBus.Unsubscribe<Systems.ChallengeRewardClaimedEvent>(OnRewardClaimed);
        }
    }

    /// <summary>
    /// UI component for a single challenge item.
    /// </summary>
    public class ChallengeItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _coinRewardText;
        [SerializeField] private TextMeshProUGUI _gemRewardText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _completedOverlay;
        [SerializeField] private GameObject _claimedCheck;

        private Systems.Challenge _challenge;
        private Action<string> _onClaimCallback;

        public void Setup(Systems.Challenge challenge, Action<string> onClaimCallback)
        {
            _challenge = challenge;
            _onClaimCallback = onClaimCallback;

            if (_titleText != null)
            {
                _titleText.text = challenge.Title;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = challenge.Description;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{challenge.CurrentProgress}/{challenge.TargetValue}";
            }

            if (_progressSlider != null)
            {
                _progressSlider.value = (float)challenge.CurrentProgress / challenge.TargetValue;
            }

            if (_coinRewardText != null)
            {
                _coinRewardText.text = challenge.CoinReward.ToString();
            }

            if (_gemRewardText != null)
            {
                _gemRewardText.text = challenge.GemReward.ToString();
            }

            // Update button state
            if (_claimButton != null)
            {
                _claimButton.onClick.RemoveAllListeners();
                _claimButton.onClick.AddListener(OnClaimClicked);
                _claimButton.interactable = challenge.IsCompleted && !challenge.IsRewardClaimed;
            }

            if (_completedOverlay != null)
            {
                _completedOverlay.SetActive(challenge.IsRewardClaimed);
            }

            if (_claimedCheck != null)
            {
                _claimedCheck.SetActive(challenge.IsRewardClaimed);
            }
        }

        private void OnClaimClicked()
        {
            _onClaimCallback?.Invoke(_challenge.Id);
        }
    }
}
