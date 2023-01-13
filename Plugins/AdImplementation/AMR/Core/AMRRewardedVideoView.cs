using System;
using AMR.iOS;
using UnityEngine;

namespace AMR
{
	public class AMRRewardedVideoView: AMRAdView
    {
        private class VideoDelegate : AMRRewardedVideoViewDelegate
        {
            private AMRRewardedVideoView videoView;
            public VideoDelegate(AMRRewardedVideoView vv)
            {
                videoView = vv;
            }

            public void didReceiveRewardedVideo(string networkName, double ecpm)
            {
                videoView.state = VideoState.Loaded;
                videoView.failCount = 0;
                if (videoView.onReadyDelegate != null)
                {
                    videoView.onReadyDelegate(networkName, ecpm);
                }
            }

            public void didFailtoReceiveRewardedVideo(string error)
            {
                videoView.failCount++;
                videoView.state = VideoState.New;
                if (videoView.failCount <= 0)
                { // Try once again when not loaded 
                    videoView.loadRewardedVideoForZoneId(videoView.iosZoneId, videoView.androidZoneId, videoView.isRefresh);
                }
                else
                {
                    videoView.failCount = 0;
                    if (videoView.onFailDelegate != null)
                    {
                        videoView.onFailDelegate(error);
                    }
                }
            }

            public void didShowRewardedVideo()
            {
                if (videoView.onShowDelegate != null)
                {
                    videoView.onShowDelegate();
                }
            }

            public void didFailtoShowRewardedVideo(String errorCode)
            {
                if (errorCode.Equals("1078") || errorCode.Equals("301"))
                {
                    videoView.state = VideoState.Loaded;
                }
                else
                {
                    videoView.state = VideoState.New;
                }
                

                if (videoView.onFailToShowDelegate != null)
                {
                    videoView.onFailToShowDelegate();
                }
            }

            public void didClickRewardedVideo(string networkName)
            {
                if (videoView.onClickDelegate != null)
                {
                    videoView.onClickDelegate(networkName);
                }
            }

            public void didImpressionRewardedVideo(AMRAd ad)
            {
                if (videoView.onImpressionDelegate != null)
                {
                    videoView.onImpressionDelegate(ad);
                }
            }

            public void didCompleteRewardedVideo()
            {
				if (videoView.onCompleteDelegate != null) 
				{
					videoView.onCompleteDelegate ();
				}
            }

            public void didDismissRewardedVideo()
            {
                videoView.state = VideoState.New;

				AMRSDK.resolveBannerConflict();
				if (videoView.onDismissDelegate != null) 
				{
                    videoView.onDismissDelegate ();
				}
            }

            public void didStatusChangeRewardedVideo(int status)
            {
                if (videoView.onStatusChangeDelegate != null)
                {
                    videoView.onStatusChangeDelegate(status);
                }
            }
            public void didRewardRewardedVideo(double rewardAmount)
            {
                if (videoView.onRewardDelegate != null)
                {
                    videoView.onRewardDelegate(rewardAmount);
                }
            }
        }
        private static AMRRewardedVideoView instance;
        private Android.AMRRewardedVideo rewardedVideoAndroid;
        private VideoState state;

        private EventDelegateReady onReadyDelegate;
        private EventDelegateFail onFailDelegate;
        private EventDelegateShow onShowDelegate;
        private EventDelegateFailToShow onFailToShowDelegate;
        private EventDelegateClick onClickDelegate;
        private EventDelegateImpression onImpressionDelegate;
        private EventDelegateComplete onCompleteDelegate;
        private EventDelegateDismiss onDismissDelegate;
        private EventDelegateStatusChange onStatusChangeDelegate;
        private EventDelegateReward onRewardDelegate;

        private int failCount;
        private String androidZoneId, iosZoneId;
        private bool isRefresh;

        private enum VideoState
        {
            New,
            Loading,
            Loaded,
            Playing
        }

