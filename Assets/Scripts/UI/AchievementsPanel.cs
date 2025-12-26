using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Events;
using Rumbax.Core.Services.Interfaces;
using Rumbax.Systems;

namespace Rumbax.UI
{
    /// <summary>
    /// UI panel for displaying achievements.
    /// </summary>
    public class AchievementsPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _unclaimedBadge;

        [Header("Category Tabs")]
        [SerializeField] private Button _allTab;
        [SerializeField] private Button _combatTab;
        [SerializeField] private Button _progressionTab;
        [SerializeField] private Button _economyTab;
        [SerializeField] private Button _collectionTab;
        [SerializeField] private Button _specialTab;

        [Header("Achievement List")]
        [SerializeField] private Transform _achievementContainer;
        [SerializeField] private GameObject _achievementItemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        private IAchievementService _achievementService;
        private AchievementCategory? _currentCategory = null;
        private List<AchievementItemUI> _achievementItems = new List<AchievementItemUI>();

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            _achievementService = ServiceLocator.Get<IAchievementService>();
            
            SubscribeToEvents();
            Hide();
        }

        private void SetupButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_allTab != null)
            {
                _allTab.onClick.AddListener(() => SelectCategory(null));
            }

            if (_combatTab != null)
            {
                _combatTab.onClick.AddListener(() => SelectCategory(AchievementCategory.Combat));
            }

            if (_progressionTab != null)
            {
                _progressionTab.onClick.AddListener(() => SelectCategory(AchievementCategory.Progression));
            }

            if (_economyTab != null)
            {
                _economyTab.onClick.AddListener(() => SelectCategory(AchievementCategory.Economy));
            }

            if (_collectionTab != null)
            {
                _collectionTab.onClick.AddListener(() => SelectCategory(AchievementCategory.Collection));
            }

            if (_specialTab != null)
            {
                _specialTab.onClick.AddListener(() => SelectCategory(AchievementCategory.Special));
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Subscribe<AchievementRewardClaimedEvent>(OnRewardClaimed);
            EventBus.Subscribe<ShowAchievementsUIEvent>(OnShowUI);
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

        private void SelectCategory(AchievementCategory? category)
        {
            _currentCategory = category;
            UpdateTabVisuals();
            RefreshAchievementList();
        }

        private void UpdateTabVisuals()
        {
            // Reset all tabs
            SetTabSelected(_allTab, _currentCategory == null);
            SetTabSelected(_combatTab, _currentCategory == AchievementCategory.Combat);
            SetTabSelected(_progressionTab, _currentCategory == AchievementCategory.Progression);
            SetTabSelected(_economyTab, _currentCategory == AchievementCategory.Economy);
            SetTabSelected(_collectionTab, _currentCategory == AchievementCategory.Collection);
            SetTabSelected(_specialTab, _currentCategory == AchievementCategory.Special);
        }

        private void SetTabSelected(Button tab, bool selected)
        {
            if (tab == null) return;

            // Change tab appearance based on selection
            ColorBlock colors = tab.colors;
            colors.normalColor = selected ? new Color(0.3f, 0.6f, 1f) : Color.white;
            tab.colors = colors;
        }

        private void RefreshDisplay()
        {
            UpdateHeader();
            UpdateTabVisuals();
            RefreshAchievementList();
        }

        private void UpdateHeader()
        {
            if (_achievementService == null) return;

            int unlocked = _achievementService.GetUnlockedCount();
            int total = _achievementService.GetTotalCount();
            int unclaimed = _achievementService.GetUnclaimedCount();

            if (_progressText != null)
            {
                _progressText.text = $"{unlocked} / {total} Achievements";
            }

            if (_progressSlider != null)
            {
                _progressSlider.value = total > 0 ? (float)unlocked / total : 0;
            }

            if (_unclaimedBadge != null)
            {
                _unclaimedBadge.gameObject.SetActive(unclaimed > 0);
                _unclaimedBadge.text = unclaimed.ToString();
            }
        }

        private void RefreshAchievementList()
        {
            if (_achievementContainer == null || _achievementService == null) return;

            // Clear existing items
            foreach (var item in _achievementItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _achievementItems.Clear();

            // Get achievements
            List<Achievement> achievements;
            if (_currentCategory.HasValue)
            {
                achievements = _achievementService.GetAchievementsByCategory(_currentCategory.Value);
            }
            else
            {
                achievements = _achievementService.GetAllAchievements();
            }

            // Sort: unclaimed first, then unlocked, then by progress
            achievements.Sort((a, b) =>
            {
                // Unclaimed rewards first
                if (a.IsUnlocked && !a.IsRewardClaimed && (!b.IsUnlocked || b.IsRewardClaimed)) return -1;
                if (b.IsUnlocked && !b.IsRewardClaimed && (!a.IsUnlocked || a.IsRewardClaimed)) return 1;

                // Then unlocked
                if (a.IsUnlocked && !b.IsUnlocked) return -1;
                if (b.IsUnlocked && !a.IsUnlocked) return 1;

                // Then by progress percentage
                float progressA = a.TargetValue > 0 ? (float)a.CurrentProgress / a.TargetValue : 0;
                float progressB = b.TargetValue > 0 ? (float)b.CurrentProgress / b.TargetValue : 0;
                return progressB.CompareTo(progressA);
            });

            // Create items
            foreach (var achievement in achievements)
            {
                // Skip hidden achievements that are not unlocked
                if (achievement.IsHidden && !achievement.IsUnlocked) continue;

                if (_achievementItemPrefab != null)
                {
                    GameObject itemObj = Instantiate(_achievementItemPrefab, _achievementContainer);
                    AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();

                    if (itemUI != null)
                    {
                        itemUI.Setup(achievement, OnClaimReward);
                        _achievementItems.Add(itemUI);
                    }
                }
            }

            // Scroll to top
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void OnClaimReward(string achievementId)
        {
            if (_achievementService != null)
            {
                _achievementService.ClaimReward(achievementId);
            }
        }

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnRewardClaimed(AchievementRewardClaimedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnShowUI(ShowAchievementsUIEvent evt)
        {
            Show();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Unsubscribe<AchievementRewardClaimedEvent>(OnRewardClaimed);
            EventBus.Unsubscribe<ShowAchievementsUIEvent>(OnShowUI);
        }
    }

    /// <summary>
    /// UI component for a single achievement item.
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _coinRewardText;
        [SerializeField] private TextMeshProUGUI _gemRewardText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _unlockedGlow;
        [SerializeField] private GameObject _claimedCheck;
        [SerializeField] private Image _categoryIcon;

        private Achievement _achievement;
        private Action<string> _onClaimCallback;

        public void Setup(Achievement achievement, Action<string> onClaimCallback)
        {
            _achievement = achievement;
            _onClaimCallback = onClaimCallback;

            // Basic info
            if (_titleText != null)
            {
                _titleText.text = achievement.Title;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = achievement.Description;
            }

            // Progress
            bool showProgress = !achievement.IsUnlocked && achievement.TargetValue > 1;
            
            if (_progressText != null)
            {
                _progressText.gameObject.SetActive(showProgress);
                _progressText.text = $"{achievement.CurrentProgress}/{achievement.TargetValue}";
            }

            if (_progressSlider != null)
            {
                _progressSlider.gameObject.SetActive(showProgress);
                _progressSlider.value = (float)achievement.CurrentProgress / achievement.TargetValue;
            }

            // Rewards
            if (_coinRewardText != null)
            {
                _coinRewardText.text = achievement.CoinReward.ToString();
            }

            if (_gemRewardText != null)
            {
                _gemRewardText.text = achievement.GemReward.ToString();
            }

            // Icon
            if (_iconImage != null && achievement.Icon != null)
            {
                _iconImage.sprite = achievement.Icon;
            }

            // States
            bool canClaim = achievement.IsUnlocked && !achievement.IsRewardClaimed;

            if (_claimButton != null)
            {
                _claimButton.onClick.RemoveAllListeners();
                _claimButton.onClick.AddListener(OnClaimClicked);
                _claimButton.gameObject.SetActive(canClaim);
            }

            if (_lockedOverlay != null)
            {
                _lockedOverlay.SetActive(!achievement.IsUnlocked);
            }

            if (_unlockedGlow != null)
            {
                _unlockedGlow.SetActive(canClaim);
            }

            if (_claimedCheck != null)
            {
                _claimedCheck.SetActive(achievement.IsRewardClaimed);
            }

            // Category icon color
            if (_categoryIcon != null)
            {
                _categoryIcon.color = GetCategoryColor(achievement.Category);
            }
        }

        private Color GetCategoryColor(AchievementCategory category)
        {
            switch (category)
            {
                case AchievementCategory.Combat:
                    return new Color(1f, 0.3f, 0.3f);
                case AchievementCategory.Progression:
                    return new Color(0.3f, 0.7f, 1f);
                case AchievementCategory.Collection:
                    return new Color(1f, 0.8f, 0.2f);
                case AchievementCategory.Economy:
                    return new Color(0.3f, 1f, 0.3f);
                case AchievementCategory.Social:
                    return new Color(1f, 0.5f, 0.8f);
                case AchievementCategory.Special:
                    return new Color(0.8f, 0.5f, 1f);
                default:
                    return Color.white;
            }
        }

        private void OnClaimClicked()
        {
            _onClaimCallback?.Invoke(_achievement.Id);
        }
    }
}
