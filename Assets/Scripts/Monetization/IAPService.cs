using System;
using System.Collections.Generic;
using UnityEngine;
using Rumbax.Core;
using Rumbax.Core.Services;
using Rumbax.Core.Events;
using Rumbax.Data;

namespace Rumbax.Monetization
{
    /// <summary>
    /// Google Play Billing integration for subscriptions and in-app purchases.
    /// Uses Unity IAP for cross-platform support.
    /// </summary>
    public class IAPService : MonoBehaviour, ISubscriptionService
    {
        [Header("Product IDs")]
        [SerializeField] private string monthlySubscriptionId = "com.rumbax.subscription.monthly";
        [SerializeField] private string yearlySubscriptionId = "com.rumbax.subscription.yearly";
        [SerializeField] private string removeAdsId = "com.rumbax.removeads";
        [SerializeField] private List<GemPackage> gemPackages = new List<GemPackage>();
        
        [Header("Subscription Benefits")]
        [SerializeField] private SubscriptionBenefits monthlyBenefits;
        [SerializeField] private SubscriptionBenefits yearlyBenefits;
        
        private bool _isInitialized;
        private bool _isSubscribed;
        private SubscriptionTier _currentTier = SubscriptionTier.None;
        private DateTime _subscriptionExpiry;
        private SubscriptionBenefits _currentBenefits;
        
        // Unity IAP references (uncomment when Unity IAP is imported)
        // private IStoreController _storeController;
        // private IExtensionProvider _extensionProvider;

        public bool IsSubscribed => _isSubscribed;
        public SubscriptionTier CurrentTier => _currentTier;
        public DateTime SubscriptionExpiry => _subscriptionExpiry;

        public event Action<SubscriptionTier> OnSubscriptionChanged;
        public event Action<string> OnPurchaseComplete;
        public event Action<string> OnPurchaseFailed;

        private void Awake()
        {
            InitializeBenefits();
        }

        private void Start()
        {
            Initialize();
            CheckSubscriptionStatus();
        }

        private void InitializeBenefits()
        {
            if (monthlyBenefits == null)
            {
                monthlyBenefits = new SubscriptionBenefits
                {
                    RemoveAds = true,
                    ExclusiveSkins = true,
                    ProgressionMultiplier = 1.5f,
                    CoinMultiplier = 1.5f,
                    DoubleOfflineEarnings = false,
                    ExtraLives = 1
                };
            }
            
            if (yearlyBenefits == null)
            {
                yearlyBenefits = new SubscriptionBenefits
                {
                    RemoveAds = true,
                    ExclusiveSkins = true,
                    ProgressionMultiplier = 2f,
                    CoinMultiplier = 2f,
                    DoubleOfflineEarnings = true,
                    ExtraLives = 3,
                    PrioritySupport = true
                };
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[IAPService] Initializing Unity IAP...");

            // Actual Unity IAP implementation:
            /*
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
            // Add subscriptions
            builder.AddProduct(monthlySubscriptionId, ProductType.Subscription);
            builder.AddProduct(yearlySubscriptionId, ProductType.Subscription);
            
            // Add consumables
            builder.AddProduct(removeAdsId, ProductType.NonConsumable);
            
            foreach (var package in gemPackages)
            {
                builder.AddProduct(package.productId, ProductType.Consumable);
            }
            
            UnityPurchasing.Initialize(this, builder);
            */

            _isInitialized = true;
            Debug.Log("[IAPService] Initialized (simulated)");
        }

        private void CheckSubscriptionStatus()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                _isSubscribed = playerData.IsSubscribed;
                _subscriptionExpiry = playerData.SubscriptionExpiry;
                
                if (!string.IsNullOrEmpty(playerData.SubscriptionTier))
                {
                    Enum.TryParse(playerData.SubscriptionTier, out _currentTier);
                }
                
                // Check if subscription has expired
                if (_isSubscribed && DateTime.UtcNow > _subscriptionExpiry)
                {
                    HandleSubscriptionExpired();
                }
                else if (_isSubscribed)
                {
                    ApplySubscriptionBenefits();
                }
            }
        }

