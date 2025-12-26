using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Core;
using Rumbax.Core.Services.Interfaces;
using Rumbax.Systems;

namespace Rumbax.UI
{
    /// <summary>
    /// UI panel for displaying leaderboards.
    /// </summary>
    public class LeaderboardPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;

        [Header("Leaderboard Tabs")]
        [SerializeField] private Button _levelTab;
        [SerializeField] private Button _scoreTab;
        [SerializeField] private Button _killsTab;
        [SerializeField] private Button _waveTab;

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerRankText;
        [SerializeField] private TextMeshProUGUI _playerScoreText;
        [SerializeField] private Image _playerAvatar;

        [Header("Leaderboard List")]
        [SerializeField] private Transform _entryContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Loading")]
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private GameObject _noDataMessage;

        private ILeaderboardService _leaderboardService;
        private LeaderboardType _currentType = LeaderboardType.HighestLevel;
        private List<LeaderboardEntryUI> _entryItems = new List<LeaderboardEntryUI>();

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            _leaderboardService = ServiceLocator.Get<ILeaderboardService>();
            Hide();
        }

        private void SetupButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_levelTab != null)
            {
                _levelTab.onClick.AddListener(() => SelectLeaderboard(LeaderboardType.HighestLevel));
            }

            if (_scoreTab != null)
            {
                _scoreTab.onClick.AddListener(() => SelectLeaderboard(LeaderboardType.TotalScore));
            }

            if (_killsTab != null)
            {
                _killsTab.onClick.AddListener(() => SelectLeaderboard(LeaderboardType.TotalEnemiesKilled));
            }

            if (_waveTab != null)
            {
                _waveTab.onClick.AddListener(() => SelectLeaderboard(LeaderboardType.HighestWave));
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

        private void SelectLeaderboard(LeaderboardType type)
        {
            _currentType = type;
            UpdateTabVisuals();
            LoadLeaderboard();
        }

        private void UpdateTabVisuals()
        {
            SetTabSelected(_levelTab, _currentType == LeaderboardType.HighestLevel);
            SetTabSelected(_scoreTab, _currentType == LeaderboardType.TotalScore);
            SetTabSelected(_killsTab, _currentType == LeaderboardType.TotalEnemiesKilled);
            SetTabSelected(_waveTab, _currentType == LeaderboardType.HighestWave);
        }

        private void SetTabSelected(Button tab, bool selected)
        {
            if (tab == null) return;

            ColorBlock colors = tab.colors;
            colors.normalColor = selected ? new Color(0.3f, 0.6f, 1f) : Color.white;
            tab.colors = colors;
        }

        private void RefreshDisplay()
        {
            UpdateTabVisuals();
            UpdatePlayerInfo();
            LoadLeaderboard();
        }

        private void UpdatePlayerInfo()
        {
            if (_leaderboardService == null) return;

            if (_playerNameText != null)
            {
                _playerNameText.text = _leaderboardService.GetPlayerDisplayName();
            }

            // Get player rank for current leaderboard
            _leaderboardService.GetPlayerRank(_currentType, rank =>
            {
                if (_playerRankText != null)
                {
                    _playerRankText.text = rank > 0 ? $"#{rank}" : "Unranked";
                }
            });
        }

        private void LoadLeaderboard()
        {
            if (_leaderboardService == null) return;

            // Show loading
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(true);
            }

            if (_noDataMessage != null)
            {
                _noDataMessage.SetActive(false);
            }

            ClearEntries();

            _leaderboardService.LoadLeaderboard(_currentType, entries =>
            {
                if (_loadingIndicator != null)
                {
                    _loadingIndicator.SetActive(false);
                }

                if (entries == null || entries.Count == 0)
                {
                    if (_noDataMessage != null)
                    {
                        _noDataMessage.SetActive(true);
                    }
                    return;
                }

                PopulateLeaderboard(entries);
            });
        }

        private void ClearEntries()
        {
            foreach (var item in _entryItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _entryItems.Clear();
        }

        private void PopulateLeaderboard(List<LeaderboardEntry> entries)
        {
            if (_entryContainer == null) return;

            foreach (var entry in entries)
            {
                if (_entryPrefab != null)
                {
                    GameObject entryObj = Instantiate(_entryPrefab, _entryContainer);
                    LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();

                    if (entryUI != null)
                    {
                        bool isCurrentPlayer = entry.PlayerId == _leaderboardService.GetPlayerId();
                        entryUI.Setup(entry, isCurrentPlayer);
                        _entryItems.Add(entryUI);
                    }
                }
            }

            // Scroll to top
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void ShowGooglePlayLeaderboards()
        {
            if (_leaderboardService != null)
            {
                _leaderboardService.ShowLeaderboardUI();
            }
        }
    }

    /// <summary>
    /// UI component for a single leaderboard entry.
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _crown;

        [Header("Rank Colors")]
        [SerializeField] private Color _goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color _silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color _bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color _currentPlayerColor = new Color(0.3f, 0.6f, 1f, 0.3f);

        public void Setup(LeaderboardEntry entry, bool isCurrentPlayer)
        {
            if (_rankText != null)
            {
                _rankText.text = $"#{entry.Rank}";
                _rankText.color = GetRankColor(entry.Rank);
            }

            if (_nameText != null)
            {
                _nameText.text = entry.PlayerName;
            }

            if (_scoreText != null)
            {
                _scoreText.text = FormatScore(entry.Score);
            }

            // Show crown for top 3
            if (_crown != null)
            {
                _crown.SetActive(entry.Rank <= 3);
            }

            // Highlight current player
            if (_backgroundImage != null && isCurrentPlayer)
            {
                _backgroundImage.color = _currentPlayerColor;
            }
        }

        private Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return _goldColor;
                case 2: return _silverColor;
                case 3: return _bronzeColor;
                default: return Color.white;
            }
        }

        private string FormatScore(long score)
        {
            if (score >= 1000000)
            {
                return $"{score / 1000000f:F1}M";
            }
            if (score >= 1000)
            {
                return $"{score / 1000f:F1}K";
            }
            return score.ToString();
        }
    }
}
