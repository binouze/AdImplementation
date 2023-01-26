using System;
using System.Collections.Generic;
using System.Threading;
using AMR;

namespace com.binouze
{
    internal class AdMostAd
    {
        public AdMostAd( bool rewarded )
        {
            Rewarded = rewarded;
        }
        
        /// <summary>
        /// la liste des zones de ce type
        /// </summary>
        private readonly Dictionary<string,AbsAdMostAd> ads = new ();
        /// <summary>
        /// la pub en cours de lecture (ou la derniere lue)
        /// </summary>
        private AbsAdMostAd ad;
        /// <summary>
        /// true si ce groupe est un groupe de videos rewarded
        /// </summary>
        private readonly bool Rewarded;

        private void Log( string str )
        {
            AdMostImplementation.Log( $"[AdMostAd<{(Rewarded ? "Rewarded" : "Intersti")}>] {str}" );
        }

        /// <summary>
        /// Charger une video pour une zone
        /// </summary>
        /// <param name="zoneID"></param>
        public void LoadAd( string zoneID )
        {
            Log( $"LoadAd zoneID:{zoneID}" );

            if( string.IsNullOrWhiteSpace( zoneID ) )
            {
                Log( "LoadAd FAIL zoneID null" );
                return;
            }
            
            if( !ads.TryGetValue( zoneID, out var _ad ) )
            {
                _ad = Rewarded ? new AdRewarded( zoneID ) : new AdInterstitial( zoneID );
                ads[zoneID] = _ad;
                
                Log( "zoneID added to Dictionary" );
            }

            if( !_ad.IsLoaded() )
            {
                Log( "startLoading" );
                _ad.LoadAd();
            }
            else
            {
                Log( "already loaded" );
            }
        }

        public bool PlayAd( string zoneID, IAdMostAdDelegate eventsReceiver )
        {
            Log( $"PlayAd zoneID:{zoneID}" );

            if( string.IsNullOrWhiteSpace( zoneID ) )
            {
                Log( "PlayAd FAIL zoneID null" );
                return false;
            }
            
            if( ads.TryGetValue( zoneID, out var _ad ) )
            {
                _ad = Rewarded ? new AdRewarded( zoneID ) : new AdInterstitial( zoneID );
                _ad.Show();

                // au cas ou on en ait toujours un
                ad?.ClearEventReceiver();
                
                // mettre a jour l'ad en cours de lecture
                ad = _ad;
                ad.SetEventReceiver( eventsReceiver );
                
                Log( "PlayAd startPlaying" );
                return true;
            }
            
            Log( "PlayAd zone non existante" );
            return false;
        }
        
        public bool IsAdReady( string zoneID )
        {
            Log( $"IsAdReady zoneID:{zoneID}" );

            if( string.IsNullOrWhiteSpace( zoneID ) )
            {
                Log( "IsAdReady FAIL zone null" );
                return false;
            }

            if( ads.TryGetValue( zoneID, out var _ad ) )
            {
                return _ad?.IsLoaded() ?? false;
            }
            
            Log( "IsAdReady FAIL zone non existante" );
            return false;
        }
    }
    
    // ADMOB VIDEO INTERFACE

    internal abstract class AbsAdMostAd
    {
        protected string _adZoneId;
        private   string NetworkName;
        private   double eCPM;
        
        private   IAdMostAdDelegate EventReceiver;
        public void SetEventReceiver( IAdMostAdDelegate receiver )
        {
            EventReceiver = receiver;
        }
        public void ClearEventReceiver()
        {
            EventReceiver = null;
        }

        protected void OnAdLoaded( string networkName, double ecpm )
        {
            NbFail      = 0;
            NetworkName = networkName;
            eCPM        = ecpm;
        }

        protected void OnAdFailedToLoad( string errorMsg )
        {
            var delay = ++NbFail * 5;
            if( delay > 60 )
                delay = 60;
            delay *= 1000;
            ReloadAdDelayed( delay );
        }

        protected void OnAdShow()
        {
            EventReceiver?.OnAdShow( NetworkName, eCPM );
        }