        public static AMRRewardedVideoView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMRRewardedVideoView();
                }
                return instance;
            }
        }

        public void loadRewardedVideoForZoneId(string zoneIdiOS,
                                                string zoneIdAndroid,
                                                bool refresh)
        {
            if (refresh)
            {
                if (state != VideoState.New && state != VideoState.Loaded)
                {
                    return;
                }
            }
            else if (state != VideoState.New)
            {
	            //Debug.Log("<AMRSDK> loadRewardedVideoForZoneId - state != VideoState.New returning");
                return;
            }

            state = VideoState.Loading;
            androidZoneId = zoneIdAndroid;
            iosZoneId = zoneIdiOS;
            isRefresh = refresh;
            VideoDelegate videoDelegate = new VideoDelegate(this);

            if (Application.platform == RuntimePlatform.IPhonePlayer) 
            {
				if (zoneIdiOS != null)
				{
                    AMRRewardedVideoManager.LoadRewardedVideo(zoneIdiOS, videoDelegate);
				}
            }
            else if (Application.platform == RuntimePlatform.Android) 
            {
				if (zoneIdAndroid != null)
				{
                    if (rewardedVideoAndroid != null)
					{
                        rewardedVideoAndroid.destroyRewardedVideo();
					}
                    rewardedVideoAndroid = new AMR.Android.AMRRewardedVideo();
                    rewardedVideoAndroid.loadRewardedVideoForZoneId(zoneIdAndroid, videoDelegate);
				}
            }
        }

        public void showRewardedVideo()
        {
            state = VideoState.Playing;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRRewardedVideoManager.ShowRewardedVideo(iosZoneId, null);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                rewardedVideoAndroid.showRewardedVideo();
            }
        }

        public void showRewardedVideo(String tag)
        {
            state = VideoState.Playing;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRRewardedVideoManager.ShowRewardedVideo(iosZoneId, tag);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                rewardedVideoAndroid.showRewardedVideo(tag);
            }
        }

        /* States */
        public Boolean isReady()
        {
            return state == VideoState.Loaded;
        }

        public Boolean isPlaying()
        {
            return state == VideoState.Playing;
        }

        public Boolean isReadyToShow()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return AMRRewardedVideoManager.isReadyToShow(iosZoneId);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return rewardedVideoAndroid.isReadyToShow();
            }

            return false;
        }

        /* Possible Callbacks */
        public void setOnReady(EventDelegateReady onReadyDelegate) {
            this.onReadyDelegate = onReadyDelegate;
        }

        public void setOnFail(EventDelegateFail onFailDelegate)
        {
            this.onFailDelegate = onFailDelegate;
        }

        public void setOnShow(EventDelegateShow onShowDelegate)
        {
            this.onShowDelegate = onShowDelegate;
        }

        public void setOnFailToShow(EventDelegateFailToShow onFailToShowDelegate)
        {
            this.onFailToShowDelegate = onFailToShowDelegate;
        }

        public void setOnClick(EventDelegateClick onClickDelegate)
        {
            this.onClickDelegate = onClickDelegate;
        }

        public void setOnImpression(EventDelegateImpression onImpressionDelegate)
        {
            this.onImpressionDelegate = onImpressionDelegate;
        }

        public void setOnComplete(EventDelegateComplete onCompleteDelegate) {
            this.onCompleteDelegate = onCompleteDelegate;
        }

	    public void setOnDismiss(EventDelegateDismiss onDismissDelegate) {
            this.onDismissDelegate = onDismissDelegate;
        }

        public void setOnStatusChange(EventDelegateStatusChange onStatusChangeDelegate)
        {
            this.onStatusChangeDelegate = onStatusChangeDelegate;
        }

        public void setOnReward(EventDelegateReward onRewardDelegate)
        {
            this.onRewardDelegate = onRewardDelegate;
        }
    }
}

