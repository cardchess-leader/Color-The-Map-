// ©2019 - 2020 HYPERBYTE STUDIOS LLP
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;

#if HB_UNITYADS 
using UnityEngine.Advertisements;
#endif

namespace Hyperbyte.Ads
{
    class UnityAdsInitializationListener : IUnityAdsInitializationListener
    {
        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads Initialization Complete.");
            // Here you can start loading ads since the SDK is initialized
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
        }
    }
    /// <summary>
    /// This class component will be added to game dynamically if Unity Ads selected as active ad network.
    /// All the callbacks will be forwarded to ad manager.
    /// </summary>
	public class UnityAdsManager : AdHelper
#if HB_UNITYADS
        , IUnityAdsLoadListener, IUnityAdsShowListener
#endif
    {
        UnityAdsSettings settings;
        bool showBannerCalled = false;


        private bool isInterstitialReady = false;
        private bool isRewardedReady = false;

        /// <summary>
        /// Initialized the ad network.
        /// </summary>
        public override void InitializeAdNetwork()
        {
            settings = (UnityAdsSettings)(Resources.Load("AdNetworkSettings/UnityAdsSettings"));
#if HB_UNITYADS
            UnityAdsInitializationListener initializationListener = new UnityAdsInitializationListener();
            Advertisement.Initialize(settings.GetGameId(), settings.GetTestMode(), initializationListener);            
            Invoke(nameof(StartLoadingAds), 3F);
#endif
        }

        /// <summary>
        /// Loads ads after initialization.
        /// </summary>
        public void StartLoadingAds()
        {
#if HB_UNITYADS
            // GDPR Consent 
            UnityEngine.Advertisements.MetaData gdprMetaData = new UnityEngine.Advertisements.MetaData("gdpr");
            gdprMetaData.Set("consent", (AdManager.Instance.consentAllowed) ? "true" : "false");
            Advertisement.SetMetaData(gdprMetaData);

            // CCPA Consent
            // If the user opts out of targeted advertising:
            UnityEngine.Advertisements.MetaData privacyMetaData = new UnityEngine.Advertisements.MetaData("privacy");
            privacyMetaData.Set("consent",(AdManager.Instance.consentAllowed) ? "true" : "false");
            Advertisement.SetMetaData(privacyMetaData);
#endif
            RequestBannerAds();
            RequestInterstitial();
            RequestRewarded();
        }

        // Requests banner ad.        
        void RequestBannerAds()
        {
#if HB_UNITYADS
            Advertisement.Banner.SetPosition(settings.GetBannerPosition());
            var options = new BannerLoadOptions {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerLoadFailed
            };
            Advertisement.Banner.Load(settings.GetBannerPlacement(), options);
#endif

        }

        // Requests intestitial ad.
        void RequestInterstitial()
        {
#if HB_UNITYADS
            Advertisement.Load(settings.GetInterstitialPlacement(), this);
#endif
        }

        // Requests rewarded ad.
        void RequestRewarded()
        {
#if HB_UNITYADS
            Advertisement.Load(settings.GetRewardedPlacement(), this);
#endif
        }

        // Shows banner ad.
        public override void ShowBanner()
        {
            showBannerCalled = true;
#if HB_UNITYADS
            if(Advertisement.Banner.isLoaded) {
                Advertisement.Banner.Show(settings.GetBannerPlacement());
            }
            else  {
                Invoke(nameof(RequestBannerAds),2F);
            }
#endif
        }

        // Hides banner ad.
        public override void HideBanner()
        {
#if HB_UNITYADS
            Advertisement.Banner.Hide();
#endif
        }

        // Check if interstitial ad ready to show.
        public override bool IsInterstitialAvailable() { return isInterstitialReady; }

        // Shows interstitial ad if available.
        public override void ShowInterstitial()
        {
#if HB_UNITYADS
            Advertisement.Show(settings.GetInterstitialPlacement(), this);
#endif
        }
        // Checks if rewarded ad ready to show.
        public override bool IsRewardedAvailable() { return isRewardedReady; }

        // Shows rewarded ad if loaded.
        public override void ShowRewarded()
        {
#if HB_UNITYADS
            Advertisement.Show(settings.GetRewardedPlacement(), this);
#endif
        }

        #region  Banner Ad Callback
        //Banner ad  event callbacks.
        public void OnBannerLoaded()
        {
            if (AdManager.Instance.adSettings.showBannerOnLoad || showBannerCalled)
            {
#if HB_UNITYADS
                Advertisement.Banner.Show(settings.GetBannerPlacement()); 
#endif
            }
            AdManager.Instance.OnBannerLoaded();
        }

        public void OnBannerLoadFailed(string message)
        {
            Invoke(nameof(RequestBannerAds), 5F);
            AdManager.Instance.OnBannerLoadFailed(message);
        }
        #endregion

#if HB_UNITYADS
        public void OnUnityAdsAdLoaded(string placementId) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                AdManager.Instance.OnInterstitialLoaded();
                isInterstitialReady = true;
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                AdManager.Instance.OnRewardedLoaded();
                isRewardedReady = true;
            }
        }
        
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                AdManager.Instance.OnInterstitialLoadFailed(message);
                isInterstitialReady = false;
                RequestInterstitial();
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                AdManager.Instance.OnRewardedLoadFailed(message);
                isRewardedReady = false;
                RequestRewarded();
            }
        }
        
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                AdManager.Instance.OnInterstitialLoadFailed(message);
                isInterstitialReady = false;
                RequestInterstitial();
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                AdManager.Instance.OnRewardedLoadFailed(message);
                isRewardedReady = false;
                RequestRewarded();
            }
        }
        
        public void OnUnityAdsShowStart(string placementId) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                AdManager.Instance.OnInterstitialShown();
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                AdManager.Instance.OnRewardedShown();
            }
        }
        
        public void OnUnityAdsShowClick(string placementId) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                
            }        
        }
        
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState) {
            if (placementId.Equals(settings.GetInterstitialPlacement())) {
                RequestInterstitial();
            } else if (placementId.Equals(settings.GetRewardedPlacement())) {
                AdManager.Instance.OnRewardedAdRewarded();
                RequestRewarded();
            }
        }
#endif
    }
}
