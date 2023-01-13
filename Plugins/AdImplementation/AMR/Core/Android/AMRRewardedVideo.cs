using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR.Android
{
	public class AMRRewardedVideo : AndroidJavaProxy
    {
        private AndroidJavaObject rewardedVideo;
        private AMRRewardedVideoViewDelegate delegateObj;

        public AMRRewardedVideo()
            : base("com.amr.unity.ads.UnityVideoAdListener")
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity =
                    playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            rewardedVideo = new AndroidJavaObject(
               "com.amr.unity.ads.Video", activity, this);
            playerClass.Dispose();
            activity.Dispose();
        }

        #region - IAMRRewardedVideo

        public void loadRewardedVideoForZoneId(string zoneId, AMRRewardedVideoViewDelegate delegateObject)
		{
            delegateObj = delegateObject;
            rewardedVideo.Call("create", new object[1] { zoneId });
        }

		public void showRewardedVideo()
		{
            rewardedVideo.Call("show");
        }

        public void showRewardedVideo(String tag)
        {
            rewardedVideo.Call("showWithTag", new object[1] { tag });
        }

        public void destroyRewardedVideo()
        {
            rewardedVideo.Call("destroy");
            rewardedVideo.Dispose();
        }

        public bool isReadyToShow()
        {
            return rewardedVideo.Call<bool>("isReadyToShow");
        }

        #endregion

        #region Callbacks from UnityVideoAdListener.

        void onAdLoaded(string networkName, double ecpm)
        {
			delegateObj.didReceiveRewardedVideo(networkName, ecpm);
        }

		void onAdFailedToLoad(int errorCode)
        {
            if (errorCode == 301 || errorCode == 302 || errorCode == 305)
            {
                delegateObj.didFailtoShowRewardedVideo("" + errorCode);
            }
            else
            {
                delegateObj.didFailtoReceiveRewardedVideo("" + errorCode);
            }
        }

        void onAdShowed(string message)
        {
            delegateObj.didShowRewardedVideo();
        }

	    void onAdFailedToShow()
	    {
		    delegateObj.didFailtoShowRewardedVideo("");
	    }

        void onAdClicked(string networkName)
        {
            delegateObj.didClickRewardedVideo(networkName);
        }

        void onAdCompleted()
        {
    		delegateObj.didCompleteRewardedVideo();
        }

        void onAdClosed(string message)
        {
            delegateObj.didDismissRewardedVideo();
        }
		
		void onStatusChanged(int status)
        {
            delegateObj.didStatusChangeRewardedVideo(status);
        }

        void onAdRevenuePaid(string zoneId, string network, string format, string placementId, string adUnitId, string currency, double revenue)
        {
            AMRAd ad = new AMRAd(zoneId, network);
            ad.AdSpaceId = adUnitId;
            ad.Currency = currency;
            ad.Revenue = revenue;

            delegateObj.didImpressionRewardedVideo(ad);
        }

        void onAdNetworkRewarded(double rewardAmount)
        {
            delegateObj.didRewardRewardedVideo(rewardAmount);
        }

        #endregion
    }
}

