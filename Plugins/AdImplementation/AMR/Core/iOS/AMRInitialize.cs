using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AMR.iOS
{
    public class AMRInitialize : MonoBehaviour, IAMRSdk
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _startWithAppId(string appID);
        
        [DllImport("__Internal")]
        private static extern void _startTestSuite(string zoneIds);

        [DllImport("__Internal")]
        private static extern void _trackPurchase(string identifier, string currencyCode, double amount);

        [DllImport("__Internal")]
        private static extern void _trackIAP(string identifier, string currencyCode, double amount, string tags);

        [DllImport("__Internal")]
        private static extern void _setUserId(string userId);

        [DllImport("__Internal")]
        private static extern void _setAdjustUserId(string adjustUserId);
        
        [DllImport("__Internal")]
        private static extern void _setCanRequestAds(bool canRequestAds);
        
        [DllImport("__Internal")]
        private static extern void _setCustomVendors(string jsonParams);

        [DllImport("__Internal")]
        private static extern void _setClientCampaignId(string campaignId);
        
        [DllImport("__Internal")]
        private static extern void _setUserConsent(bool consent);
        
        [DllImport("__Internal")]
        private static extern void _subjectToGDPR(bool subject);

        [DllImport("__Internal")]
        private static extern void _subjectToCCPA(bool subject);

        [DllImport("__Internal")]
        private static extern void _setUserChild(bool userChild);

        [DllImport("__Internal")]
        private static extern void _spendVirtualCurrency();
        
        [DllImport("__Internal")]
        private static extern void _setVirtualCurrencyDidSpendCallback();

        [DllImport("__Internal")]
        private static extern void _setTrackPurchaseResponseCallback();
        
        [DllImport("__Internal")]
        private static extern void _setIsGDPRApplicableCallback();
        
        [DllImport("__Internal")] 
        private static extern void _setPrivacyConsentRequiredCallback();
#endif
        
        #region Singleton
        private AMRInitializeDelegate sdkInitDelegate;
        private AMRGDPRDelegate gdprDelegate;
        private AMRPrivacyConsentDelegate privacyConsentDelegate;
        private AMRTrackPurchaseDelegate trackPurchaseDelegate;
        private AMRVirtualCurrencyDelegate virtualCurrencyDelegate;
        
        private Dictionary<string, AMRRewardedVideoViewDelegate> delegates = new Dictionary<string, AMRRewardedVideoViewDelegate>();
        private static AMRInitialize _instance;
        public static AMRInitialize Instance
        {
            get {
                if (_instance == null) {
                    var obj = new GameObject("AMRInitialize");
                    _instance = obj.AddComponent<AMRInitialize>();
                }
                return _instance;
            }
        }

        private void Awake() {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region iOS Events

        public void SDKInitializeEvent(string stringParams) {
            AMRUtil.Log("Event: SDKInitializeEvent, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);
            if (parameters.Length < 2) { return; }
            
            bool isInitialized = Convert.ToInt16(parameters[0]) == 1;
            var errorMessage = parameters[1];
            sdkInitDelegate?.didSDKInitialize(isInitialized, errorMessage);
        }
        
        public void IsGdprApplicableEvent(string stringParams) {
            AMRUtil.Log("Event: IsGdprApplicableEvent, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);
            if (parameters.Length < 1) { return; }
            
            bool isApplicable = Convert.ToInt16(parameters[0]) == 1;
            gdprDelegate?.isGDPRApplicable(isApplicable);
        }
        
        public void PrivacyConsentRequiredEvent(string stringParams) {
            AMRUtil.Log("Event: PrivacyConsentRequiredEvent, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);
            if (parameters.Length < 1) { return; }
            
            int consentStatus = Convert.ToInt16(parameters[0]);
            var cStatus = "None";

            if (consentStatus == 1) {
                cStatus = "GDPR";
            } else if (consentStatus == 2) {
                cStatus = "CCPA";
            }
            
            privacyConsentDelegate?.privacyConsentRequired(cStatus);
        }
        
        public void TrackPurchaseResponseEvent(string stringParams) {
            AMRUtil.Log("Event: TrackPurchaseResponseEvent, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);
            if (parameters.Length < 2) { return; }
            
            var uniqueID = parameters[0];
            int status = Convert.ToInt16(parameters[1]);
            trackPurchaseDelegate?.onResult(uniqueID, (Enums.AMRSDKTrackPurchaseResult)status);
        }
        
        public void VirtualCurrencyDidSpendEvent(string stringParams) {
            AMRUtil.Log("Event: VirtualCurrencyDidSpendEvent, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);
            if (parameters.Length < 3) { return; }
            
            var networkName = parameters[0];
            var currency = parameters[1];
            var amount = Convert.ToDouble(parameters[2]);
            virtualCurrencyDelegate?.didSpendVirtualCurrency(networkName, currency, amount);
        }
        
        #endregion
        
        public void startWithAppId(string appId, bool isUserChild, string canReqeustAds)
        {
#if UNITY_IOS
            if (isUserChild)
            {
                _setUserChild(isUserChild);
            }

            setCanRequestAds(canReqeustAds == "1");
            _startWithAppId(appId);
#endif
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string userConsent, bool isUserChild, string canReqeustAds)
        {
#if UNITY_IOS
            if (!string.IsNullOrEmpty(userConsent))
            {
                _setUserConsent(userConsent == "1");
            }
                
            if (!string.IsNullOrEmpty(subjectToGDPR))
            {
                _subjectToGDPR(subjectToGDPR == "1");
            }
            
            if (isUserChild == true)
            {
                _setUserChild(isUserChild);
            }
            
            _startWithAppId(appId);
#endif
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, bool isUserChild, string canReqeustAds)
        {
#if UNITY_IOS
            if (!string.IsNullOrEmpty(userConsent))
            {
                _setUserConsent(userConsent == "1");
            }
                
            if (!string.IsNullOrEmpty(subjectToGDPR))
            {
                _subjectToGDPR(subjectToGDPR == "1");
            }

            if (!string.IsNullOrEmpty(subjectToCCPA))
            {
                _subjectToCCPA(subjectToCCPA == "1");
            }
            
            if (isUserChild == true)
            {
                _setUserChild(isUserChild);
            }
            
            _startWithAppId(appId);
#endif
        }

        public void startWithAppId(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp, string canReqeustAds)
        {
#if UNITY_IOS
            startWithAppIdConsent(appId, subjectToGDPR, subjectToCCPA, userConsent, isUserChild == "1", canReqeustAds);
#endif
        }

        public void startTestSuite(string[] zoneIds)
		{
#if UNITY_IOS
			string zones = "";
			for (int i = 0; i < zoneIds.Length; i++) {
				zones = zones + "," + zoneIds [i];
			}

			_startTestSuite(zones);
#endif
		}

        public string trackPurchase(string uniqueID, double localizedPrice, string isoCurrencyCode)
        {
#if UNITY_IOS
            /* uniqueID = transactionID for ios */
            _trackPurchase(uniqueID, isoCurrencyCode, localizedPrice);
#endif
            return uniqueID;
        }

        public string trackIAP(string uniqueID, double localizedPrice, string isoCurrencyCode, string[] tags)
        {
#if UNITY_IOS
            string tagsString = "";
            for (int i = 0; i < tags.Length; i++)
            {
                tagsString = tagsString + "," + tags[i];
            }

            /* uniqueID = transactionID for ios */
            _trackIAP(uniqueID, isoCurrencyCode, localizedPrice, tagsString);
#endif
            return uniqueID;
        }

        public string trackPurchaseForAmazon(string userId, string receiptId, double localizedPrice, string marketPlace, string isoCurrencyCode)
        {
            return "";
        }

        public void setUserId(string userId)
        {
#if UNITY_IOS
            _setUserId(userId);
#endif
        }

        public void setAdjustUserId(string adjustUserId)
        {
#if UNITY_IOS
            _setAdjustUserId(adjustUserId);
#endif
        }

        public void setCanRequestAds(bool canRequestAds)
        {
#if UNITY_IOS
            _setCanRequestAds(canRequestAds);
#endif
        }

        public void setCustomVendors(Dictionary<string, bool> parameters)
        {
#if UNITY_IOS
            var jsonParams = AMRUtil.BoolDictionaryToJson(parameters);
            if (jsonParams != null) {
                _setCustomVendors(jsonParams);
            } else {
                Debug.Log("<Admost> Missing custom vendors!");
            }
#endif
        }

        public void setThirdPartyExperiment(string experiment, string group)
        {
#if UNITY_IOS
            
#endif
        }

        public void setClientCampaignId(string campaignId)
        {
#if UNITY_IOS
            _setClientCampaignId(campaignId);
#endif
        }

        public void setUserConsent(bool consent)
        {
#if UNITY_IOS
            _setUserConsent(consent);
#endif
        }
        
        public void subjectToGDPR(bool subject)
        {
#if UNITY_IOS
            _subjectToGDPR(subject);
#endif
        }
		
		public void subjectToCCPA(bool subject)
        {
#if UNITY_IOS
            _subjectToCCPA(subject);
#endif
        }

        public void spendVirtualCurrency()
        {
#if UNITY_IOS
            _spendVirtualCurrency();
#endif
        }
        
        public void setSDKInitializeDelegate(AMRInitializeDelegate delegateObject)
        {
#if UNITY_IOS
            sdkInitDelegate = delegateObject;
#endif
        }
        
        public void setGDPRDelegate(AMRGDPRDelegate delegateObject)
        {
#if UNITY_IOS
            gdprDelegate = delegateObject;
            _setIsGDPRApplicableCallback();
#endif
        }
        
        public void setPrivacyConsentDelegate(AMRPrivacyConsentDelegate delegateObject)
        {
#if UNITY_IOS
            privacyConsentDelegate = delegateObject;
            _setPrivacyConsentRequiredCallback();
#endif
        }
        
        public void setTrackPurchaseDelegate(AMRTrackPurchaseDelegate delegateObject)
        {
#if UNITY_IOS
            trackPurchaseDelegate = delegateObject;
            _setTrackPurchaseResponseCallback();
#endif
        }
        
        public void setVirtualCurrencyDelegate(AMRVirtualCurrencyDelegate delegateObject)
        {
#if UNITY_IOS
            virtualCurrencyDelegate = delegateObject;
            _setVirtualCurrencyDidSpendCallback();
#endif
        }

        public string trackIAPForHuawei(string uniqueID, string signature, string[] tags) { return ""; }
        public void setUnityMainThread() { }
        public int getDeviceScore() {
            return 100;
        }

        public void pause() { }
        public void resume() { }

        public void start() { }
        public void stop() { }
        public void destroy() { }
        public void setApiHttps() { }
    }
}
