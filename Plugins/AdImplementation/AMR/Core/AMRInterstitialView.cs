using System;
using UnityEngine;
using AMR.iOS;
namespace AMR
{
    public class AMRInterstitialView: AMRAdView
    {
        private class InterstitialDelegate : AMRInterstitialViewDelegate
        {
            private AMRInterstitialView interstitialView;
            
            public InterstitialDelegate(AMRInterstitialView iv)
            {
                interstitialView = iv;
            }

            public void didReceiveInterstitial(string networkName, double ecpm)
            {
                interstitialView.state = InterstitialState.Loaded;
                if (interstitialView.onReadyDelegate != null)
                {
                    interstitialView.onReadyDelegate(networkName, ecpm);
                }
            }

            public void didFailtoReceiveInterstitial(string error)
            {
                interstitialView.state = InterstitialState.New;
                if (interstitialView.onFailDelegate != null)
                {
                    interstitialView.onFailDelegate(error);
                }
                
            }

            public void didShowInterstitial()
            {
                if (interstitialView.onShowDelegate != null)
                {
                    interstitialView.onShowDelegate();
                }
            }

            public void didFailtoShowInterstitial(string errorCode)
            {
                if (errorCode.Equals("1078") || errorCode.Equals("301"))
                {
                    interstitialView.state = InterstitialState.Loaded;
                } else
                {
                    interstitialView.state = InterstitialState.New;
                }

                if (interstitialView.onFailToShowDelegate != null)
                {
                    interstitialView.onFailToShowDelegate();
                }
            }

            public void didClickInterstitial(string networkName)
            {
                if (interstitialView.onClickDelegate != null)
                {
                    interstitialView.onClickDelegate(networkName);
                }
            }

            public void didImpressionInterstitial(AMRAd ad)
            {
                if (interstitialView.onImpressionDelegate != null)
                {
                    interstitialView.onImpressionDelegate(ad);
                }
            }

            public void didDismissInterstitial()
            {
                interstitialView.state = InterstitialState.New;
                AMRSDK.resolveBannerConflict();
                if (interstitialView.onDismissDelegate != null)
                {
                    interstitialView.onDismissDelegate();
                }
            }

            public void didStatusChangeInterstitial(int status)
            {
                if (interstitialView.onStatusChangeDelegate != null)
                {
                    interstitialView.onStatusChangeDelegate(status);
                }
            }
        }

        private static AMRInterstitialView instance;
        private IAMRInterstitial interstitial;
        private InterstitialState state;

        private EventDelegateReady onReadyDelegate;
        private EventDelegateFail onFailDelegate;
        private EventDelegateShow onShowDelegate;
        private EventDelegateFailToShow onFailToShowDelegate;
        private EventDelegateClick onClickDelegate;
        private EventDelegateImpression onImpressionDelegate;
        private EventDelegateDismiss onDismissDelegate;
        private EventDelegateStatusChange onStatusChangeDelegate;

        private string androidZoneId, iosZoneId;
        private bool isRefresh;

        private enum InterstitialState
        {
            New,
            Loading,
            Loaded,
            Showing
        }

        public static AMRInterstitialView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMRInterstitialView();
                }
                return instance;
            }
        }

        public void loadInterstitialForZoneId(string zoneIdiOS,
                                                string zoneIdAndroid,
                                                bool refresh)
        {
            if (refresh)
            {
                if (state != InterstitialState.New && state != InterstitialState.Loaded)
                {
                    return;
                }
            } 
            else if (state != InterstitialState.New)
            {
                return;
            }

            androidZoneId = zoneIdAndroid;
            iosZoneId = zoneIdiOS;
            isRefresh = refresh;
            state = InterstitialState.Loading;
            InterstitialDelegate interstitialDelegate = new InterstitialDelegate(this);
            if (Application.platform == RuntimePlatform.IPhonePlayer) 
            {
				if (zoneIdiOS != null)
				{
                    AMRInterstitialManager.LoadInterstitial(zoneIdiOS, interstitialDelegate);
                }
            }
            else if (Application.platform == RuntimePlatform.Android) {
				if (zoneIdAndroid != null)
				{
					if (interstitial != null)
					{
						interstitial.destroyInterstitial();
					}
					interstitial = new AMR.Android.AMRInterstitial();
					interstitial.loadInterstitialForZoneId(zoneIdAndroid, interstitialDelegate);
				}
            }

        }

        public void showInterstitial()
        {
            state = InterstitialState.Showing;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRInterstitialManager.ShowInterstitial(iosZoneId, null);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                interstitial.showInterstitial();
            }
        }

        public void showInterstitial(string tag)
        {
            state = InterstitialState.Showing;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRInterstitialManager.ShowInterstitial(iosZoneId, tag);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                interstitial.showInterstitial(tag);
            }
        }

        /* States */
        public Boolean isReady()
        {
            return state == InterstitialState.Loaded;
        }

        public Boolean isShowing()
        {
            return state == InterstitialState.Showing;
        }

        public Boolean isReadyToShow()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return AMRInterstitialManager.isReadyToShow(iosZoneId);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return interstitial.isReadyToShow();
            }

            return false;
        }

        /* Possible Callbacks */
        public void setOnReady(EventDelegateReady onReadyDelegate)
        {
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

        public void setOnDismiss(EventDelegateDismiss onDismissDelegate)
        {
            this.onDismissDelegate = onDismissDelegate;
        }

        public void setOnClick(EventDelegateClick onClickDelegate)
        {
            this.onClickDelegate = onClickDelegate;
        }

        public void setOnImpression(EventDelegateImpression onImpressionDelegate)
        {
            this.onImpressionDelegate = onImpressionDelegate;
        }

        public void setOnStatusChange(EventDelegateStatusChange onStatusChangeDelegate)
        {
            this.onStatusChangeDelegate = onStatusChangeDelegate;
        }
    }
}

