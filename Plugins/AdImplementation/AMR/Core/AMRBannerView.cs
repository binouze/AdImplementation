using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace AMR
{
	public class AMRBannerView: AMRAdView
	{
        private bool isConflicted;
        private static AMRBannerView instance;
        private BannerState state;
        private Boolean dontShowBanner;

        private class BannerDelegate : AMR.AMRBannerViewDelegate
        {
            private AMRBannerView bannerView;
            public BannerDelegate(AMRBannerView bv)
            {
                bannerView = bv;
            }

            public void didReceiveBanner(string networkName, double ecpm)
            {
                bannerView.state = BannerState.Loaded;

                if (bannerView.autoShow && !bannerView.dontShowBanner)
                {
                    bannerView.showBanner(false);
                }

                if (bannerView.didReceiveDelegate != null)
                {
                    bannerView.didReceiveDelegate(networkName, ecpm);
                }
            }

            public void didFailtoReceiveBanner(string error)
            {
                bannerView.state = BannerState.New;
                if (bannerView.didFailToReceiveDelegate != null)
                    bannerView.didFailToReceiveDelegate(error);                
            }

            public void didImpressionBanner(AMRAd ad)
            {
                if (bannerView.didImpressionDelegate != null)
                {
                    bannerView.didImpressionDelegate(ad);
                }
            }

            public void didClickBanner(string networkName)
            {
                if (bannerView.didClickDelegate != null)
                {
                    bannerView.didClickDelegate(networkName);
                }
            }
        }

        private IAMRBanner Banner;
        private EventDelegateReady didReceiveDelegate;
        private EventDelegateFail didFailToReceiveDelegate;
        private EventDelegateImpression didImpressionDelegate;
        private EventDelegateClick didClickDelegate;
        private string zoneIdiOS;
        private string zoneIdAndroid;
        private Enums.AMRSDKBannerPosition position;
        private int offset;
		private bool autoShow;
        private double positionX;
        private double positionY;
        private bool useBannerCoordinates;

        private enum BannerState
        {
            New,
            Loading,
            Loaded
        }

        public static AMRBannerView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMRBannerView();
                }
                return instance;
            }
        }

        public void loadBannerForZoneId(string zoneIdiOS,
                                        string zoneIdAndroid,
                                        AMR.Enums.AMRSDKBannerPosition position,
                                        int offset,
                                        bool autoShow)
        {

            if (autoShow)
                dontShowBanner = false;

            if (state == BannerState.Loading)
            {
                return;
            }

            this.zoneIdiOS = zoneIdiOS;
            this.zoneIdAndroid = zoneIdAndroid;
            this.position = position;
            this.offset = offset;
            this.autoShow = autoShow;
            this.isConflicted = false;
            this.useBannerCoordinates = false;

            loadBanner();
        }

        public void loadBannerForZoneId(string zoneIdiOS,
                                        string zoneIdAndroid,
                                        double positionX,
                                        double positionY,
                                        bool autoShow)
        {
            if (autoShow)
                dontShowBanner = false;

            if (state == BannerState.Loading)
            {
                return;
            }

            this.zoneIdiOS = zoneIdiOS;
            this.zoneIdAndroid = zoneIdAndroid;
            this.positionX = positionX;
            this.positionY = positionY;
            this.autoShow = autoShow;
            this.isConflicted = false;
            this.useBannerCoordinates = true;

            loadBanner();
        }

        private void loadBanner()
        {
            if (state == BannerState.Loading)
            {
                return;
            }

            hideBanner(false);

            state = BannerState.Loading;
            BannerDelegate bDelegate = new BannerDelegate(this);

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (zoneIdiOS != null)
                {
                    Banner = new AMR.iOS.AMRBanner();

                    if (useBannerCoordinates)
                    {
                        Banner.loadBannerForZoneIdWithPosition(zoneIdiOS, positionX, positionY, bDelegate);
                    }
                    else
                    {
                        Banner.loadBannerForZoneId(zoneIdiOS, position, offset, bDelegate);
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (zoneIdAndroid != null)
                {
                    if (Banner == null)
                    {
                        Banner = new AMR.Android.AMRBanner();
                    }

                    if (useBannerCoordinates)
                    {
                        Banner.loadBannerForZoneIdWithPosition(zoneIdAndroid, positionX, positionY, bDelegate);
                    }
                    else
                    {
                        Banner.loadBannerForZoneId(zoneIdAndroid, position, offset, bDelegate);
                    }
                }
            }
        }

        public void showBanner(Boolean userAction)
		{
            if (userAction)
            {
                dontShowBanner = false;
            }

            if (AMRSDK.isFullScreenAdShowing())
            {
                hideBanner(false);
                isConflicted = true;
            }
            else
            {
                if (state == BannerState.Loaded)
                {
                    Banner.showBanner();
                }
                else if (state == BannerState.New)
                {
                    loadBanner();
                }
            }
		}

        public void hideBanner(Boolean userAction)
		{
            if (userAction)
            {
                dontShowBanner = true;
            }
            if (Banner != null)
               Banner.hideBanner();
		}

        public void destroyBanner()
        {
            state = BannerState.New;
            if (Banner != null)
            {
                Banner.destroyBanner();
            }
        }

        public void resolveConflict()
        {
            if (isConflicted)
            {
                isConflicted = false;

                if (!dontShowBanner)
                {
                    showBanner(false);
                }
            }		
        }

        /* Possible Callbacks */
        public void setDidReceiveBanner(EventDelegateReady didReceiveDelegate)
        {
            this.didReceiveDelegate = didReceiveDelegate;
        }

        public void setDidFailToReceiveBanner(EventDelegateFail didFailToReceiveDelegate)
        {
            this.didFailToReceiveDelegate = didFailToReceiveDelegate;
        }

        public void setDidImpressionBanner(EventDelegateImpression didImpressionDelegate)
        {
            this.didImpressionDelegate = didImpressionDelegate;
        }

        public void setDidClickBanner(EventDelegateClick didClickDelegate)
        {
            this.didClickDelegate = didClickDelegate;
        }

    }
}