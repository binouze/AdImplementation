using System;
using System.Collections.Generic;
using System.Threading;
using AMR;

namespace com.binouze
{
    internal class AdMostAd
    {
        /// <summary>
        /// Vider les zones: leurs objets AMR, leurs event receivers et leurs rechargements differes datent de
        /// la session de jeu precedente (voir AdImplementation.ResetStatics). Ce controleur est tenu par un
        /// statique readonly d'AdMostImplementation, il survit donc au Play Mode.
        /// </summary>
        internal void ResetStatics()
        {
            foreach( var kv in ads )
                kv.Value?.Liberer();
    
            ads.Clear();
            ad = null;
        }
        
        
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
            // chargee mais perimee (voir AdImplementation.SetDureeVieMaxPub): on en recharge une neuve
            else if( _ad.EstPerimee() )
            {
                _ad.RechargerCarPerimee();
            }
            else
            {
                Log( "already loaded" );
            }
        }

        /// <summary>
        /// Start playing an Ad
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="eventsReceiver"></param>
        /// <param name="tag"></param>
        /// <param name="ssvExtra"></param>
        /// <returns></returns>
        public bool PlayAd( string zoneID, IAdMostAdDelegate eventsReceiver, string tag = null, Dictionary<string,string> ssvExtra = null )
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
                _ad.Show(tag, ssvExtra);
                
