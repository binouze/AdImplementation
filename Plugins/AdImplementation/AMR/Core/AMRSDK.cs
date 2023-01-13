using System;
using UnityEngine;
using AMR;

namespace AMR
{
	public class AMRSDK
	{
        public const string AMR_PLUGIN_VERSION = "1.7.3"; 
	    
	    public delegate void VirtualCurrencyDelegateDidSpend(string network, string currency, double amount);
        public delegate void SDKInitializeDelegateDidInitialize(bool isInitialized, string errorMessage);

        public delegate void TrackPurchaseOnResult(string purchaseId, AMR.Enums.AMRSDKTrackPurchaseResult responseCode);
        public delegate void GDPRIsApplicable(bool isGDPRApplicable);
        public delegate void PrivacyConsentRequired(string consentType);

        private class VirtualCurrencyDelegate : AMRVirtualCurrencyDelegate
	    {
	        private AMRSDK amrSdk;
	        public VirtualCurrencyDelegate(AMRSDK asdk)
	        {
	            amrSdk = asdk;
	        }

	        public void didSpendVirtualCurrency(string network, string currency, double amount)
	        {
	            if (amrSdk.onDidSpendDelegate != null) {
	                amrSdk.onDidSpendDelegate(network, currency, amount);
	            }
	        }
	    }

        private class SDKInitializeDelegate : AMRInitializeDelegate
        {
            private AMRSDK amrSdk;

            public SDKInitializeDelegate(AMRSDK asdk)
            {
                amrSdk = asdk;
            }

            public void didSDKInitialize(bool isInitialized, string errorMessage)
            {
                if (amrSdk.onDidSDKInitializeDelegate != null)
                {
                    amrSdk.onDidSDKInitializeDelegate(isInitialized, errorMessage);
                }
            }
        }

        private class TrackPurchaseDelegate : AMRTrackPurchaseDelegate
        {
            private AMRSDK amrSdk;
            public TrackPurchaseDelegate(AMRSDK asdk)
            {
                amrSdk = asdk;
            }

            public void onResult(string purchaseId, AMR.Enums.AMRSDKTrackPurchaseResult responseCode)
            {
                if (amrSdk.onTrackPurchaseOnResult != null)
                {
                    amrSdk.onTrackPurchaseOnResult(purchaseId, responseCode);
                }
            }
        }

        private class GDPRDelegate : AMRGDPRDelegate
        {
            private AMRSDK amrSdk;
            public GDPRDelegate(AMRSDK asdk)
            {
                amrSdk = asdk;
            }

            public void isGDPRApplicable(bool isGDPRApplicable)
            {
                if (amrSdk.isGDPRApplicable != null)
                {
                    amrSdk.isGDPRApplicable(isGDPRApplicable);
                }
            }
        }

        private class PrivacyConsentDelegate : AMRPrivacyConsentDelegate
        {
            private AMRSDK amrSdk;
            public PrivacyConsentDelegate(AMRSDK asdk)
            {
                amrSdk = asdk;
            }

            public void privacyConsentRequired(string consentType)
            {
                if (amrSdk.privacyConsentRequired != null)
                {
                    amrSdk.privacyConsentRequired(consentType);
                }
            }
        }

        private IAMRSdk AMRSdk;
        private AMRSdkConfig Config;
	    private VirtualCurrencyDelegateDidSpend onDidSpendDelegate;
        private SDKInitializeDelegateDidInitialize onDidSDKInitializeDelegate;

