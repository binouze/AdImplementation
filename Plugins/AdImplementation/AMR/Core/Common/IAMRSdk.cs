using System;
using System.Collections.Generic;

namespace AMR
{
	public interface IAMRSdk
	{
        void setApiHttps();
		void startWithAppId(string appId, bool isUserChild, string canRequestAds);
        void startWithAppIdConsent(string appId, string subjectToGDPR, string userConsent, bool isUserChild, string canRequestAds);
		void startWithAppIdConsent(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, bool isUserChild, string canRequestAds);
        void startWithAppId(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp, string canRequestAds);
        void pause();
        void resume();
        void start();
        void stop();
        void destroy();
		void startTestSuite(string[] zoneIds);
        void setUserId(string userId);
        void setAdjustUserId(string adjustUserId);
        void setClientCampaignId(string campaignId);
        void setCanRequestAds(bool canRequestAds);
        void setCustomVendors(Dictionary<string, bool> parameters);
        void setThirdPartyExperiment(string experiment, string group);
        string trackPurchase(string uniqueID, double localizedPrice, string isoCurrencyCode);
        string trackIAP(string uniqueID, double localizedPrice, string isoCurrencyCode, string[] tags);
        string trackPurchaseForAmazon(string userId, string receiptId, double localizedPrice, string marketPlace, string isoCurrencyCode);
        string trackIAPForHuawei(string uniqueID, string signature, string[] tags);
        void spendVirtualCurrency();
        void setVirtualCurrencyDelegate(AMRVirtualCurrencyDelegate delegateObject);
        void setSDKInitializeDelegate(AMRInitializeDelegate delegateObject);
        void setTrackPurchaseDelegate(AMRTrackPurchaseDelegate delegateObject);
        void setGDPRDelegate(AMRGDPRDelegate delegateObject);
        void setPrivacyConsentDelegate(AMRPrivacyConsentDelegate delegateObject);
        void setUnityMainThread();
    }
}

