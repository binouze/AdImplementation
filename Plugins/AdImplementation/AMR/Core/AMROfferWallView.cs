using System;
using AMRPlugin.iOS;
using UnityEngine;

namespace AMR
{
    public class AMROfferWallView: AMRAdView
    {
        private class OfferWallDelegate : AMROfferWallViewDelegate
        {
            private AMROfferWallView offerWallView;
            
            public OfferWallDelegate(AMROfferWallView ov)
            {
                offerWallView = ov;
            }       

            public void didDismissOfferWall()
            {
                offerWallView.state = OfferWallState.New;
                AMRSDK.resolveBannerConflict();
                if (offerWallView.onDismissDelegate != null)
                {
                    offerWallView.onDismissDelegate();
                }
            }

            public void didFailToReceiveOfferWall(string error)
            {   
                offerWallView.failCount++;
                offerWallView.state = OfferWallState.New;
                if (offerWallView.failCount <= 1)
                { // Try once again when not loaded
                    offerWallView.loadOfferWallForZoneId(offerWallView.iosZoneId, offerWallView.androidZoneId, offerWallView.isRefresh);
                }
                else
                {
                    offerWallView.failCount = 0;
                    if (offerWallView.onFailDelegate != null)
                    {
                        offerWallView.onFailDelegate(error);
                    }
                }   
            }

            public void didReceiveOfferWall(string networkName, double ecpm)
            {   
                offerWallView.state = OfferWallState.Loaded;
                offerWallView.failCount = 0;
                if (offerWallView.onReadyDelegate != null)
                {
                    offerWallView.onReadyDelegate(networkName, ecpm);
                }
            }
        }
        
        private static AMROfferWallView instance;
        private IAMROfferWall offerWall;
        private OfferWallState state;

        private EventDelegateReady onReadyDelegate;
        private EventDelegateDismiss onDismissDelegate;
        private EventDelegateFail onFailDelegate;

        private int failCount = 0;
        private string androidZoneId, iosZoneId;
        private bool isRefresh;
        
        private enum OfferWallState
        {
            New,
            Loading,
            Loaded,
            Showing
        }
        
        public static AMROfferWallView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMROfferWallView();
                }
                return instance;
            }
        }

        public void loadOfferWallForZoneId(string zoneIdiOS,
                                           string zoneIdAndroid,
                                           bool refresh)
        {   
            if (refresh)
            {   
                if (state != OfferWallState.New && state != OfferWallState.Loaded)
                {
                    return;
                }
            } 
            else if (state != OfferWallState.New)
            {
                return;
            }

            androidZoneId = zoneIdAndroid;
            iosZoneId = zoneIdiOS;
            isRefresh = refresh;
            state = OfferWallState.Loading;
            OfferWallDelegate offerWallDelegate = new OfferWallDelegate(this);
            
            if (Application.platform == RuntimePlatform.IPhonePlayer) 
            {
                if (zoneIdiOS == null) return;
                
                offerWall = new AMRPlugin.iOS.AMROfferWall();
                offerWall.loadOfferWallForZoneId(zoneIdiOS, offerWallDelegate);
            }
            else if (Application.platform == RuntimePlatform.Android) 
            {
                if (zoneIdAndroid == null) return;
                
                if (offerWall != null)
                {
                    offerWall.destroyOfferWall();
                }
                
                offerWall = new Android.AMROfferWall();
                offerWall.loadOfferWallForZoneId(zoneIdAndroid, offerWallDelegate);
            }

        }

        public void showOfferWall()
        {
            state = OfferWallState.Showing;
            offerWall.showOfferWall();
        }

        public void showOfferWall(String tag)
        {
            state = OfferWallState.Showing;
            offerWall.showOfferWall(tag);
        }

        /* States */
        public Boolean isReady()
        {
            return state == OfferWallState.Loaded;
        }
        public Boolean isShowing()
        {
            return state == OfferWallState.Showing;
        }

        /* Possible Callbacks */
        public void setOnReady(EventDelegateReady onReadyDelegate)
        {
            this.onReadyDelegate = onReadyDelegate;
        }

        public void setOnDismiss(EventDelegateDismiss onDismissDelegate)
        {
            this.onDismissDelegate = onDismissDelegate;
        }

        public void setOnFail(EventDelegateFail onFailDelegate)
        {
            this.onFailDelegate = onFailDelegate;
        }
        
    }
}