
using System;
using AMR.iOS;
using com.binouze;
using UnityEngine;

namespace AMR
{
    public class AMRInterstitialAd : AMRAdView
    {
        public string AndroidZoneId;
        public string iOSZoneId;
        public string ZoneId
        {
            get
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return iOSZoneId;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return AndroidZoneId;
                }
                else
                {
                    return null;
                }
            }
        }

        private Android.AMRInterstitial interstitialAndroid;

        public AdStatus Status;

        private AdDelegateReady onInterstitialReadyDelegate;
        private AdDelegateFail onInterstitialFailDelegate;
        private AdDelegateShow onInterstitialShowDelegate;
        private AdDelegateFailToShow onInterstitialFailToShowDelegate;
        private AdDelegateClick onInterstitialClickDelegate;
        private AdDelegateImpression onInterstitialImpressionDelegate;
        private AdDelegateDismiss onInterstitialDismissDelegate;

        #region AMRInterstitialAd

        public void LoadInterstitial()
        {
            if (Status == AdStatus.Loading)
            {
                return;
            }

            Status = AdStatus.Loading;
            InterstitialAdDelegate interstitialDelegate = new InterstitialAdDelegate(this);

            #if UNITY_EDITOR
            interstitialDelegate.didReceiveInterstitial( "test", 0 );
            #else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRInterstitialManager.LoadInterstitial(ZoneId, interstitialDelegate);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (interstitialAndroid != null)
                {
                    interstitialAndroid.destroyInterstitial();
                }
                interstitialAndroid = new Android.AMRInterstitial();
                interstitialAndroid.loadInterstitialForZoneId(ZoneId, interstitialDelegate);
            }
            #endif
        }

        public void ShowInterstitial(string tag = null)
        {
            Status = AdStatus.Playing;
            
            #if UNITY_EDITOR
            var interstitialDelegate = new InterstitialAdDelegate(this);
            AdsEditorHelper.ShowDialog( "TEST INTERSTITIAL", "OK", () =>
            {
                interstitialDelegate.didDismissInterstitial();
            } );
            #else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                AMRInterstitialManager.ShowInterstitial(ZoneId, tag);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (tag == null)
                {
                    interstitialAndroid.showInterstitial();
                }
                else
                {
                    interstitialAndroid.showInterstitial(tag);
                }
            }
            #endif
        }

        /* Interstitial Callbacks */

        public void SetOnInterstitialReady(AdDelegateReady onInterstitialReadyDelegate)
        {
            this.onInterstitialReadyDelegate = onInterstitialReadyDelegate;
        }

        public void SetOnInterstitialFail(AdDelegateFail onInterstitialFailDelegate)
        {
            this.onInterstitialFailDelegate = onInterstitialFailDelegate;
        }

        public void SetOnInterstitialShow(AdDelegateShow onInterstitialShowDelegate)
        {
            this.onInterstitialShowDelegate = onInterstitialShowDelegate;
        }

        public void SetOnInterstitialFailToShow(AdDelegateFailToShow onInterstitialFailToShowDelegate)
        {
            this.onInterstitialFailToShowDelegate = onInterstitialFailToShowDelegate;
        }

        public void SetOnInterstitialClick(AdDelegateClick onInterstitialClickDelegate)
        {
            this.onInterstitialClickDelegate = onInterstitialClickDelegate;
        }

        public void SetOnInterstitialDismiss(AdDelegateDismiss onInterstitialDismissDelegate)
        {
            this.onInterstitialDismissDelegate = onInterstitialDismissDelegate;
        }
        public void SetOnInterstitialImpression(AdDelegateImpression onInterstitialImpressionDelegate)
        {
            this.onInterstitialImpressionDelegate = onInterstitialImpressionDelegate;
        }

        #endregion


        #region Other

        public enum AdStatus
        {
            New,
            Loading,
            Loaded,
            Playing
        }

        private class InterstitialAdDelegate : AMRInterstitialViewDelegate
        {
            private readonly AMRInterstitialAd interstitialAd;
            public InterstitialAdDelegate(AMRInterstitialAd va)
            {
                interstitialAd = va;
            }

            public void didReceiveInterstitial(string networkName, double ecpm)
            {
                interstitialAd.Status = AdStatus.Loaded;

                if (interstitialAd.onInterstitialReadyDelegate != null)
                {
                    interstitialAd.onInterstitialReadyDelegate(interstitialAd.ZoneId, networkName, ecpm);
                }
            }

            public void didFailtoReceiveInterstitial(string error)
            {
                interstitialAd.Status = AdStatus.New;

                if (interstitialAd.onInterstitialFailDelegate != null)
                {
                    interstitialAd.onInterstitialFailDelegate(interstitialAd.ZoneId, error);
                }
            }

            public void didShowInterstitial()
            {
                if (interstitialAd.onInterstitialShowDelegate != null)
                {
                    interstitialAd.onInterstitialShowDelegate(interstitialAd.ZoneId);
                }
            }

            public void didFailtoShowInterstitial(String errorCode)
            {
                interstitialAd.Status = AdStatus.Loaded;

                if (interstitialAd.onInterstitialFailToShowDelegate != null)
                {
                    interstitialAd.onInterstitialFailToShowDelegate(interstitialAd.ZoneId);
                }
            }

            public void didClickInterstitial(string networkName)
            {
                if (interstitialAd.onInterstitialClickDelegate != null)
                {
                    interstitialAd.onInterstitialClickDelegate(interstitialAd.ZoneId, networkName);
                }
            }

            public void didImpressionInterstitial(AMRAd ad)
            {
                if (interstitialAd.onInterstitialImpressionDelegate != null)
                {
                    interstitialAd.onInterstitialImpressionDelegate(ad);
                }
            }

            public void didDismissInterstitial()
            {
                interstitialAd.Status = AdStatus.New;

                AMRSDK.resolveBannerConflict();

                if (interstitialAd.onInterstitialDismissDelegate != null)
                {
                    interstitialAd.onInterstitialDismissDelegate(interstitialAd.ZoneId);
                }
            }

            public void didStatusChangeInterstitial(int status) { }
        }
        #endregion
    }
}


