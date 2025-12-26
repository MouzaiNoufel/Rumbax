using System;
using UnityEngine;
using Rumbax.Core.Services;

namespace Rumbax.Monetization
{
    /// <summary>
    /// AdMob integration service for banner, interstitial, and rewarded ads.
    /// Uses Google Mobile Ads Unity plugin.
    /// </summary>
    public class AdMobService : MonoBehaviour, IAdService
    {
        [Header("Ad Unit IDs - Android")]
        [SerializeField] private string androidBannerId = "ca-app-pub-xxxxx/banner";
        [SerializeField] private string androidInterstitialId = "ca-app-pub-xxxxx/interstitial";
        [SerializeField] private string androidRewardedId = "ca-app-pub-xxxxx/rewarded";
        
        [Header("Ad Unit IDs - iOS")]
        [SerializeField] private string iosBannerId = "ca-app-pub-xxxxx/banner";
        [SerializeField] private string iosInterstitialId = "ca-app-pub-xxxxx/interstitial";
        [SerializeField] private string iosRewardedId = "ca-app-pub-xxxxx/rewarded";
        
        [Header("Settings")]
        [SerializeField] private bool useTestAds = true;
        [SerializeField] private int interstitialFrequency = 3;
        
        // Test Ad Unit IDs
        private const string TEST_BANNER = "ca-app-pub-3940256099942544/6300978111";
        private const string TEST_INTERSTITIAL = "ca-app-pub-3940256099942544/1033173712";
        private const string TEST_REWARDED = "ca-app-pub-3940256099942544/5224354917";
        
        private string _bannerId;
        private string _interstitialId;
        private string _rewardedId;
        
        private bool _isInitialized;
        private bool _bannerLoaded;
        private bool _interstitialLoaded;
        private bool _rewardedLoaded;
        private bool _adsRemoved;
        private int _levelsSinceLastInterstitial;
        
        private Action _onRewardCallback;
        private Action _onRewardFailedCallback;
        private Action _onInterstitialClosedCallback;

        public bool IsRewardedAdReady => _rewardedLoaded;
        public bool IsInterstitialReady => _interstitialLoaded && !_adsRemoved;
        public bool AdsRemoved => _adsRemoved;

        public event Action OnRewardedAdCompleted;
        public event Action OnRewardedAdFailed;
        public event Action OnInterstitialClosed;

        private void Awake()
        {
            SetupAdUnitIds();
        }

        private void Start()
        {
            CheckAdsRemoved();
            Initialize();
        }

        private void SetupAdUnitIds()
        {
            if (useTestAds)
            {
                _bannerId = TEST_BANNER;
                _interstitialId = TEST_INTERSTITIAL;
                _rewardedId = TEST_REWARDED;
            }
            else
            {
#if UNITY_ANDROID
                _bannerId = androidBannerId;
                _interstitialId = androidInterstitialId;
                _rewardedId = androidRewardedId;
#elif UNITY_IOS
                _bannerId = iosBannerId;
                _interstitialId = iosInterstitialId;
                _rewardedId = iosRewardedId;
#else
                _bannerId = TEST_BANNER;
                _interstitialId = TEST_INTERSTITIAL;
                _rewardedId = TEST_REWARDED;
#endif
            }
        }

        private void CheckAdsRemoved()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            _adsRemoved = playerData?.AdsRemoved ?? false;
            
            // Also check subscription
            var subService = ServiceLocator.Get<ISubscriptionService>();
            if (subService?.HasBenefit(SubscriptionBenefit.RemoveAds) == true)
            {
                _adsRemoved = true;
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[AdMobService] Initializing Google Mobile Ads SDK...");

            // Note: In actual implementation, uncomment this when Google Mobile Ads SDK is imported
            /*
            MobileAds.Initialize(initStatus => {
                Debug.Log("[AdMobService] SDK Initialized");
                _isInitialized = true;
                
                LoadInterstitial();
                LoadRewarded();
                
                if (!_adsRemoved)
                {
                    LoadBanner();
                }
            });
            */
            
            // Simulated initialization for development
            _isInitialized = true;
            SimulateAdLoad();
        }

        private void SimulateAdLoad()
        {
            // For testing without SDK
            _bannerLoaded = true;
            _interstitialLoaded = true;
            _rewardedLoaded = true;
            Debug.Log("[AdMobService] Simulated ad load complete");
        }

        #region Banner Ads

        public void ShowBanner()
        {
            if (_adsRemoved)
            {
                Debug.Log("[AdMobService] Ads removed - not showing banner");
                return;
            }

            Debug.Log("[AdMobService] Showing banner ad");
            
            // Actual implementation:
            /*
            if (_bannerView == null)
            {
                LoadBanner();
            }
            _bannerView?.Show();
            */
        }

        public void HideBanner()
        {
            Debug.Log("[AdMobService] Hiding banner ad");
            
            // Actual implementation:
            // _bannerView?.Hide();
        }

        private void LoadBanner()
        {
            if (_adsRemoved) return;

            Debug.Log("[AdMobService] Loading banner ad");
            
            // Actual implementation:
            /*
            _bannerView?.Destroy();
            
            _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Bottom);
            
            _bannerView.OnBannerAdLoaded += () => {
                Debug.Log("[AdMobService] Banner loaded");
                _bannerLoaded = true;
            };
            
            _bannerView.OnBannerAdLoadFailed += (error) => {
                Debug.LogError($"[AdMobService] Banner failed: {error}");
                _bannerLoaded = false;
            };
            
            var request = new AdRequest();
            _bannerView.LoadAd(request);
            */
        }

