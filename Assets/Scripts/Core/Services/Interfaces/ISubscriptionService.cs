using System;
using Rumbax.Data;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for subscription and IAP management.
    /// </summary>
    public interface ISubscriptionService
    {
        bool IsSubscribed { get; }
        SubscriptionTier CurrentTier { get; }
        DateTime SubscriptionExpiry { get; }
        
        event Action<SubscriptionTier> OnSubscriptionChanged;
        event Action<string> OnPurchaseComplete;
        event Action<string> OnPurchaseFailed;
        
        void Initialize();
        void PurchaseSubscription(SubscriptionTier tier);
        void PurchaseProduct(string productId);
        void RestorePurchases();
        SubscriptionBenefits GetCurrentBenefits();
        bool HasBenefit(SubscriptionBenefit benefit);
    }

    public enum SubscriptionTier
    {
        None,
        Monthly,
        Yearly
    }

    public enum SubscriptionBenefit
    {
        RemoveAds,
        ExclusiveSkins,
        FasterProgression,
        PremiumRewards,
        DoubleOfflineEarnings,
        ExtraLives
    }
}
