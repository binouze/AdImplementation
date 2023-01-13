using System;
using UnityEngine;
using System.Runtime.InteropServices;


namespace AMR.Android
{
	public class AMRBanner:AndroidJavaProxy, IAMRBanner
    {
        private AndroidJavaObject banner;
        private AMRBannerViewDelegate delegateObj;

        public AMRBanner()
            : base("com.amr.unity.ads.UnityBannerAdListener")
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity =
                    playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            banner = new AndroidJavaObject(
               "com.amr.unity.ads.Banner", activity, this);

            playerClass.Dispose();
            activity.Dispose();
        }

        #region - IAMRBanner

        public void loadBannerForZoneId(string zoneId, 
		                                AMR.Enums.AMRSDKBannerPosition position,
                                        int offset,
		                                AMRBannerViewDelegate delegateObject)
		{
            delegateObj = delegateObject;
            banner.Call("create", new object[4] { zoneId, 50, (int)position, offset });
        }

        public void loadBannerForZoneIdWithPosition(string zoneId,
                                            double positionX,
                                            double positionY,
                                            AMRBannerViewDelegate delegateObject)
        {
            delegateObj = delegateObject;
            banner.Call("create_xy", new object[4] { zoneId, 50, (int) positionX, (int) positionY });
        }

        public void showBanner()
		{
			banner.Call("show");
		}

		public void hideBanner()
		{
            banner.Call("hide");
        }

        public void destroyBanner()
        {
            banner.Call("destroy");
        }

        #endregion


        #region Callbacks from UnityBannerAdListener.

        void onAdLoaded(string networkName, double ecpm)
        {
			delegateObj.didReceiveBanner(networkName, ecpm);
        }

        void onAdFailedToLoad(string error)
        {
            delegateObj.didFailtoReceiveBanner(error);
        }

        void onAdClicked(string networkName)
        {
            delegateObj.didClickBanner(networkName);
        }

        void onAdRevenuePaid(string zoneId, string network, string format, string placementId, string adUnitId, string currency, double revenue)
        {
            AMRAd ad = new AMRAd(zoneId, network);
            ad.AdSpaceId = adUnitId;
            ad.Currency = currency;
            ad.Revenue = revenue;

            delegateObj.didImpressionBanner(ad);
        }

        #endregion
    }
}

