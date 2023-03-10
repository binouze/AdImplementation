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
        internal static void LogZone( string str, string zoneID, bool Rewarded )
        {
            AdMostImplementation.Log( $"[AdMostAd<{(Rewarded ? "Rewarded" : "Intersti")}>][AdZone<{zoneID}>] {str}" );
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
                //_ad = Rewarded ? new AdRewarded( zoneID ) : new AdInterstitial( zoneID );

                // on reset les event receiver de la video precedente
                ad?.ClearEventReceiver();
                
                // mettre a jour l'ad en cours de lecture
                ad = _ad;
                ad.SetEventReceiver( eventsReceiver );
                _ad.Show();
                
                Log( "PlayAd startPlaying" );
                return true;
            }
            
            Log( "PlayAd zone non existante" );
            return false;
        }
        
        public bool IsAdReady( string zoneID )
        {
            if( string.IsNullOrWhiteSpace( zoneID ) )
            {
                Log( "IsAdReady FAIL zone null" );
                return false;
            }

            var ready = false;
            if( ads.TryGetValue( zoneID, out var _ad ) )
            {
                ready = _ad?.IsLoaded() ?? false;
            }
            
            Log( $"IsAdReady zoneID:{zoneID} - ready:{ready}" );
            
            return ready;
        }
    }
    
    // ADMOB VIDEO INTERFACE

    internal abstract class AbsAdMostAd
    {
        protected void Log( string str )
        {
            AdMostAd.LogZone( str, _adZoneId, Rewarded );
        }
        
        protected string _adZoneId;
        private   string NetworkName;
        private   double eCPM;
        protected bool   Rewarded;
        
        private   IAdMostAdDelegate EventReceiver;
        public void SetEventReceiver( IAdMostAdDelegate receiver )
        {
            EventReceiver = receiver;
        }
        public void ClearEventReceiver()
        {
            EventReceiver = null;
        }

        protected void OnAdLoaded( string zoneID, string networkName, double ecpm )
        {
            if( zoneID != _adZoneId )
                return;

            Log( $"OnAdLoaded {networkName} {ecpm}" );
            
            NbFail      = 0;
            NetworkName = networkName;
            eCPM        = ecpm;
        }

        protected void OnAdFailedToLoad( string zoneID, string errorMsg )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( $"OnAdFailedToLoad {errorMsg}" );
            
            var delay = ++NbFail * 5;
            if( delay > 60 )
                delay = 60;
            delay *= 1000;
            ReloadAdDelayed( delay );
        }

        protected void OnAdShow( string zoneID )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( "OnAdShow" );
            EventReceiver?.OnAdShow( NetworkName, eCPM );
        }

        protected void OnAdFailedToShow( string zoneID )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( "OnAdFailedToShow" );
            EventReceiver?.OnAdFailToShow();
        }

        protected void OnAdClicked( string zoneID, string networkName )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( $"OnAdClicked {networkName}" );
            EventReceiver?.OnAdClick();
        }

        protected void OnAdImpression( AMRAd ad )
        {
            Log( $"OnAdImpression {ad.Network}" );
            EventReceiver?.OnAdImpression( ad );
        }

        protected void OnAdComplete( string zoneID )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( "OnAdComplete" );
            EventReceiver?.OnAdComplete();
        }

        protected void OnAdDismissed( string zoneID )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( "OnAdDismissed" );
            EventReceiver?.OnAdDismissed();
            EventReceiver = null;
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
        private readonly AMRRewardedVideoAd ad;
        
        public AdRewarded( string adZoneId )
        {
            Rewarded  = true;
            _adZoneId = adZoneId;
            ad        = new AMRRewardedVideoAd
            {
                AndroidZoneId = adZoneId,
                iOSZoneId     = adZoneId
            };
            
            ad.SetOnVideoReady( OnAdLoaded );
            ad.SetOnVideoFail( OnAdFailedToLoad );
            ad.SetOnVideoShow( OnAdShow );
            ad.SetOnVideoFailToShow( OnAdFailedToShow );
            ad.SetOnVideoClick( OnAdClicked );
            ad.SetOnVideoImpression( OnAdImpression );
            ad.SetOnVideoComplete( OnAdComplete );
            ad.SetOnVideoDismiss( OnAdDismissed );
        }

        public override void LoadAd()
        {
            Log( "LoadAd" );
            ResetCancelationToken();
            ad.LoadRewardedVideo();
        }

        public override bool IsLoaded() => ad.Status == AMRRewardedVideoAd.AdStatus.Loaded;

        public override void Show( string tag = null )
        {
            Log( "Show" );
            ad.ShowRewardedVideo( tag );
        }
    }
    
    
    
    // ADMOST IMPLEMENTATION OF INTERSTITIALS AD
    internal class AdInterstitial : AbsAdMostAd
    {
        private readonly AMRInterstitialAd ad;
        
        public AdInterstitial( string adZoneId )
        {
            _adZoneId = adZoneId;
            ad        = new AMRInterstitialAd
            {
                AndroidZoneId = adZoneId,
                iOSZoneId     = adZoneId
            };

            ad.SetOnInterstitialReady( OnAdLoaded );
            ad.SetOnInterstitialFail( OnAdFailedToLoad );
            ad.SetOnInterstitialShow( OnAdShow );
            ad.SetOnInterstitialFailToShow( OnAdFailedToShow );
            ad.SetOnInterstitialClick( OnAdClicked );
            ad.SetOnInterstitialImpression( OnAdImpression );
            ad.SetOnInterstitialDismiss( OnAdDismissed );
        }

        public override void LoadAd()
        {
            Log( "LoadAd" );
            ResetCancelationToken();
            ad.LoadInterstitial();
        }

        public override bool IsLoaded() => ad.Status == AMRInterstitialAd.AdStatus.Loaded;

        public override void Show( string tag = null )
        {
            Log( "Show" );
            ad.ShowInterstitial( tag );
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