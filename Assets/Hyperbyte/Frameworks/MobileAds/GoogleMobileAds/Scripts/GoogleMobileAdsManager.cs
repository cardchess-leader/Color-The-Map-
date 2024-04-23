// ©2019 - 2020 HYPERBYTE STUDIOS LLP
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;

#if HB_ADMOB
using GoogleMobileAds.Api;
using System;
#endif

namespace Hyperbyte.Ads
{
    /// <summary>
    /// This class component will be added to game dynamically if Google Ads is selected as active ad network.
    /// All the callbacks will be forwarded to ad manager.
    /// </summary>
    public class GoogleMobileAdsManager : AdHelper
    {
        GoogleMobileAdsSettings settings;

        #if HB_ADMOB
        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardedAd;
        #endif

        /// Initialized the ad network.
        public override void InitializeAdNetwork()
        {
            settings = (GoogleMobileAdsSettings)(Resources.Load("AdNetworkSettings/GoogleMobileAdsSettings"));

            #if HB_ADMOB
            
            RequestConfiguration requestConfiguration = new RequestConfiguration.Builder().SetSameAppKeyEnabled(true).build();
            MobileAds.SetRequestConfiguration(requestConfiguration);
            
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.Initialize((InitializationStatus initStatus) => {
                Invoke(nameof(StartLoadingAds), 1F);
            });
            #endif
        }

        /// Loads ads after initialization.
        public void StartLoadingAds() {
            RequestBannerAds();
            RequestInterstitial();
            RequestRewarded();
        }

        // Requests banner ad.        
        public void RequestBannerAds()
        {
            #if HB_ADMOB
            if (this.bannerView != null) {
                this.bannerView.Destroy();
            }
            bannerView = new BannerView(settings.GetBannetAdUnitId(), AdSize.Banner, settings.GetBannerPosition());
            AdRequest request = new AdRequest.Builder().Build();

            // Register for ad events.
            this.bannerView.OnBannerAdLoaded += this.HandleBannerAdLoaded;
            this.bannerView.OnBannerAdLoadFailed += this.HandleBannerAdFailedToLoad;
            this.bannerView.OnAdPaid += this.HandleOnBannerAdPaid;

            this.bannerView.OnAdImpressionRecorded += HandleOnBannerAdImpressionRecorded;
            this.bannerView.OnAdClicked += HandleOnBannerAdClicked;
            this.bannerView.OnAdFullScreenContentOpened += HandleOnBannerAdFullScreenContentOpened;
            this.bannerView.OnAdFullScreenContentClosed += HandleOnBannerAdFullScreenContentClosed;
            
            bannerView.LoadAd(this.CreateAdRequest());
            bannerView.Hide();

            if (AdManager.Instance.adSettings.showBannerOnLoad) {
                bannerView.Show();
            }
            #endif
        }

        // Requests intestitial ad.
        public void RequestInterstitial()
        {
            #if HB_ADMOB
            if (this.interstitial != null) {
                this.interstitial.Destroy();
            }
            // Create an interstitial.
            
            InterstitialAd.Load(settings.GetInterstitialAdUnityId(), CreateAdRequest(),
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null) {
                        return;
                    }
                    this.interstitial = ad;
                    this.interstitial.OnAdPaid += this.HandleOnInterstitialAdPaid;
                    this.interstitial.OnAdFullScreenContentClosed += OnInterstitialAdFullScreenContentClosed;
                    this.interstitial.OnAdFullScreenContentFailed += OnInterstitialAdFullScreenContentFailed;
                });

