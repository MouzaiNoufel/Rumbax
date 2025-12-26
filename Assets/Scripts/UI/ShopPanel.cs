using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Rumbax.Core.Services;

namespace Rumbax.UI
{
    /// <summary>
    /// Shop panel for purchasing gems, coins, and subscriptions.
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button coinsTabButton;
        [SerializeField] private Button gemsTabButton;
        [SerializeField] private Button subscriptionTabButton;
        
        [Header("Tab Panels")]
        [SerializeField] private GameObject coinsPanel;
        [SerializeField] private GameObject gemsPanel;
        [SerializeField] private GameObject subscriptionPanel;
        
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI gemsText;
        
        [Header("Subscription Info")]
        [SerializeField] private TextMeshProUGUI subscriptionStatusText;
        [SerializeField] private GameObject subscriptionActiveIndicator;
        [SerializeField] private Button monthlySubButton;
        [SerializeField] private Button yearlySubButton;
        [SerializeField] private TextMeshProUGUI monthlyPriceText;
        [SerializeField] private TextMeshProUGUI yearlyPriceText;
        
        [Header("Gem Packages")]
        [SerializeField] private List<ShopItemUI> gemPackages;
        
        [Header("Coin Offers")]
        [SerializeField] private Button watchAdForCoinsButton;
        [SerializeField] private TextMeshProUGUI adCoinsAmountText;
        
        private ISubscriptionService _subscriptionService;
        private IAdService _adService;
        private ICurrencyService _currencyService;

        private void OnEnable()
        {
            _subscriptionService = ServiceLocator.Get<ISubscriptionService>();
            _adService = ServiceLocator.Get<IAdService>();
            _currencyService = ServiceLocator.Get<ICurrencyService>();
            
            SetupButtons();
            ShowTab(0);
            UpdateCurrencyDisplay();
            UpdateSubscriptionStatus();
        }

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            if (coinsTabButton != null)
                coinsTabButton.onClick.AddListener(() => ShowTab(0));
            
            if (gemsTabButton != null)
                gemsTabButton.onClick.AddListener(() => ShowTab(1));
            
            if (subscriptionTabButton != null)
                subscriptionTabButton.onClick.AddListener(() => ShowTab(2));
            
            if (monthlySubButton != null)
                monthlySubButton.onClick.AddListener(OnMonthlySubscriptionClicked);
            
            if (yearlySubButton != null)
                yearlySubButton.onClick.AddListener(OnYearlySubscriptionClicked);
            
            if (watchAdForCoinsButton != null)
                watchAdForCoinsButton.onClick.AddListener(OnWatchAdForCoinsClicked);
            
            // Setup gem packages
            foreach (var package in gemPackages)
            {
                package?.Setup(OnGemPackagePurchased);
            }
        }

        private void ShowTab(int tabIndex)
        {
            PlayButtonSound();
            
            if (coinsPanel != null)
                coinsPanel.SetActive(tabIndex == 0);
            
            if (gemsPanel != null)
                gemsPanel.SetActive(tabIndex == 1);
            
            if (subscriptionPanel != null)
                subscriptionPanel.SetActive(tabIndex == 2);
        }

        private void UpdateCurrencyDisplay()
        {
            if (coinsText != null && _currencyService != null)
                coinsText.text = FormatNumber(_currencyService.Coins);
            
            if (gemsText != null && _currencyService != null)
                gemsText.text = _currencyService.Gems.ToString();
            
            // Update ad button
            if (watchAdForCoinsButton != null)
                watchAdForCoinsButton.interactable = _adService?.IsRewardedAdReady == true;
            
            var config = Core.GameManager.Instance?.Config;
            if (adCoinsAmountText != null && config != null)
                adCoinsAmountText.text = $"+{config.RewardedAdCoinBonus}";
        }

        private void UpdateSubscriptionStatus()
        {
            bool isSubscribed = _subscriptionService?.IsSubscribed ?? false;
            
            if (subscriptionActiveIndicator != null)
                subscriptionActiveIndicator.SetActive(isSubscribed);
            
            if (subscriptionStatusText != null)
            {
                if (isSubscribed)
                {
                    var tier = _subscriptionService.CurrentTier;
                    var expiry = _subscriptionService.SubscriptionExpiry;
                    subscriptionStatusText.text = $"{tier} - Expires {expiry:MMM dd, yyyy}";
                }
                else
                {
                    subscriptionStatusText.text = "Not subscribed";
                }
            }
            
            // Update button states
            if (monthlySubButton != null)
                monthlySubButton.interactable = !isSubscribed;
            
            if (yearlySubButton != null)
                yearlySubButton.interactable = !isSubscribed;
        }

        private void OnCloseClicked()
        {
            PlayButtonSound();
            gameObject.SetActive(false);
        }

        private void OnMonthlySubscriptionClicked()
        {
            PlayButtonSound();
            _subscriptionService?.PurchaseSubscription(SubscriptionTier.Monthly);
        }

        private void OnYearlySubscriptionClicked()
        {
            PlayButtonSound();
            _subscriptionService?.PurchaseSubscription(SubscriptionTier.Yearly);
        }

        private void OnGemPackagePurchased(string productId)
        {
            PlayButtonSound();
            _subscriptionService?.PurchaseProduct(productId);
        }

        private void OnWatchAdForCoinsClicked()
        {
            PlayButtonSound();
            
            if (_adService?.IsRewardedAdReady == true)
            {
                _adService.ShowRewardedAd(
                    onReward: () => {
                        var config = Core.GameManager.Instance?.Config;
                        int bonus = config?.RewardedAdCoinBonus ?? 100;
                        _currencyService?.AddCoins(bonus);
                        UpdateCurrencyDisplay();
                        ServiceLocator.Get<IAnalyticsService>()?.LogAdWatched("rewarded", "shop_coins");
                    }
                );
            }
        }

        private void PlayButtonSound()
        {
            ServiceLocator.Get<IAudioService>()?.PlaySfxOneShot("button_click");
        }

        private string FormatNumber(long number)
        {
            if (number >= 1000000)
                return (number / 1000000f).ToString("0.#") + "M";
            if (number >= 1000)
                return (number / 1000f).ToString("0.#") + "K";
            return number.ToString();
        }
    }

    /// <summary>
    /// Individual shop item UI component.
    /// </summary>
    [System.Serializable]
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private string productId;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image iconImage;
        
        private System.Action<string> _onPurchase;

        public void Setup(System.Action<string> onPurchase)
        {
            _onPurchase = onPurchase;
            
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        private void OnPurchaseClicked()
        {
            _onPurchase?.Invoke(productId);
        }
    }
}
