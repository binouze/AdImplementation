using System;
using System.Collections.Generic;
using UnityEngine;

namespace AMR.Core.UnityEditor
{
    public class AMRInitialize : IAMRSdk
    {
        private AMRInitializeDelegate sdkInitDelegate;

        public void disableAppharbr() { }
        public void setApiHttps() { }

        public void startWithAppId(string appId, bool isUserChild, string canRequestAds)
        {
            startWithAppId(appId);
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string userConsent, bool isUserChild, string canRequestAds)
        {
            startWithAppId(appId);
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, bool isUserChild, string canRequestAds)
        {
            startWithAppId(appId);
        }

        public void startWithAppId(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp, string canRequestAds)
        {
            startWithAppId(appId);
        }

        private void startWithAppId(string appId)
        {
            AMR.AMRRemoteConfig.fetch(null, appId, () =>
            {
                sdkInitDelegate.didSDKInitialize(true, null);
            });
        }

        public void pause() { }
        public void resume() { }
        public void start() { }
        public void stop() { }
        public void destroy() { }
        public void startTestSuite(string[] zoneIds) { }
        public void setUserId(string userId) { }
        public void setAdjustUserId(string adjustUserId) { }
        public void setClientCampaignId(string campaignId) { }
        public void setCanRequestAds(bool canRequestAds) { }
        public void setCustomVendors(Dictionary<string, bool> parameters) { }
        public void setThirdPartyExperiment(string experiment, string group) { }

        public string trackPurchase(string uniqueID, double localizedPrice, string isoCurrencyCode)
        {
            return null;
        }

        public string trackIAP(string uniqueID, double localizedPrice, string isoCurrencyCode, string[] tags, bool isDebug = false)
        {
            return null;
        }

        public string trackPurchaseForAmazon(string userId, string receiptId, double localizedPrice, string marketPlace, string isoCurrencyCode)
        {
            return null;
        }

        public string trackIAPForHuawei(string uniqueID, string signature, string[] tags)
        {
            return null;
        }

        public void trackEvent(string eventName, Dictionary<string, string> parameters) { }
        public void trackEvent(string eventName, Dictionary<string, string> parameters, string currency, double value) { }
        public void trackLog(string eventName, Dictionary<string, string> parameters) { }
        public void trackScreenView(string screenName) { }

        public void spendVirtualCurrency() { }
        public void setVirtualCurrencyDelegate(AMRVirtualCurrencyDelegate delegateObject) { }

        public void setSDKInitializeDelegate(AMRInitializeDelegate delegateObject)
        {
            if (AMRUtil.IsPlatformEditor())
            {
                sdkInitDelegate = delegateObject;
            }
        }

        public void setTrackPurchaseDelegate(AMRTrackPurchaseDelegate delegateObject) { }
        public void setGDPRDelegate(AMRGDPRDelegate delegateObject) { }
        public void setPrivacyConsentDelegate(AMRPrivacyConsentDelegate delegateObject) { }
        public void setUnityMainThread() { }

        public int getDeviceScore()
        {
            return 0;
        }

        public void trackAdmobMediationRevenue(string adFormat, double revenue, string placementId, string adUnitId) { }
    }
}