        #endregion

        #region Interstitial Ads

        public void ShowInterstitial(Action onComplete = null)
        {
            if (_adsRemoved)
            {
                Debug.Log("[AdMobService] Ads removed - skipping interstitial");
                onComplete?.Invoke();
                return;
            }

            _levelsSinceLastInterstitial++;
            
            if (_levelsSinceLastInterstitial < interstitialFrequency)
            {
                Debug.Log($"[AdMobService] Interstitial frequency not met ({_levelsSinceLastInterstitial}/{interstitialFrequency})");
                onComplete?.Invoke();
                return;
            }

            if (!_interstitialLoaded)
            {
                Debug.Log("[AdMobService] Interstitial not ready");
                onComplete?.Invoke();
                LoadInterstitial();
                return;
            }

            _onInterstitialClosedCallback = onComplete;
            _levelsSinceLastInterstitial = 0;
            
            Debug.Log("[AdMobService] Showing interstitial ad");
            
            // Actual implementation:
            /*
            _interstitialAd?.Show();
            */
            
            // Simulated for development
            SimulateInterstitialClose();
        }

        private void SimulateInterstitialClose()
        {
            // Simulate ad close after delay
            Invoke(nameof(OnInterstitialAdClosed), 0.5f);
        }

        private void LoadInterstitial()
        {
            Debug.Log("[AdMobService] Loading interstitial ad");
            
            // Actual implementation:
            /*
            InterstitialAd.Load(_interstitialId, new AdRequest(), (ad, error) => {
                if (error != null)
                {
                    Debug.LogError($"[AdMobService] Interstitial load failed: {error}");
                    _interstitialLoaded = false;
                    return;
                }
                
                _interstitialAd = ad;
                _interstitialLoaded = true;
                
                _interstitialAd.OnAdFullScreenContentClosed += OnInterstitialAdClosed;
                _interstitialAd.OnAdFullScreenContentFailed += OnInterstitialAdFailed;
            });
            */
            
            _interstitialLoaded = true;
        }

        private void OnInterstitialAdClosed()
        {
            Debug.Log("[AdMobService] Interstitial closed");
            
            _interstitialLoaded = false;
            LoadInterstitial();
            
            _onInterstitialClosedCallback?.Invoke();
            _onInterstitialClosedCallback = null;
            
            OnInterstitialClosed?.Invoke();
        }

        private void OnInterstitialAdFailed()
        {
            Debug.Log("[AdMobService] Interstitial failed to show");
            _onInterstitialClosedCallback?.Invoke();
            _onInterstitialClosedCallback = null;
        }

        #endregion

        #region Rewarded Ads

        public void ShowRewardedAd(Action onReward, Action onFailed = null)
        {
            if (!_rewardedLoaded)
            {
                Debug.Log("[AdMobService] Rewarded ad not ready");
                onFailed?.Invoke();
                LoadRewarded();
                return;
            }

            _onRewardCallback = onReward;
            _onRewardFailedCallback = onFailed;
            
            Debug.Log("[AdMobService] Showing rewarded ad");
            
            // Actual implementation:
            /*
            _rewardedAd?.Show((reward) => {
                Debug.Log($"[AdMobService] User rewarded: {reward.Amount} {reward.Type}");
                OnRewardedAdEarned();
            });
            */
            
            // Simulated for development
            SimulateRewardedComplete();
        }

        private void SimulateRewardedComplete()
        {
            // Simulate ad completion
            Invoke(nameof(OnRewardedAdEarned), 1f);
        }

        private void LoadRewarded()
        {
            Debug.Log("[AdMobService] Loading rewarded ad");
            
            // Actual implementation:
            /*
            RewardedAd.Load(_rewardedId, new AdRequest(), (ad, error) => {
                if (error != null)
                {
                    Debug.LogError($"[AdMobService] Rewarded load failed: {error}");
                    _rewardedLoaded = false;
                    return;
                }
                
                _rewardedAd = ad;
                _rewardedLoaded = true;
                
                _rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdClosed;
                _rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdShowFailed;
            });
            */
            
            _rewardedLoaded = true;
        }

        private void OnRewardedAdEarned()
        {
            Debug.Log("[AdMobService] Reward earned");
            
            _onRewardCallback?.Invoke();
            _onRewardCallback = null;
            _onRewardFailedCallback = null;
            
            OnRewardedAdCompleted?.Invoke();
            
            _rewardedLoaded = false;
            LoadRewarded();
        }

        private void OnRewardedAdClosed()
        {
            Debug.Log("[AdMobService] Rewarded ad closed");
            _rewardedLoaded = false;
            LoadRewarded();
        }

        private void OnRewardedAdShowFailed()
        {
            Debug.Log("[AdMobService] Rewarded ad failed to show");
            
            _onRewardFailedCallback?.Invoke();
            _onRewardCallback = null;
            _onRewardFailedCallback = null;
            
            OnRewardedAdFailed?.Invoke();
            
            LoadRewarded();
        }

        #endregion

        public void RemoveAds()
        {
            _adsRemoved = true;
            HideBanner();
            
            var saveService = ServiceLocator.Get<ISaveService>();
            var playerData = saveService?.GetPlayerData();
            if (playerData != null)
            {
                playerData.AdsRemoved = true;
                saveService.SaveGame();
            }
            
            Debug.Log("[AdMobService] Ads removed");
        }

        private void OnDestroy()
        {
            // Cleanup ads
            // _bannerView?.Destroy();
            // _interstitialAd?.Destroy();
            // _rewardedAd?.Destroy();
        }
    }
}