        private TrackPurchaseOnResult onTrackPurchaseOnResult;
        private GDPRIsApplicable isGDPRApplicable;
        private PrivacyConsentRequired privacyConsentRequired;
        private bool isInitialized = false;
        private static AMRSDK instance;
        private static AMRSDK Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMRSDK();
                }
                return instance;
            }
        }

        private void create()
        {
            if (AMRSdk != null)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRSdk = new iOS.AMRInitialize();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                AMRSdk = new Android.AMRInitialize();
            }
            else
            {
                AMRSdk = null;
            }
        }

        private void startWithAppId(SDKInitializeDelegateDidInitialize onDidInitializeDelegate, string appIdiOS, string appIdAndroid, string isUserChild, bool isHuaweiApp = false)
		{
            create();
            setOnSDKDidInitialize(onDidInitializeDelegate);

            if (Application.platform == RuntimePlatform.IPhonePlayer) 
            {
				AMRSdk.startWithAppId(appIdiOS, isUserChild != null && isUserChild.Equals("1"));
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                AMRSdk.startWithAppId(appIdAndroid, null, null, null, isUserChild, isHuaweiApp);
            }
		}
		
		private void startWithAppIdConsent(SDKInitializeDelegateDidInitialize onDidInitializeDelegate, string appIdiOS, string appIdAndroid, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp = false)
        {
            create();
            setOnSDKDidInitialize(onDidInitializeDelegate);

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRSdk.startWithAppIdConsent(appIdiOS, subjectToGDPR, subjectToCCPA, userConsent, isUserChild != null && isUserChild.Equals("1"));
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                AMRSdk.startWithAppId(appIdAndroid, subjectToGDPR, subjectToCCPA, userConsent,isUserChild, isHuaweiApp);
            }
        }

        public static void startWithConfig(AMRSdkConfig config)
        {
            AMRUtil.Log("AMR Plugin Version: [" + AMR_PLUGIN_VERSION + "]");

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
                Instance.Config = config;
                if (config.SubjectToGDPR != null || config.UserConsent != null || config.SubjectToCCPA != null)
                {
                    Instance.startWithAppIdConsent(null, config.ApplicationIdIOS, config.ApplicationIdAndroid, config.SubjectToGDPR, config.SubjectToCCPA, config.UserConsent, config.IsUserChild, config.IsHuaweiApp);
                } else { 
                    Instance.startWithAppId(null, config.ApplicationIdIOS, config.ApplicationIdAndroid, config.IsUserChild, config.IsHuaweiApp);
                }
                Instance.isInitialized = true;
            } else {
                AMRUtil.Log("AMRSDK only supports Android and iOS platforms.");
            }
        }

        public static void startWithConfig(AMRSdkConfig config, SDKInitializeDelegateDidInitialize onDidInitializeDelegate)
        {
            AMRUtil.Log("AMR Plugin Version: [" + AMR_PLUGIN_VERSION + "]");

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Instance.Config = config;
                if (config.SubjectToGDPR != null || config.UserConsent != null || config.SubjectToCCPA != null)
                {
                    Instance.startWithAppIdConsent(onDidInitializeDelegate, config.ApplicationIdIOS, config.ApplicationIdAndroid, config.SubjectToGDPR, config.SubjectToCCPA, config.UserConsent, config.IsUserChild, config.IsHuaweiApp);
                }
                else
                {
                    Instance.startWithAppId(onDidInitializeDelegate, config.ApplicationIdIOS, config.ApplicationIdAndroid, config.IsUserChild, config.IsHuaweiApp);
                }
                Instance.isInitialized = true;
            }
            else
            {
                AMRUtil.Log("AMRSDK only supports Android and iOS platforms.");
            }
        }

		public static bool initialized() {
			return Instance.isInitialized;
		}

		public static void startTestSuite(string[] zoneIds)
		{
            if (initialized()) {
                Instance.AMRSdk.startTestSuite(zoneIds);   
            } else {
                //AMRUtil.Log("AMRSDK has not been initialized.");
            }
		}

        public static void setGDPRIsApplicable(GDPRIsApplicable isGDPRApplicable)
        {
            if (!initialized())
            {
                Instance.create();
            }

            if (Instance.AMRSdk == null)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                Instance.isGDPRApplicable = isGDPRApplicable;
                GDPRDelegate gdprDelegate = new GDPRDelegate(Instance);
                Instance.AMRSdk.setGDPRDelegate(gdprDelegate);
            }
        }

        public static void setPrivacyConsentRequired(PrivacyConsentRequired privacyConsentRequired)
        {
            if (!initialized())
            {
                Instance.create();
            }

            if (Instance.AMRSdk == null)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                Instance.privacyConsentRequired = privacyConsentRequired;
                PrivacyConsentDelegate privacyConsentDelegate = new PrivacyConsentDelegate(Instance);
                Instance.AMRSdk.setPrivacyConsentDelegate(privacyConsentDelegate);
            }
        }

        public static string trackPurchaseForAndroid(string receipt, decimal localizedPrice, string isoCurrencyCode)
        {
			if (receipt == null || 
                receipt.Equals ("") ||
			    isoCurrencyCode == null || 
                isoCurrencyCode.Equals ("")) {
                //AMRUtil.Log("Track Purchase failed: null or empty parameters");
                return "";
			}

            return Instance.AMRSdk.trackPurchase(receipt, Convert.ToDouble(localizedPrice), isoCurrencyCode);
        }

        public static string trackIAPForAndroid(string receipt, decimal localizedPrice, string isoCurrencyCode, string[] tags)
        {
            if (receipt == null ||
                receipt.Equals("") ||
                isoCurrencyCode == null ||
                isoCurrencyCode.Equals(""))
            {
                //AMRUtil.Log("Track Purchase failed: null or empty parameters");
                return "";
            }

            return Instance.AMRSdk.trackIAP(receipt, Convert.ToDouble(localizedPrice), isoCurrencyCode, tags);
        }

        public static string trackPurchaseForAmazon(string userId, string receiptId, decimal localizedPrice, string marketplace, string isoCurrencyCode)
        {
            if (receiptId == null || receiptId.Equals("") || 
                userId == null || userId.Equals("") ||
                (
                (marketplace == null || marketplace.Equals("")) && 
                (isoCurrencyCode == null || isoCurrencyCode.Equals(""))
                ))
            {
                //AMRUtil.Log("Track Purchase failed: null or empty parameters");
                return "";
            }

            return Instance.AMRSdk.trackPurchaseForAmazon(userId, receiptId, Convert.ToDouble(localizedPrice), marketplace, isoCurrencyCode);
        }

        public static string trackIAPForHuawei(string receipt, string signature, string[] tags)
        {
            if (receipt == null ||
                receipt.Equals("") ||
                signature == null ||
                signature.Equals(""))
            {
                //AMRUtil.Log("trackIAPForHuawei failed: null or empty parameters");
                return "";
            }

            return Instance.AMRSdk.trackIAPForHuawei(receipt, signature, tags);
        }

        public static string trackPurchaseForIOS(string transactionID, decimal localizedPrice, string isoCurrencyCode)
        {
            if (transactionID == null ||
                transactionID.Equals("") ||
                isoCurrencyCode == null ||
                isoCurrencyCode.Equals(""))
            {
                //AMRUtil.Log("Track Purchase failed: null or empty parameters");
                return "";
            }

            return Instance.AMRSdk.trackPurchase(transactionID, Convert.ToDouble(localizedPrice), isoCurrencyCode);
        }

        public static string trackIAPForIOS(string transactionID, decimal localizedPrice, string isoCurrencyCode, string[] tags)
        {
            if (transactionID == null ||
                transactionID.Equals("") ||
                isoCurrencyCode == null ||
                isoCurrencyCode.Equals(""))
            {
                //AMRUtil.Log("Track IAP failed: null or empty parameters");
                return "";
            }

            return Instance.AMRSdk.trackIAP(transactionID, Convert.ToDouble(localizedPrice), isoCurrencyCode, tags);
        }

        public static void setTrackPurchaseOnResult(TrackPurchaseOnResult onResultDelegate)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                Instance.onTrackPurchaseOnResult = onResultDelegate;
                TrackPurchaseDelegate trackPurchaseDelegate = new TrackPurchaseDelegate(Instance);
                Instance.AMRSdk.setTrackPurchaseDelegate(trackPurchaseDelegate);
            }
        }

        public static void setUserId(string userId)
        {
            if (initialized())
            {
                Instance.AMRSdk.setUserId(userId);
            }
            else
            {
                AMRUtil.Log("AMRSDK has not been initialized.");
            }
        }

        public static void setAdjustUserId(string adjustUserId)
        {
            if (initialized())
            {
                Instance.AMRSdk.setAdjustUserId(adjustUserId);
            }
            else
            {
                AMRUtil.Log("AMRSDK has not been initialized.");
            }
        }

        public static void setClientCampaignId(string campaignId)
		{
			if (initialized())
			{
				if (!String.IsNullOrEmpty(campaignId))
				{
					Instance.AMRSdk.setClientCampaignId(campaignId);
				}
				else
				{
					//AMRUtil.Log("campaignId is null or empty!");
				}
			}
			else
			{
				AMRUtil.Log("AMRSDK has not been initialized.");
			}
			
		}

		public static void spendVirtualCurrency()
	    {
	        if (initialized())
	        {
	            Instance.AMRSdk.spendVirtualCurrency();
	        }
	        else
	        {
	            AMRUtil.Log("AMRSDK has not been initialized.");
	        }
	    }

        public static void onPause()
        {
	        if (!initialized()) return;
	        Instance.AMRSdk.pause();
        }

        public static void onResume()
        {
            if (!initialized()) return;
	        Instance.AMRSdk.resume();
        }

        public static void onStop()
        {
            if (!initialized()) return;
            Instance.AMRSdk.stop();
        }

        public static void onStart()
        {
            if (!initialized()) return;
            Instance.AMRSdk.start();
        }

        public static void onDestroy()
        {
            if (!initialized()) return;
            Instance.AMRSdk.destroy();
        }

        #region Banner
		public static void loadBanner(AMR.Enums.AMRSDKBannerPosition position, bool autoShow) 
		{
		    if (!initialized()) return;
		    AMRBannerView.Instance.loadBannerForZoneId(Instance.Config.BannerIdIOS,
		                                                Instance.Config.BannerIdAndroid,
		                                                position,
                                                        0,
		                                                autoShow);
            
        }

        public static void loadBanner(AMR.Enums.AMRSDKBannerPosition position, int offset, bool autoShow)
        {
            if (!initialized()) return;
            AMRBannerView.Instance.loadBannerForZoneId(Instance.Config.BannerIdIOS,
                                                        Instance.Config.BannerIdAndroid,
                                                        position,
                                                        offset,
                                                        autoShow);

        }

        public static void loadBannerWithPosition(double positionX, double positionY, bool autoShow)
        {
            if (!initialized()) return;
            AMRBannerView.Instance.loadBannerForZoneId(Instance.Config.BannerIdIOS,
                                                        Instance.Config.BannerIdAndroid,
                                                        positionX,
                                                        positionY,
                                                        autoShow);
        }

        public static void showBanner()
		{
			AMRBannerView.Instance.showBanner(true);
		}

        public static void hideBanner()
        {
            AMRBannerView.Instance.hideBanner(true);
        }

        public static void destroyBanner()
        {
            AMRBannerView.Instance.destroyBanner();
        }

        public static void resolveBannerConflict()
        {
            AMRBannerView.Instance.resolveConflict();
        }

        public static void setOnBannerReady(AMRAdView.EventDelegateReady onReadyDelegate)
        {
            AMRBannerView.Instance.setDidReceiveBanner(onReadyDelegate);
        }

        public static void setOnBannerFail(AMRAdView.EventDelegateFail onFailDelegate)
        {
            AMRBannerView.Instance.setDidFailToReceiveBanner(onFailDelegate);
        }

        public static void setOnBannerImpression(AMRAdView.EventDelegateImpression onImpressionDelegate)
        {
            AMRBannerView.Instance.setDidImpressionBanner(onImpressionDelegate);
        }

        public static void setOnBannerClick(AMRAdView.EventDelegateClick onClickDelegate)
        {
            AMRBannerView.Instance.setDidClickBanner(onClickDelegate);
        }

        #endregion

        #region Interstitial

        public static void loadInterstitial()
        {
            if (!Instance.isInitialized)
            {
                return;
            }
            AMRInterstitialView.Instance.loadInterstitialForZoneId(Instance.Config.InterstitialIdIOS,
                                       Instance.Config.InterstitialIdAndroid,false);
        }
		
		public static void showInterstitial()
		{
			if (isInterstitialReady())
			{
				AMRInterstitialView.Instance.showInterstitial();
			}
		}

        public static void showInterstitial(String tag)
        {
            if (isInterstitialReady())
            {
                AMRInterstitialView.Instance.showInterstitial(tag);
            }
        }

        public static void refreshInterstitial()
        {
            if (!Instance.isInitialized)
            {
                return;
            }
            AMRInterstitialView.Instance.loadInterstitialForZoneId(Instance.Config.InterstitialIdIOS,
                                       Instance.Config.InterstitialIdAndroid, true);
        }
		
		public static Boolean isInterstitialReady()
		{
			return AMRInterstitialView.Instance.isReady();
		}
		
		public static Boolean isInterstitialShowing()
		{
			return AMRInterstitialView.Instance.isShowing();
		}

        public static Boolean isInterstitialReadyToShow()
        {
            return AMRInterstitialView.Instance.isReadyToShow();
        }

        public static void setOnInterstitialReady(AMRAdView.EventDelegateReady onReadyDelegate)
        {
            AMRInterstitialView.Instance.setOnReady(onReadyDelegate);
        }

        public static void setOnInterstitialFail(AMRAdView.EventDelegateFail onFailDelegate)
        {
            AMRInterstitialView.Instance.setOnFail(onFailDelegate);
        }

        public static void setOnInterstitialShow(AMRAdView.EventDelegateShow onShowDelegate)
        {
            AMRInterstitialView.Instance.setOnShow(onShowDelegate);
        }

        public static void setOnInterstitialFailToShow(AMRAdView.EventDelegateFailToShow onFailToShowDelegate)
        {
            AMRInterstitialView.Instance.setOnFailToShow(onFailToShowDelegate);
        }

        public static void setOnInterstitialClick(AMRAdView.EventDelegateClick onClickDelegate)
        {
            AMRInterstitialView.Instance.setOnClick(onClickDelegate);
        }

        public static void setOnInterstitialImpression(AMRAdView.EventDelegateImpression onImpressionDelegate)
        {
            AMRInterstitialView.Instance.setOnImpression(onImpressionDelegate);
        }

        public static void setOnInterstitialDismiss(AMRAdView.EventDelegateDismiss onDismissDelegate)
        {
            AMRInterstitialView.Instance.setOnDismiss(onDismissDelegate);
        }

        public static void setOnInterstitialStatusChange(AMRAdView.EventDelegateStatusChange onStatusChangeDelegate)
        {
            AMRInterstitialView.Instance.setOnStatusChange(onStatusChangeDelegate);
        }

        #endregion

        #region RewardedVideo

        public static void loadRewardedVideo()
        {
	        if (!initialized()) return;
            AMRRewardedVideoView.Instance.loadRewardedVideoForZoneId(Instance.Config.RewardedVideoIdIOS,
                                       Instance.Config.RewardedVideoIdAndroid, false);
        }
		
		public static void showRewardedVideo()
		{
			if (isRewardedVideoReady())
			{
				AMRRewardedVideoView.Instance.showRewardedVideo();
			}
		}
        
        public static void showRewardedVideo(String tag)
        {
            if (isRewardedVideoReady())
            {
                AMRRewardedVideoView.Instance.showRewardedVideo(tag);
            }
        }

        public static void refreshRewardedVideo()
        {
	        if (!initialized()) return; 
            AMRRewardedVideoView.Instance.loadRewardedVideoForZoneId(Instance.Config.RewardedVideoIdIOS,
                                       Instance.Config.RewardedVideoIdAndroid, true);
        }
		
		public static Boolean isRewardedVideoReady()
		{
			return AMRRewardedVideoView.Instance.isReady();
		}
		
		public static Boolean isRewardedVideoShowing()
		{
			return AMRRewardedVideoView.Instance.isPlaying();
		}

        public static Boolean isRewardedVideoReadyToShow()
        {
            return AMRRewardedVideoView.Instance.isReadyToShow();
        }

        public static void setOnRewardedVideoReady(AMRAdView.EventDelegateReady onReadyDelegate)
        {
            AMRRewardedVideoView.Instance.setOnReady(onReadyDelegate);
        }

        public static void setOnRewardedVideoFail(AMRAdView.EventDelegateFail onFailDelegate)
        {
            AMRRewardedVideoView.Instance.setOnFail(onFailDelegate);
        }

        public static void setOnRewardedVideoShow(AMRAdView.EventDelegateShow onShowDelegate)
        {
            AMRRewardedVideoView.Instance.setOnShow(onShowDelegate);
        }

		public static void setOnRewardedVideoFailToShow(AMRAdView.EventDelegateFailToShow onFailToShowDelegate)
		{
			AMRRewardedVideoView.Instance.setOnFailToShow(onFailToShowDelegate);
		}

        public static void setOnRewardedVideoClick(AMRAdView.EventDelegateClick onClickDelegate)
        {
            AMRRewardedVideoView.Instance.setOnClick(onClickDelegate);
        }

        public static void setOnRewardedVideoImpression(AMRAdView.EventDelegateImpression onImpressionDelegate)
        {
            AMRRewardedVideoView.Instance.setOnImpression(onImpressionDelegate);
        }

        public static void setOnRewardedVideoComplete(AMRAdView.EventDelegateComplete onCompleteDelegate)
        {
            AMRRewardedVideoView.Instance.setOnComplete(onCompleteDelegate);
        }

        public static void setOnRewardedVideoDismiss(AMRAdView.EventDelegateDismiss onDismissDelegate)
        {
            AMRRewardedVideoView.Instance.setOnDismiss(onDismissDelegate);
        }

        public static void setOnRewardedVideoStatusChange(AMRAdView.EventDelegateStatusChange onStatusChangeDelegate)
        {
            AMRRewardedVideoView.Instance.setOnStatusChange(onStatusChangeDelegate);
        }

        public static void setOnRewardedVideoReward(AMRAdView.EventDelegateReward onRewardDelegate)
        {
            AMRRewardedVideoView.Instance.setOnReward(onRewardDelegate);
        }

        #endregion

        #region OfferWall

        public static void loadOfferWall()
	    {
		    if (!initialized()) return;
	        
	        AMROfferWallView.Instance.loadOfferWallForZoneId(instance.Config.OfferWallIdIOS, instance.Config.OfferWallIdAndroid, false);
	    }
	    
	    public static void showOfferWall()
	    {
		    if (!initialized()) return;
	        
	        AMROfferWallView.Instance.showOfferWall();
	    }

        public static void showOfferWall(String tag)
        {
            if (!initialized()) return;

            AMROfferWallView.Instance.showOfferWall(tag);
        }

        public static void refreshOfferWall()
	    {
		    if (!initialized()) return;
	        
	        AMROfferWallView.Instance.loadOfferWallForZoneId(instance.Config.OfferWallIdIOS, instance.Config.OfferWallIdAndroid, true);
	    }
		
		public static Boolean isOfferWallReady()
		{
			return AMROfferWallView.Instance.isReady();
		}
		
		public static Boolean isOfferWallShowing()
		{
			return AMROfferWallView.Instance.isShowing();
		}

	    public static void setOnOfferWallReady(AMRAdView.EventDelegateReady onReadyDelegate)
	    {
	        AMROfferWallView.Instance.setOnReady(onReadyDelegate);
	    }
	    
	    public static void setOnOfferWallFail(AMRAdView.EventDelegateFail onFailDelegate)
	    {
	        AMROfferWallView.Instance.setOnFail(onFailDelegate);
	    }

	    public static void setOnOfferWallDismiss(AMRAdView.EventDelegateDismiss onDismissDelegate)
	    {
	        AMROfferWallView.Instance.setOnDismiss(onDismissDelegate);
	    }
	    
	    public static void setOnDidSpendVirtualCurrency(VirtualCurrencyDelegateDidSpend onDidSpendDelegate) {
		    if (Application.platform == RuntimePlatform.IPhonePlayer || 
		        Application.platform == RuntimePlatform.Android) 
		    {
			    Instance.onDidSpendDelegate = onDidSpendDelegate;
			    VirtualCurrencyDelegate virtualCurrencyDelegate = new VirtualCurrencyDelegate(Instance);
			    Instance.AMRSdk.setVirtualCurrencyDelegate(virtualCurrencyDelegate);
		    }
        }

        private static void setOnSDKDidInitialize(SDKInitializeDelegateDidInitialize onDidInitializeDelegate)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                Instance.onDidSDKInitializeDelegate = onDidInitializeDelegate;
                SDKInitializeDelegate initializeDelegate = new SDKInitializeDelegate(Instance);
                Instance.AMRSdk.setSDKInitializeDelegate(initializeDelegate);
            }
        }

        #endregion

        #region
        public static bool isFullScreenAdShowing()
        {
            return (isInterstitialShowing() || isRewardedVideoShowing() || isOfferWallShowing());
        }

        #endregion
    }

}