        private void HandleSubscriptionExpired()
        {
            Debug.Log("[IAPService] Subscription expired");
            
            _isSubscribed = false;
            _currentTier = SubscriptionTier.None;
            _currentBenefits = null;
            
            // Update player data
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                playerData.IsSubscribed = false;
                playerData.SubscriptionTier = "";
                // Keep AdsRemoved if they purchased it separately
                saveService.SaveGame();
            }
            
            OnSubscriptionChanged?.Invoke(SubscriptionTier.None);
            ServiceLocator.Get<IEventBus>()?.Publish(
                new SubscriptionChangedEvent(false, SubscriptionTier.None));
        }

        public void PurchaseSubscription(SubscriptionTier tier)
        {
            string productId = tier == SubscriptionTier.Monthly ? 
                monthlySubscriptionId : yearlySubscriptionId;
            
            Debug.Log($"[IAPService] Purchasing subscription: {tier}");
            
            // Actual implementation:
            /*
            if (_storeController != null)
            {
                var product = _storeController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _storeController.InitiatePurchase(product);
                }
            }
            */
            
            // Simulated purchase for development
            SimulatePurchase(productId, tier);
        }

        public void PurchaseProduct(string productId)
        {
            Debug.Log($"[IAPService] Purchasing product: {productId}");
            
            // Actual implementation:
            /*
            if (_storeController != null)
            {
                var product = _storeController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _storeController.InitiatePurchase(product);
                }
            }
            */
            
            // Simulated for development
            SimulateProductPurchase(productId);
        }

        private void SimulatePurchase(string productId, SubscriptionTier tier)
        {
            // Simulate successful subscription purchase
            _isSubscribed = true;
            _currentTier = tier;
            _subscriptionExpiry = tier == SubscriptionTier.Monthly ? 
                DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddYears(1);
            
            SaveSubscriptionData();
            ApplySubscriptionBenefits();
            
            OnPurchaseComplete?.Invoke(productId);
            OnSubscriptionChanged?.Invoke(tier);
            ServiceLocator.Get<IEventBus>()?.Publish(
                new SubscriptionChangedEvent(true, tier));
            
            Debug.Log($"[IAPService] Subscription activated: {tier}, expires: {_subscriptionExpiry}");
        }

        private void SimulateProductPurchase(string productId)
        {
            // Check if it's a gem package
            var package = gemPackages.Find(p => p.productId == productId);
            if (package != null)
            {
                ServiceLocator.Get<ICurrencyService>()?.AddGems(package.gemAmount);
                OnPurchaseComplete?.Invoke(productId);
                
                ServiceLocator.Get<IAnalyticsService>()?.LogPurchase(
                    productId, package.priceUSD, "USD");
                
                Debug.Log($"[IAPService] Gems purchased: {package.gemAmount}");
                return;
            }
            
            // Check if it's remove ads
            if (productId == removeAdsId)
            {
                ServiceLocator.Get<IAdService>()?.RemoveAds();
                OnPurchaseComplete?.Invoke(productId);
                Debug.Log("[IAPService] Ads removed");
            }
        }

        private void SaveSubscriptionData()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                playerData.IsSubscribed = _isSubscribed;
                playerData.SubscriptionTier = _currentTier.ToString();
                playerData.SubscriptionExpiry = _subscriptionExpiry;
                
                if (_currentBenefits?.RemoveAds == true)
                {
                    playerData.AdsRemoved = true;
                }
                
                saveService.SaveGame();
            }
        }

        private void ApplySubscriptionBenefits()
        {
            _currentBenefits = _currentTier == SubscriptionTier.Yearly ? 
                yearlyBenefits : monthlyBenefits;
            
            if (_currentBenefits == null) return;
            
            // Apply remove ads
            if (_currentBenefits.RemoveAds)
            {
                ServiceLocator.Get<IAdService>()?.RemoveAds();
            }
            
            // Apply multipliers to player data
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            
            if (playerData != null)
            {
                playerData.CoinMultiplier = _currentBenefits.CoinMultiplier;
                playerData.ExpMultiplier = _currentBenefits.ProgressionMultiplier;
            }
            
            Debug.Log($"[IAPService] Benefits applied: {_currentTier}");
        }

        public void RestorePurchases()
        {
            Debug.Log("[IAPService] Restoring purchases...");
            
            // Actual implementation:
            /*
            #if UNITY_IOS
            var apple = _extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, message) => {
                Debug.Log($"[IAPService] Restore result: {result}, {message}");
            });
            #elif UNITY_ANDROID
            var google = _extensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            google.RestoreTransactions((result, message) => {
                Debug.Log($"[IAPService] Restore result: {result}, {message}");
            });
            #endif
            */
            
            Debug.Log("[IAPService] Purchases restored (simulated)");
        }

        public SubscriptionBenefits GetCurrentBenefits()
        {
            return _currentBenefits;
        }

        public bool HasBenefit(SubscriptionBenefit benefit)
        {
            if (!_isSubscribed || _currentBenefits == null) return false;
            
            return benefit switch
            {
                SubscriptionBenefit.RemoveAds => _currentBenefits.RemoveAds,
                SubscriptionBenefit.ExclusiveSkins => _currentBenefits.ExclusiveSkins,
                SubscriptionBenefit.FasterProgression => _currentBenefits.ProgressionMultiplier > 1f,
                SubscriptionBenefit.PremiumRewards => _currentTier == SubscriptionTier.Yearly,
                SubscriptionBenefit.DoubleOfflineEarnings => _currentBenefits.DoubleOfflineEarnings,
                SubscriptionBenefit.ExtraLives => _currentBenefits.ExtraLives > 0,
                _ => false
            };
        }

        #region Unity IAP Callbacks (Uncomment when Unity IAP is imported)
        
        /*
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[IAPService] Unity IAP initialized successfully");
            _storeController = controller;
            _extensionProvider = extensions;
            _isInitialized = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[IAPService] Unity IAP initialization failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAPService] Unity IAP initialization failed: {error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            Debug.Log($"[IAPService] Purchase successful: {productId}");
            
            if (productId == monthlySubscriptionId)
            {
                ActivateSubscription(SubscriptionTier.Monthly);
            }
            else if (productId == yearlySubscriptionId)
            {
                ActivateSubscription(SubscriptionTier.Yearly);
            }
            else if (productId == removeAdsId)
            {
                ServiceLocator.Get<IAdService>()?.RemoveAds();
            }
            else
            {
                // Check gem packages
                var package = gemPackages.Find(p => p.productId == productId);
                if (package != null)
                {
                    ServiceLocator.Get<ICurrencyService>()?.AddGems(package.gemAmount);
                }
            }
            
            OnPurchaseComplete?.Invoke(productId);
            
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"[IAPService] Purchase failed: {product.definition.id} - {reason}");
            OnPurchaseFailed?.Invoke(product.definition.id);
        }
        */
        
        #endregion

        private void ActivateSubscription(SubscriptionTier tier)
        {
            _isSubscribed = true;
            _currentTier = tier;
            _subscriptionExpiry = tier == SubscriptionTier.Monthly ? 
                DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddYears(1);
            
            SaveSubscriptionData();
            ApplySubscriptionBenefits();
            
            OnSubscriptionChanged?.Invoke(tier);
        }
    }

    /// <summary>
    /// Gem package configuration for the shop.
    /// </summary>
    [Serializable]
    public class GemPackage
    {
        public string productId;
        public string displayName;
        public int gemAmount;
        public decimal priceUSD;
        public bool isBestValue;
        public float bonusPercent;
    }
}
