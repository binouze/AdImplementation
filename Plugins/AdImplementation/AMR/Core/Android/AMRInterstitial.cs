using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR.Android
{
	public class AMRInterstitial : AndroidJavaProxy, IAMRInterstitial
    {
        private AndroidJavaObject interstitial;
        private AMRInterstitialViewDelegate delegateObj;

        public AMRInterstitial()
            : base("com.amr.unity.ads.UnityInterstitialAdListener")
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity =
                    playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            interstitial = new AndroidJavaObject(
               "com.amr.unity.ads.Interstitial", activity, this);
            playerClass.Dispose();
            activity.Dispose();
        }

        #region - IAMRInterstitial

        public void loadInterstitialForZoneId(string zoneId, AMRInterstitialViewDelegate delegateObject)
		{
            delegateObj = delegateObject;
            interstitial.Call("create", new object[1] { zoneId });
        }

		public void showInterstitial()
		{
            interstitial.Call("show");
        }

        public void showInterstitial(string tag)
        {
            interstitial.Call("showWithTag", new object[1] { tag });
        }

        public void destroyInterstitial()
        {
            Debug.Log("destroyInterstitial started");
            interstitial.Call("destroy");
            interstitial.Dispose();
        }

        public bool isReadyToShow()
        {
            return interstitial.Call<bool>("isReadyToShow");
        }

        #endregion

        #region Callbacks from UnityInterstitialAdListener.

        void onAdLoaded(string networkName, double ecpm)
        {
            delegateObj.didReceiveInterstitial(networkName, ecpm);
        }

		void onAdFailedToLoad(int errorCode)
        {
            if (errorCode == 301 || errorCode == 302 || errorCode == 305)
            {
                delegateObj.didFailtoShowInterstitial(errorCode + "");
            }
            else
            {
                delegateObj.didFailtoReceiveInterstitial("" + errorCode);
            }
        }

        void onAdShowed(string message)
        {
            delegateObj.didShowInterstitial();
        }

        void onAdOpened()
        {
            
        }

        void onAdClosed(string message)
        {
            delegateObj.didDismissInterstitial();
        }

        void onAdClicked(string networkName)
        {
            delegateObj.didClickInterstitial(networkName);
        }
		
		void onStatusChanged(int status)
        {
            delegateObj.didStatusChangeInterstitial(status);
        }

        void onAdRevenuePaid(string zoneId, string network, string format, string placementId, string adUnitId, string currency, double revenue)
        {
            AMRAd ad = new AMRAd(zoneId, network);
            ad.AdSpaceId = adUnitId;
            ad.Currency = currency;
            ad.Revenue = revenue;
            
            delegateObj.didImpressionInterstitial(ad);
        }

        #endregion
    }
}

