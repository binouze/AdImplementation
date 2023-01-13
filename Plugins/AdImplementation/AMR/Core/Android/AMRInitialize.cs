using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR.Android
{
    public class AMRInitialize : IAMRSdk
    {
        protected internal AndroidJavaObject config;
        protected internal AndroidJavaObject activity;
        private AMRPlugin.Android.AMROfferWallSpendVirtualCurrencyListener offerwallListener = new AMRPlugin.Android.AMROfferWallSpendVirtualCurrencyListener();
        private AMRPlugin.Android.AMRTrackPurchaseListener trackPurchaseListener;
        private AMRPlugin.Android.AMRGDPRListener gdprListener;
        private AMRPlugin.Android.AMRPrivacyConsentListener privacyConsentListener;
        private AMRPlugin.Android.AMRRemoteConfigListener rcListener;
        private AMRPlugin.Android.AMRInitializeListener initListener;

        public AMRInitialize()
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity =
                    playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            config = new AndroidJavaObject("com.amr.unity.ads.Config", activity);
            playerClass.Dispose();
            activity.Dispose();

        }

        public void startWithAppId(string appId, bool isUserChild)
        {
            startWithAppId(appId, null, null, null, isUserChild ? "1" : "0", false);
        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string userConsent, bool isUserChild)
        {
            startWithAppId(appId, subjectToGDPR, null, userConsent, isUserChild ? "1" : "0", false);

        }

        public void startWithAppIdConsent(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, bool isUserChild)
        {
            startWithAppId(appId, subjectToGDPR, subjectToCCPA, userConsent, isUserChild ? "1":"0", false);
        }

        public void startWithAppId(string appId, string subjectToGDPR, string subjectToCCPA, string userConsent, string isUserChild, bool isHuaweiApp)
        {

            if (Application.platform == RuntimePlatform.Android)
            {
                config.Call("initialize", new object[6] { appId, subjectToGDPR, subjectToCCPA, userConsent, isUserChild, isHuaweiApp });
            }
        }

        public void start()
        {
            config.Call("onStart");
        }
        public void stop()
        {
            config.Call("onStop");
        }
        public void resume()
        {
            config.Call("onResume");
        }
        public void pause()
        {
            config.Call("onPause");
        }
        public void destroy()
        {
            config.Call("onDestroy");
        }
        
		public void startTestSuite(string[] zoneIds)
		{
			config.Call("startTestSuite", new object[1] { zoneIds });
		}

        public void setUserId(string userId)
        {
            config.Call("setUserId", new object[1] { userId });
        }

        public void setAdjustUserId(string adjustUserId)
        {
            config.Call("setAdjustUserId", new object[1] { adjustUserId });
        }

        public void setClientCampaignId(string campaignId)
        {
            config.Call("setClientCampaignId", new object[1] { campaignId });
        }

        public string trackPurchase(string uniqueID, double localizedPrice, string isoCurrencyCode)
		{
            /* uniqueID = receipt for android */
            AMRUtil.Log("ADMOST trackPurchase AMRInitilize called;");
			string[] toReturnArray = new string[3];
            toReturnArray[0] = uniqueID;
			toReturnArray[1] = localizedPrice+"";
            toReturnArray[2] = isoCurrencyCode;

            AMRUtil.Log("receipt ="+uniqueID+"localizedPriceString = "+localizedPrice +"isoCurrencyCode = "+isoCurrencyCode);

            return config.Call<string>("trackPurchase", new object[2] { toReturnArray, trackPurchaseListener });
		}

        public string trackIAP(string uniqueID, double localizedPrice, string isoCurrencyCode, string[] tags)
        {
            /* uniqueID = receipt for android */
            AMRUtil.Log("ADMOST trackIAP AMRInitilize called;");
            string[] toReturnArray = new string[3];
            toReturnArray[0] = uniqueID;
            toReturnArray[1] = localizedPrice + "";
            toReturnArray[2] = isoCurrencyCode;

            AMRUtil.Log("receipt =" + uniqueID + "localizedPriceString = " + localizedPrice + "isoCurrencyCode = " + isoCurrencyCode);

            return config.Call<string>("trackIAP", new object[2] { toReturnArray, tags });
        }

        public string trackIAPForHuawei(string uniqueID, string signature, string[] tags)
        {
            /* uniqueID = receipt for android */
            AMRUtil.Log("ADMOST trackIAPForHuawei AMRInitilize called;");
            string[] toReturnArray = new string[3];
            toReturnArray[0] = uniqueID;
            toReturnArray[1] = signature;
            
            AMRUtil.Log("receipt =" + uniqueID + " ** signature = " + signature);

            return config.Call<string>("trackIAPForHuawei", new object[2] { toReturnArray, tags });
        }

        public string trackPurchaseForAmazon(string userId, string receiptId, double localizedPrice, string marketPlace, string isoCurrencyCode)
        {
            /* uniqueID = receipt for android */
            string[] toReturnArray = new string[6];
            toReturnArray[0] = userId;
            toReturnArray[1] = receiptId;
            toReturnArray[2] = localizedPrice + "";
            toReturnArray[3] = marketPlace;
            toReturnArray[4] = isoCurrencyCode;
            toReturnArray[5] = "0"; // isDebug default false for now

            AMRUtil.Log("receipt =" + receiptId + "localizedPriceString = " + localizedPrice + "marketPlace = " + marketPlace + "userId = " + userId + " isoCurrencyCode:" + isoCurrencyCode);

            return config.Call<string>("trackPurchaseForAmazon", new object[2] { toReturnArray, trackPurchaseListener });
        }

        public void setTrackPurchaseDelegate(AMRTrackPurchaseDelegate delegateObject)
        {
            if (trackPurchaseListener == null)
            {
                trackPurchaseListener = new AMRPlugin.Android.AMRTrackPurchaseListener();
            }
            trackPurchaseListener.setDelegateObject(delegateObject);
        }

        public void spendVirtualCurrency()
        {
            config.Call("spendVirtualCurrency", new object[1] { offerwallListener });
        }

        public void setVirtualCurrencyDelegate(AMRVirtualCurrencyDelegate delegateObject)
        {
            offerwallListener.setDelegateObject(delegateObject);
        }

        public void setGDPRDelegate(AMRGDPRDelegate delegateObject)
        {
            if (gdprListener == null)
            {
                gdprListener = new AMRPlugin.Android.AMRGDPRListener();
            }
            gdprListener.setDelegateObject(delegateObject);

            config.Call("setGDPRListener", new object[1] { gdprListener });
        }

        public void setPrivacyConsentDelegate(AMRPrivacyConsentDelegate delegateObject)
        {
            if (privacyConsentListener == null)
            {
                privacyConsentListener = new AMRPlugin.Android.AMRPrivacyConsentListener();
            }
            privacyConsentListener.setDelegateObject(delegateObject);

            config.Call("setConsentListener", new object[1] { privacyConsentListener });
        }

        public void setSDKInitializeDelegate(AMRInitializeDelegate delegateObject)
        {
            if (delegateObject == null)
            {
                return;
            }

            if (initListener == null)
            {
                initListener = new AMRPlugin.Android.AMRInitializeListener();
            }
            initListener.setDelegateObject(delegateObject);

            config.Call("setInitializationListener", new object[1] { initListener });
        }
    }

}
