using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR.iOS
{
    public class AMRInitialize : IAMRSdk
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
        private static extern void _setVirtualCurrencyDidSpendCallback(VirtualCurrencyDidSpendCallback cb, IntPtr virtualCurrencyHandle);

        [DllImport("__Internal")]
        private static extern void _setTrackPurchaseResponseCallback(TrackPurchaseResponseCallback cb, IntPtr trackPurchaseHandle);

        [DllImport("__Internal")]
        private static extern void _setIsGDPRApplicableCallback(IsGDPRApplicableCallback cb, IntPtr gdprHandle);

        [DllImport("__Internal")]
        private static extern void _setPrivacyConsentRequiredCallback(PrivacyConsentRequiredCallback cb, IntPtr consentStatus);

        [DllImport("__Internal")]
        private static extern void _setSDKInitializeCallback(SDKInitializeCallback cb, IntPtr initializeHandle);

#endif

        private delegate void VirtualCurrencyDidSpendCallback(IntPtr virtualCurrencyHandlePtr, string networkName, string currency, double amount);
        private delegate void TrackPurchaseResponseCallback(IntPtr trackPurchaseHandlePtr, string uniqueID, int status);
        private delegate void IsGDPRApplicableCallback(IntPtr gdprHandlePtr, bool isApplicable);
        private delegate void PrivacyConsentRequiredCallback(IntPtr privacyConsentHandlePtr, int consentStatus);
        private delegate void SDKInitializeCallback(IntPtr sdkInitializeHandlePtr, bool isInitialized, string errorMessage);

        public void start() {}
        public void stop() {}
        public void resume() {}
        public void pause() {}
        public void destroy() {}
        
        public void startWithAppId(string appId, bool isUserChild)
        {
#if UNITY_IOS
            if (isUserChild)
            {
                _setUserChild(isUserChild);
            }
            
            _startWithAppId(appId);
#endif
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string userConsent, bool isUserChild)
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
		
		public void startWithAppIdConsent(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, bool isUserChild)
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

        public void startWithAppId(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp)
        {
#if UNITY_IOS
            startWithAppIdConsent(appId, subjectToGDPR, subjectToCCPA, userConsent, isUserChild == "1");
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
        
        [MonoPInvokeCallback(typeof(VirtualCurrencyDidSpendCallback))]
        private static void virtualCurrencyDidSpendCallback(IntPtr virtualCurrencyHandlePtr, string networkName, string currency, double amount)
        {
            GCHandle virtualCurrencyVideoHandle = (GCHandle)virtualCurrencyHandlePtr;
            AMRVirtualCurrencyDelegate delegateObject = virtualCurrencyVideoHandle.Target as AMRVirtualCurrencyDelegate;
            delegateObject.didSpendVirtualCurrency(networkName, currency, amount);
        }

        public void setVirtualCurrencyDelegate(AMRVirtualCurrencyDelegate delegateObject)
        {
#if UNITY_IOS
            GCHandle handle = GCHandle.Alloc(delegateObject);
            IntPtr parameter = (IntPtr)handle;
            
            _setVirtualCurrencyDidSpendCallback(virtualCurrencyDidSpendCallback, parameter);
#endif
        }

        [MonoPInvokeCallback(typeof(TrackPurchaseResponseCallback))]
        private static void trackPurchaseResponseCallback(IntPtr trackPurchaseHandlePtr, string uniqueID, int status)
        {
            GCHandle trackPurchaseHandle = (GCHandle)trackPurchaseHandlePtr;
            AMRTrackPurchaseDelegate delegateObject = trackPurchaseHandle.Target as AMRTrackPurchaseDelegate;
            delegateObject.onResult(uniqueID, (AMR.Enums.AMRSDKTrackPurchaseResult)status);
        }


        public void setTrackPurchaseDelegate(AMRTrackPurchaseDelegate delegateObject)
        {
#if UNITY_IOS
            GCHandle handle = GCHandle.Alloc(delegateObject);
            IntPtr parameter = (IntPtr)handle;
            
            _setTrackPurchaseResponseCallback(trackPurchaseResponseCallback, parameter);

#endif
        }
        
        [MonoPInvokeCallback(typeof(IsGDPRApplicableCallback))]
        private static void isGDPRApplicableCallback(IntPtr gdprHandlePtr, bool isApplicable)
        {
            GCHandle gdprHandle = (GCHandle)gdprHandlePtr;
            AMRGDPRDelegate delegateObject = gdprHandle.Target as AMRGDPRDelegate;
            delegateObject.isGDPRApplicable(isApplicable);
        }

        [MonoPInvokeCallback(typeof(PrivacyConsentRequiredCallback))]
        private static void privacyConsentRequiredCallback(IntPtr privacyConsentHandlePtr, int consentStatus)
        {
            string cStatus = "None";

            if (consentStatus == 1)
            {
                cStatus = "GDPR";
            } else if (consentStatus == 2)
            {
                cStatus = "CCPA";
            }

            GCHandle privacyConsentHandle = (GCHandle)privacyConsentHandlePtr;

            AMRPrivacyConsentDelegate delegateObject = privacyConsentHandle.Target as AMRPrivacyConsentDelegate;
            delegateObject.privacyConsentRequired(cStatus);
        }

        [MonoPInvokeCallback(typeof(SDKInitializeCallback))]
        private static void sdkInitializeCallback(IntPtr sdkInitializeHandlePtr, bool isInitialized, string errorMessage)
        {
            GCHandle initCallbackHandle = (GCHandle)sdkInitializeHandlePtr;

            AMRInitializeDelegate delegateObject = initCallbackHandle.Target as AMRInitializeDelegate;
            delegateObject.didSDKInitialize(isInitialized, errorMessage);
        }


        public void setGDPRDelegate(AMRGDPRDelegate delegateObject)
        {
#if UNITY_IOS
            GCHandle handle = GCHandle.Alloc(delegateObject);
            IntPtr parameter = (IntPtr)handle;
            
            _setIsGDPRApplicableCallback(isGDPRApplicableCallback, parameter);
#endif
        }

        public void setPrivacyConsentDelegate(AMRPrivacyConsentDelegate delegateObject)
        {
#if UNITY_IOS
            GCHandle handle = GCHandle.Alloc(delegateObject);
            IntPtr parameter = (IntPtr)handle;

            _setPrivacyConsentRequiredCallback(privacyConsentRequiredCallback, parameter);
#endif
        }

        public void setSDKInitializeDelegate(AMRInitializeDelegate delegateObject)
        {
#if UNITY_IOS
            GCHandle handle = GCHandle.Alloc(delegateObject);
            IntPtr parameter = (IntPtr)handle;

            _setSDKInitializeCallback(sdkInitializeCallback, parameter);
#endif
        }

        public string trackIAPForHuawei(string uniqueID, string signature, string[] tags)
        {
            return "";
        }
    }
}