        protected void OnAdFailedToShow()
        {
            EventReceiver?.OnAdFailToShow();
        }

        protected void OnAdClicked( string networkName )
        {
            EventReceiver?.OnAdClick();
        }

        protected void OnAdImpression( AMRAd ad )
        {
            EventReceiver?.OnAdImpression( ad );
        }

        protected void OnAdComplete()
        {
            EventReceiver?.OnAdComplete();
        }

        protected void OnAdDismissed()
        {
            EventReceiver?.OnAdDismissed();
            EventReceiver = null;
        }

        protected void OnAdStatusChanged( int status )
        {
            // nothing to do here
        }

        protected void OnAdRewarded( double reward )
        {
            // nothing to do here
        }


        public virtual void LoadAd()                    { throw new NotImplementedException(); }
        public virtual bool IsLoaded()                  { throw new NotImplementedException(); }
        public virtual void Show(string tag = null)     { throw new NotImplementedException(); }
        
        
        
        private int                     NbFail;
        private CancellationTokenSource AsyncCancellationToken;
        private void ReloadAdDelayed( int ms )
        {
            AsyncCancellationToken?.Cancel();
            AsyncCancellationToken?.Dispose();
            AsyncCancellationToken = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( LoadAd, ms, AsyncCancellationToken.Token );
        }
        
        protected void ResetCancelationToken()
        {
            AsyncCancellationToken?.Cancel();
            AsyncCancellationToken?.Dispose();
            AsyncCancellationToken = null;
        }
    }
    
    // ADMOST IMPLEMENTATION OF REWARDED AD
    internal class AdRewarded : AbsAdMostAd
    {
        private readonly AMRRewardedVideoView ad;
        
        public AdRewarded( string adZoneId )
        {
            _adZoneId = adZoneId;
            ad        = new AMRRewardedVideoView();
            
            ad.setOnReady( OnAdLoaded );
            ad.setOnFail( OnAdFailedToLoad );
            ad.setOnShow( OnAdShow );
            ad.setOnFailToShow( OnAdFailedToShow );
            ad.setOnClick( OnAdClicked );
            ad.setOnImpression( OnAdImpression );
            ad.setOnComplete( OnAdComplete );
            ad.setOnDismiss( OnAdDismissed );
            ad.setOnStatusChange( OnAdStatusChanged );
            ad.setOnReward( OnAdRewarded );
        }

        public override void LoadAd()
        {
            ResetCancelationToken();
            ad.loadRewardedVideoForZoneId( _adZoneId, _adZoneId, false );
        }

        public override bool IsLoaded() => ad.isReadyToShow();

        public override void Show( string tag = null )
        {
            ad.showRewardedVideo( tag );
        }
    }
    
    
    
    // ADMOST IMPLEMENTATION OF INTERSTITIALS AD
    internal class AdInterstitial : AbsAdMostAd
    {
        private readonly AMRInterstitialView ad;
        
        public AdInterstitial( string adZoneId )
        {
            _adZoneId = adZoneId;
            ad        = new AMRInterstitialView();
            
            ad.setOnReady( OnAdLoaded );
            ad.setOnFail( OnAdFailedToLoad );
            ad.setOnShow( OnAdShow );
            ad.setOnFailToShow( OnAdFailedToShow );
            ad.setOnClick( OnAdClicked );
            ad.setOnImpression( OnAdImpression );
            ad.setOnDismiss( OnAdDismissed );
            ad.setOnStatusChange( OnAdStatusChanged );
        }

        public override void LoadAd()
        {
            ResetCancelationToken();
            ad.loadInterstitialForZoneId( _adZoneId, _adZoneId, false );
        }

        public override bool IsLoaded() => ad.isReadyToShow();

        public override void Show( string tag = null )
        {
            ad.showInterstitial( tag );
        }
    }
    
    
    public interface IAdMostAdDelegate
    {
        void OnAdShow( string networkName, double ecpm );
        void OnAdImpression(AMRAd ad);
        void OnAdClick();
        void OnAdDismissed();
        void OnAdComplete();
        void OnAdFailToShow();
    }
}