                Log( "PlayAd startPlaying" );
                return true;
            }
            
            Log( "PlayAd zone non existante" );
            return false;
        }
        
        /// <summary>
        /// returns true if an Ad is ready to display for a specific zone
        /// </summary>
        /// <param name="zoneID"></param>
        /// <returns></returns>
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
                // une pub perimee reste annoncee disponible: a l'affichage, AdMost recharge a la volee si la regie
                // la refuse (et ca passe parfois). La peremption n'est traitee qu'au prechargement (LoadAd), que
                // le jeu relance assez souvent pour qu'une pub depasse rarement sa duree de vie.
                ready = _ad?.IsLoaded() ?? false;
            }
            
            Log( $"IsAdReady zoneID:{zoneID} - ready:{ready}" );
            
            return ready;
        }
        
        /// <summary>
        /// la regie de la pub chargee pour cet emplacement (null si aucune)
        /// </summary>
        public string GetReseau( string zoneID )
        {
            return !string.IsNullOrWhiteSpace( zoneID ) && ads.TryGetValue( zoneID, out var _ad ) ? _ad?.Reseau : null;
        }

        /// <summary>
        /// l'age en minutes de la pub chargee pour cet emplacement (-1 si aucune)
        /// </summary>
        public int GetAgeMinutes( string zoneID )
        {
            return !string.IsNullOrWhiteSpace( zoneID ) && ads.TryGetValue( zoneID, out var _ad ) && _ad != null ? _ad.AgeMinutes() : -1;
        }

        /// <summary>
        /// true si la pub chargee pour cet emplacement a depasse la duree de vie max de sa regie (false si aucune)
        /// </summary>
        public bool EstPerimee( string zoneID )
        {
            return !string.IsNullOrWhiteSpace( zoneID ) && ads.TryGetValue( zoneID, out var _ad ) && _ad != null && _ad.EstPerimee();
        }

        /// <summary>
        /// returns true if an Ad is currently loading for a specific zone
        /// </summary>
        /// <param name="zoneID"></param>
        /// <returns></returns>
        public bool IsLoading( string zoneID )
        {
            if( string.IsNullOrWhiteSpace( zoneID ) )
            {
                Log( "IsAdReady FAIL zone null" );
                return false;
            }

            var ready = false;
            if( ads.TryGetValue( zoneID, out var _ad ) )
            {
                ready = _ad?.IsLoading() ?? false;
            }
            
            Log( $"IsAdReady zoneID:{zoneID} - ready:{ready}" );
            
            return ready;
        }
    }
    
    // ADMOST VIDEO INTERFACE

    internal abstract class AbsAdMostAd
    {
        /// <summary>
        /// Couper ce qui est arme sur la session de jeu precedente: rechargement differe et callbacks vers un
        /// receveur qui n'existe plus (voir AdImplementation.ResetStatics).
        /// </summary>
        internal void Liberer()
        {
            ClearEventReceiver();
            ResetCancelationToken();
        }
        
        protected void Log( string str )
        {
            AdMostAd.LogZone( str, _adZoneId, Rewarded );
        }
        
        protected string _adZoneId;
        private   string NetworkName;
        private   double eCPM;
        protected bool   Rewarded;
        /// <summary>heure (UTC) du dernier chargement reussi. DateTime et pas Time.realtimeSinceStartup: les
        /// callbacks AMR peuvent arriver hors du main thread, et l'horloge doit continuer app en arriere-plan</summary>
        private   DateTime HeureChargement;

        /// <summary>
        /// true si la pub chargee a depasse la duree de vie max de sa regie (AdImplementation.SetDureeVieMaxPub).
        /// Aucun signal d'expiration n'existe cote AdMost: sans cette limite, un prechargement relance sur une pub
        /// deja chargee ne ferait rien, et la pub finirait refusee par la regie a l'affichage.
        /// </summary>
        public bool EstPerimee()
        {
            if( !IsLoaded() || HeureChargement == default )
                return false;

            var ageMinutes = (DateTime.UtcNow - HeureChargement).TotalMinutes;
            return ageMinutes >= AdImplementation.GetDureeVieMaxPubMinutes( NetworkName );
        }

        /// <summary>la regie qui a servi la pub chargee</summary>
        public string Reseau => NetworkName;

        /// <summary>
        /// age de la pub chargee en minutes (-1 si aucune pub chargee)
        /// </summary>
        public int AgeMinutes()
        {
            if( HeureChargement == default )
                return -1;

            return (int)(DateTime.UtcNow - HeureChargement).TotalMinutes;
        }

        /// <summary>
        /// relancer un chargement a la place d'une pub perimee
        /// </summary>
        public void RechargerCarPerimee()
        {
            Log( $"pub perimee ({NetworkName}, chargee il y a {(DateTime.UtcNow - HeureChargement).TotalMinutes:F0} min, duree de vie {AdImplementation.GetDureeVieMaxPubMinutes( NetworkName )} min) : rechargement" );
            HeureChargement = default;
            LoadAd();
        }
        
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
            
            NbFail          = 0;
            NetworkName     = networkName;
            eCPM            = ecpm;
            HeureChargement = DateTime.UtcNow;
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
        
        protected void OnAdReward( string zoneID, double amount )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( $"OnAdReward {amount}" );
            EventReceiver?.OnAdReward(amount);
        }

        protected void OnAdDismissed( string zoneID )
        {
            if( zoneID != _adZoneId )
                return;
            
            Log( "OnAdDismissed" );
            EventReceiver?.OnAdDismissed();
            EventReceiver = null;
        }


        public virtual void LoadAd()                                                           { throw new NotImplementedException(); }
        public virtual bool IsLoaded()                                                         { throw new NotImplementedException(); }
        public virtual bool IsLoading()                                                        { throw new NotImplementedException(); }
        public virtual void Show(string tag = null, Dictionary<string,string> ssvExtra = null) { throw new NotImplementedException(); }
        
        
        
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
            ad.SetOnVideoReward( OnAdReward );
        }

        public override void LoadAd()
        {
            Log( $"LoadAd {ad.Status}" );
            ResetCancelationToken();
            ad.LoadRewardedVideo();
        }

        public override bool IsLoaded() => ad.Status == AMRRewardedVideoAd.AdStatus.Loaded;
        public override bool IsLoading() => ad.Status == AMRRewardedVideoAd.AdStatus.Loading;

        public override void Show( string tag = null, Dictionary<string,string> ssvExtra = null )
        {
            Log( $"Show {ad.Status}" );
            
            // adding tag to ssv custom datas if defined
            /*if( tag != null )
            {
                ad.setSSVCustomData( new Dictionary<string, string>{ {"tag", tag} } );
            }*/
            
            var data = new Dictionary<string, string>();
            
            if( tag != null )
            {
                data["tag"] = tag;
            }
            
            if( ssvExtra != null )
            {
                foreach( var kv in ssvExtra )
                    data[kv.Key] = kv.Value;
            }
            
            // custom data for the s2s callback
            ad.setSSVCustomData( data );
            
            // play video
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
            Log( $"LoadAd {ad.Status}" );
            ResetCancelationToken();
            ad.LoadInterstitial();
        }

        public override bool IsLoaded()  => ad.Status == AMRInterstitialAd.AdStatus.Loaded;
        public override bool IsLoading() => ad.Status == AMRInterstitialAd.AdStatus.Loading;

        public override void Show( string tag = null, Dictionary<string,string> ssvExtra = null )
        {
            Log( $"Show {ad.Status}" );
            ad.ShowInterstitial( tag );
        }
    }
    
    
    public interface IAdMostAdDelegate
    {
        void OnAdShow( string     networkName, double ecpm );
        void OnAdImpression(AMRAd ad);
        void OnAdClick();
        void OnAdDismissed();
        void OnAdComplete();
        void OnAdReward(double amount);
        void OnAdFailToShow();
    }
}