            // Register for ad events.
            #endif
        }

        // Requests rewarded ad.
        public void RequestRewarded()
        {
            #if HB_ADMOB
            
            if (rewardedAd != null) {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
            
            RewardedAd.Load(settings.GetRewardedAdUnitId(), CreateAdRequest(),
                (ad, error) => {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null) {
                        return;
                    }
                    rewardedAd = ad;
                    
                    this.rewardedAd.OnAdPaid += OnRewardedAdPaid;
                    this.rewardedAd.OnAdFullScreenContentOpened += OnRewardedAdOpening;
                    this.rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFailedToShow;
                    this.rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdClosed;
                });
            #endif
        }

        #if HB_ADMOB
        private AdRequest CreateAdRequest()
        {
            return new AdRequest.Builder()
                .AddExtra("npa", (AdManager.Instance.consentAllowed) ? "0" : "1")
                //.AddTestDevice(AdRequest.TestDeviceSimulator)
                //.AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
                .AddKeyword("game")
                //.SetGender(Gender.Male)
                //.SetBirthday(new DateTime(1985, 1, 1))
                .AddExtra("color_bg", settings.GetBannerBgColor())
                .Build();

        }
        #endif
        
        // Shows banner ad.
        public override void ShowBanner()
        {
            #if HB_ADMOB
            if (this.bannerView != null) {
                this.bannerView.Show();
            }
            #endif
        }
    
        // Hides banner ad.
        public override void HideBanner()
        {
            #if HB_ADMOB
            if (this.bannerView != null) {
                this.bannerView.Hide();
            }
            #endif
        }

        // Check if interstitial ad ready to show.
        public override bool IsInterstitialAvailable()
        {
            #if HB_ADMOB
            return this.interstitial.CanShowAd(); 
            #endif
            return false;
        }
        
        // Shows interstitial ad if available.
        public override void ShowInterstitial()
        {
            #if HB_ADMOB
            if (this.interstitial.CanShowAd()) {
                this.interstitial.Show();
            }
            #endif
        }

        // Checks if rewarded ad ready to show.
        public override bool IsRewardedAvailable()
        {
            #if HB_ADMOB
            return this.rewardedAd.CanShowAd(); 
            #endif
            return false;
        }

        // Shows rewarded ad if loaded.
        public override void ShowRewarded()
        {
            #if HB_ADMOB
            if (this.rewardedAd.CanShowAd()) {
                this.rewardedAd.Show((Reward reward) => {
                    
                });
            }
            #endif
        }

        #if HB_ADMOB
        #region Banner callback handlers
        // Banner ad  event callbacks.
        public void HandleBannerAdLoaded() {
            AdManager.Instance.OnBannerLoaded();
        }

        public void HandleBannerAdFailedToLoad(LoadAdError error) {
            AdManager.Instance.OnBannerLoadFailed(error.ToString());
        }

        public void HandleOnBannerAdPaid(AdValue adValue) {}
        public void HandleOnBannerAdImpressionRecorded() {}
        public void HandleOnBannerAdClicked() {}
        public void HandleOnBannerAdFullScreenContentOpened(){}
        public void HandleOnBannerAdFullScreenContentClosed() {}
        #endregion

        #region Interstitial callback handlers
        // Interstitial ad event callbacks.
        
        public void HandleOnInterstitialAdPaid(AdValue adValue) {}
        
        public void OnInterstitialAdFullScreenContentClosed() {
            AdManager.Instance.OnInterstitialClosed();
            RequestInterstitial();
        }

        public void OnInterstitialAdFullScreenContentFailed(AdError error) {
            AdManager.Instance.OnInterstitialLoadFailed(error.ToString());
            RequestInterstitial();
        }
        #endregion

        #region RewardedAd callback handlers
        public void OnRewardedAdOpening() {
           AdManager.Instance.OnRewardedShown();
        }

        public void OnRewardedAdFailedToShow(AdError adError) {
            RequestRewarded();
        }

        public void OnRewardedAdClosed() {
            RequestRewarded();
            AdManager.Instance.OnRewardedClosed();
        }

        public void OnRewardedAdPaid(AdValue adValue) {
            AdManager.Instance.OnRewardedAdRewarded();
        }
        #endregion
        #endif
    }
}
