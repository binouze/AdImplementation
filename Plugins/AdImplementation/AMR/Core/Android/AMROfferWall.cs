using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR.Android
{
    public class AMROfferWall : AndroidJavaProxy, IAMROfferWall
    {
        private AndroidJavaObject offerWall;
        private AMROfferWallViewDelegate delegateObj;

        public AMROfferWall()
            : base("com.amr.unity.ads.UnityOfferWallAdListener")
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            offerWall = new AndroidJavaObject(
                "com.amr.unity.ads.OfferWall", activity, this);
        }

        #region - IAMROfferWall

        public void loadOfferWallForZoneId(string zoneId, AMROfferWallViewDelegate delegateObject)
        {
            delegateObj = delegateObject;
            offerWall.Call("create", new object[1] { zoneId });
        }

        public void showOfferWall()
        {
            offerWall.Call("show");
        }

        public void showOfferWall(String tag)
        {
            offerWall.Call("showWithTag", new object[1] { tag });
        }

        public void destroyOfferWall()
        {
            offerWall.Call("destroy");
        }

        #endregion

        #region Callbacks from UnityOfferWallAdListener.

        void onAdLoaded(string networkName, double ecpm)
        {
            delegateObj.didReceiveOfferWall(networkName, ecpm);
        }

        void onAdFailedToLoad(int errorCode)
        {
            delegateObj.didFailToReceiveOfferWall(errorCode.ToString());
        }


        void onAdClosed(string err)
        {
            AMRUtil.Log("onAdClosed from android");
            delegateObj.didDismissOfferWall();
        }

        void onAdShowed(string message)
        {
            // No such method
        }
		
		// No method exists yet, it might be added later
		void onStatusChanged(int status)
        {
			
        }

        #endregion
    }
}