using System;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for advertisement management.
    /// </summary>
    public interface IAdService
    {
        bool IsRewardedAdReady { get; }
        bool IsInterstitialReady { get; }
        bool AdsRemoved { get; }
        
        event Action OnRewardedAdCompleted;
        event Action OnRewardedAdFailed;
        event Action OnInterstitialClosed;
        
        void Initialize();
        void ShowBanner();
        void HideBanner();
        void ShowInterstitial(Action onComplete = null);
        void ShowRewardedAd(Action onReward, Action onFailed = null);
        void RemoveAds();
    }
}
