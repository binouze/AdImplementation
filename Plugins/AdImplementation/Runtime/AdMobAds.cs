using System;
using System.Threading;
using GoogleMobileAds.Api;

namespace com.binouze
{
    internal class AdMobAd
    {
        /// <summary>
        /// true si une pub est dispo
        /// </summary>
        public bool AdAvailable => ad?.IsLoaded() ?? false;
        
        private IAdMobAd ad;
        private string   adUnit;
        private int      NbFailPlay;
        private bool     Rewarded;

        private void Log( string str )
        {
            var type = Rewarded ? "Rewarded" : "Intersti";
            AdMobImplementation.Log( $"[AdMobAd<{type}>] {str}" );
        }

        /// <summary>
        /// DO NOT USE
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="isRewarded"></param>
        public void SetupAd( string adUnitId, bool isRewarded )
        {
            Rewarded = isRewarded;
            
            Log( $"SetupAd {adUnitId}" );

            // au cas ou
            Dispose();

            //Create
            ad         = isRewarded ? AdRewarded.Create( adUnitId ) : AdInterstitial.Create( adUnitId );
            adUnit     = adUnitId;
            NbFailPlay = 0;
            
            //Subscribe to events
            ad.OnAdClosed              += AdClosed;
            ad.OnAdLoaded              += AdLoaded;
            ad.OnAdFailedToLoad        += AdFailedLoad;
            ad.OnAdFailedToShow        += AdFailedShow;
            ad.OnAdOpening             += AdShown;
            ad.OnPaidEvent             += AdPaidEvent;
            ad.OnAdDidRecordImpression += AdDidRecordImpression;
            ad.OnUserEarnedReward      += UserEarnedReward;
            
            // Preload AD
            LoadAd();
        }

        public void Dispose()
        {
            ad?.Destroy();
            OnAdComplete = null;
            CancelDelayedCall();
        }

        private void CancelDelayedCall()
        {
            AsyncCancellationToken?.Cancel();
            AsyncCancellationToken?.Dispose();
            AsyncCancellationToken = null;
        }
        
        private Action<bool> OnAdComplete;

        private void CallOnComplete( bool status )
        {
            Log( $"CallOnComplete {status} {OnAdComplete}" );
            
            OnAdComplete?.Invoke( status );
            OnAdComplete = null;

            CancelDelayedCall();
        }
        
        /// <summary>
        /// lancer l'affichage d'une pub
        /// </summary>
        public void ShowAd( Action<bool> onComplete )
        {
            Log( $"ShowAd {AdAvailable}" );
            
            if( AdAvailable )
            {
                OnAdComplete = onComplete;
                ad.Show();
            }
            else
            {
                onComplete?.Invoke( false );
            }
        }

        private void ReloadAd()
        {
            Log( "ReloadAd" );
            //ResetupAd();
            CancelDelayedCall();
            LoadAd();
        }
        private void ResetupAd()
        {
            Log( "ResetupAd" );
            
            CancelDelayedCall();
            SetupAd( adUnit, Rewarded );
        }
        private void LoadAd()
        {
            Log( $"LoadAd AdAvailable:{AdAvailable} - AdActive:{AdImplementation.IsActive}" );
            
            if( AdImplementation.IsActive )
                ad.LoadAd( AdMobImplementation.CreateAdRequest() );
            else
                DelayCall( ReloadAd, 60_000 );
        }

        private void AdLoaded( object sender, EventArgs e )
        {
            var infos = ad.GetResponseInfo();
            Log( $"AdLoaded {infos.GetMediationAdapterClassName()} {infos}" );

            if( Rewarded && !string.IsNullOrEmpty(AdImplementation.UserId) )
            {
                // Create and pass the SSV options to the rewarded ad.
                var options = new ServerSideVerificationOptions.Builder()
                             .SetUserId( AdImplementation.UserId )
                             .SetCustomData( "CUSTOMDATAS" )
                             .Build();
                ad.SetServerSideVerificationOptions(options);
                
                Log( $"Adding user ID for SSV: {AdImplementation.UserId}" );
            }
        }

        private CancellationTokenSource AsyncCancellationToken;
        private void DelayCall( Action a, int ms )
        {
            AsyncCancellationToken?.Cancel();
            AsyncCancellationToken?.Dispose();
            AsyncCancellationToken = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( a, ms, AsyncCancellationToken.Token );
            
            //AdsAsyncUtils.Delay( ms, AsyncCancellationToken.Token );
        }
        
        private void AdFailedLoad( object sender, AdFailedToLoadEventArgs adFailedToLoadEventArgs )
        {
            var loadAdError = adFailedToLoadEventArgs.LoadAdError;

            // Gets the domain from which the error came.
            var domain = loadAdError.GetDomain();

            // Gets the error code. See
            // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest
            // and https://developers.google.com/admob/ios/api/reference/Enums/GADErrorCode
            // for a list of possible codes.
            var code = loadAdError.GetCode();

            // Gets an error message.
            // For example "Account not approved yet". See
            // https://support.google.com/admob/answer/9905175 for explanations of
            // common errors.
            var message = loadAdError.GetMessage();

            // Gets the cause of the error, if available.
            //var underlyingError = loadAdError.GetCause();

            try
            {
                Log( "AdFailedLoad" );
                Log( $"             -> domain: {domain}" );
                Log( $"             -> code: {code}" );
                Log( $"             -> message: {message}" );
                Log( $"             -> error: {loadAdError}" );
                //Log( $"             -> underlyingError: {underlyingError}" );
            }
            catch( Exception )
            {
                Log( "--> ERROR CATCHED DURING LOG" );
            }
            

            var erreur = AdMobErrorCodeHelper.GetErrorCodeFromInteger( code );
            switch( erreur )
            {
                case AdMobErrorCode.AD_ALREADY_USED:
                case AdMobErrorCode.TIMEOUT:
                case AdMobErrorCode.NO_FILL:
                case AdMobErrorCode.MEDIATION_NO_FILL:
                    // dans ces cas la on recheck plus tard si une ad est dispo
                    DelayCall(ReloadAd, 20_000);
                    break;
                
                case AdMobErrorCode.INVALID_REQUEST:
                case AdMobErrorCode.NETWORK_ERROR:
                case AdMobErrorCode.SERVER_ERROR:
                case AdMobErrorCode.MEDIATION_DATA_ERROR:
                case AdMobErrorCode.MEDIATION_ADAPTER_ERROR:
                case AdMobErrorCode.MEDIATION_INVALID_AD:
                case AdMobErrorCode.INTERNAL_ERROR:
                case AdMobErrorCode.INVALID_ARGUMENT:
                case AdMobErrorCode.INVALID_RESPONSE:
                    // dans ces cas la on attend un moment et on resetup
                    DelayCall(ResetupAd, 20_000);
                    break;
                
                case AdMobErrorCode.OS_NOT_SUPPORTED:
                case AdMobErrorCode.APP_ID_MISSING:
                    // bah la y'a pas besoin de reessayer ...
                    break;
                
                default:
                    // par defaut, on reload dans 20secondes
                    DelayCall(ReloadAd, 20_000);
                    break;
            }
        }
        
        private void AdShown( object sender, EventArgs eventArgs )
        {
            Log( "AdShown" );
            NbFailPlay = 0;
        }
        
        private void AdClosed( object sender, EventArgs e )
        {
            Log( "AdClosed" );

            // on charge la video suivante
            // apparement parfois si on appelle ca direct ca crash l'app Android
            // donc on force l'appel sur le main thresd
            AdsAsyncUtils.CallOnMainThread( ReloadAd );
            
            // sur les video rewarded on recois le close avant le reward event donc on delaye un peu
            if( Rewarded )
                DelayCall( DoAdClosed, 2_000 );
            else
                DoAdClosed();
        }

        private void DoAdClosed()
        {
            Log( "DoAdClosed" );
            CallOnComplete( false );
        }
        
        private void AdDidRecordImpression( object sender, EventArgs e )
        {
            Log( $"AdDidRecordImpression {e}" );
        }

        private void UserEarnedReward( object sender, Reward args )
        {
            var type   = args.Type;
            var amount = args.Amount;
            Log( $"UserEarnedReward event received for {amount} {type}" );
            CallOnComplete( true );
        }
        
        private void AdFailedShow( object sender, AdErrorEventArgs adErrorEventArgs )
        {
            Log( $"AdFailedShow NbFailPlay:{NbFailPlay} - Error:{adErrorEventArgs.AdError}" );
            
            CallOnComplete( false );

            if( ++NbFailPlay > 5 )
            {
                Log( " -> TOO MUCH FAIL... RESETUP" );
                ResetupAd();
            }
        }
        
        private void AdPaidEvent( object sender, AdValueEventArgs args )
        {
            // preferred analytics server directly within this callback.
            var responseInfo = ad.GetResponseInfo();
            
            /*
            var adValue      = args.AdValue;
            var valueMicros  = adValue.Value;
            var currencyCode = adValue.CurrencyCode;
            var precision    = adValue.Precision;
            
            var responseId   = responseInfo.GetResponseId();

            var loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
            var adSourceId                = loadedAdapterResponseInfo.AdSourceId;
            var adSourceInstanceId        = loadedAdapterResponseInfo.AdSourceInstanceId;
            var adSourceInstanceName      = loadedAdapterResponseInfo.AdSourceInstanceName;
            var adSourceName              = loadedAdapterResponseInfo.AdSourceName;
            var adapterClassName          = loadedAdapterResponseInfo.AdapterClassName;
            var latencyMillis             = loadedAdapterResponseInfo.LatencyMillis;
            var credentials               = loadedAdapterResponseInfo.AdUnitMapping;

            var extras                 = responseInfo.GetResponseExtras();
            var mediationGroupName     = extras["mediation_group_name"];
            var mediationABTestName    = extras["mediation_ab_test_name"];
            var mediationABTestVariant = extras["mediation_ab_test_variant"];
            */
            
            AdMobImplementation.OnImpressionDatas( args, responseInfo, Rewarded );
        }
    }
    
    // ADMOB VIDEO INTERFACE
    internal interface IAdMobAd
    {
        public event EventHandler<EventArgs>               OnAdLoaded;
        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;
        public event EventHandler<EventArgs>               OnAdOpening;
        public event EventHandler<EventArgs>               OnAdClosed;
        public event EventHandler<AdErrorEventArgs>        OnAdFailedToShow;
        public event EventHandler<EventArgs>               OnAdDidRecordImpression;
        public event EventHandler<Reward>                  OnUserEarnedReward;
        public event EventHandler<AdValueEventArgs>        OnPaidEvent;

        public void         LoadAd(AdRequest request);
        public bool         IsLoaded();
        public void         Show();
        public void         SetServerSideVerificationOptions( ServerSideVerificationOptions serverSideVerificationOptions );
        public Reward       GetRewardItem();
        public void         Destroy();
        public ResponseInfo GetResponseInfo();
    }
    
    // IADMOBAD IMPLEMENTATION OF REWARDED AD
    internal class AdRewarded : Factory<AdRewarded, string>, IAdMobAd
    {
        [Obsolete( FactoryMessage, true)]
        public AdRewarded() { }
        
        private RewardedAd ad;
        private string     adUnitId;
        
        protected override void Initialize( string adUnitID )
        {
            ad = new RewardedAd( adUnitID );
            
            ad.OnAdLoaded += ((sender, args) =>
            {
                OnAdLoaded?.Invoke( sender, args );
            });
            
            ad.OnAdFailedToLoad += ((sender, args) =>
            {
                OnAdFailedToLoad?.Invoke(sender,args);
            });
            
            ad.OnAdOpening += ((sender, args) =>
            {
                OnAdOpening?.Invoke(sender, args);
            });
            
            ad.OnAdClosed += ((sender, args) =>
            {
                OnAdClosed?.Invoke(sender, args);
            });
            
            ad.OnAdFailedToShow += ((sender, args) =>
            {
                OnAdFailedToShow?.Invoke(sender, args);
            });
            
            ad.OnAdDidRecordImpression += ((sender, args) =>
            {
                OnAdDidRecordImpression?.Invoke(sender, args);
            });
            
            ad.OnUserEarnedReward += ((sender, args) =>
            {
                OnUserEarnedReward?.Invoke(sender, args);
            });
            
            ad.OnPaidEvent += ((sender, args) =>
            {
                OnPaidEvent?.Invoke(sender, args);
            });
        }
        
        public event EventHandler<EventArgs>               OnAdLoaded;
        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;
        public event EventHandler<EventArgs>               OnAdOpening;
        public event EventHandler<EventArgs>               OnAdClosed;
        public event EventHandler<AdErrorEventArgs>        OnAdFailedToShow;
        public event EventHandler<EventArgs>               OnAdDidRecordImpression;
        public event EventHandler<Reward>                  OnUserEarnedReward;
        public event EventHandler<AdValueEventArgs>        OnPaidEvent;
        
        public void LoadAd( AdRequest request )
        {
            ad.LoadAd( request );
        }

        public bool IsLoaded() => ad.IsLoaded();

        public void Show()
        {
            ad.Show();
        }

        public void SetServerSideVerificationOptions( ServerSideVerificationOptions serverSideVerificationOptions )
        {
            ad.SetServerSideVerificationOptions( serverSideVerificationOptions );
        }

        public Reward GetRewardItem() => ad.GetRewardItem();

        public void Destroy()
        {
            ad.Destroy();
            ad = null;
        }

        public ResponseInfo GetResponseInfo() => ad.GetResponseInfo();
    }
    
    // IADMOBAD IMPLEMENTATION OF INTERSTITIALS AD
    internal class AdInterstitial : Factory<AdInterstitial, string>, IAdMobAd
    {
        [Obsolete( FactoryMessage, true)]
        public AdInterstitial() { }
        
        private InterstitialAd ad;
        
        protected override void Initialize( string adUnitID )
        {
            ad = new InterstitialAd( adUnitID );
            
            ad.OnAdLoaded += ((sender, args) =>
            {
                OnAdLoaded?.Invoke( sender, args );
            });
            
            ad.OnAdFailedToLoad += ((sender, args) =>
            {
                OnAdFailedToLoad?.Invoke(sender,args);
            });
            
            ad.OnAdOpening += ((sender, args) =>
            {
                OnAdOpening?.Invoke(sender, args);
            });
            
            ad.OnAdClosed += ((sender, args) =>
            {
                OnAdClosed?.Invoke(sender, args);
            });
            
            ad.OnAdFailedToShow += ((sender, args) =>
            {
                OnAdFailedToShow?.Invoke(sender, args);
            });
            
            ad.OnAdDidRecordImpression += ((sender, args) =>
            {
                OnAdDidRecordImpression?.Invoke(sender, args);
            });

            ad.OnPaidEvent += ((sender, args) =>
            {
                OnPaidEvent?.Invoke(sender, args);
            });
        }
        
        public event EventHandler<EventArgs>               OnAdLoaded;
        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;
        public event EventHandler<EventArgs>               OnAdOpening;
        public event EventHandler<EventArgs>               OnAdClosed;
        public event EventHandler<AdErrorEventArgs>        OnAdFailedToShow;
        public event EventHandler<EventArgs>               OnAdDidRecordImpression;
        public event EventHandler<Reward>                  OnUserEarnedReward;
        public event EventHandler<AdValueEventArgs>        OnPaidEvent;
        
        public void LoadAd( AdRequest request )
        {
            ad.LoadAd( request );
        }

        public bool IsLoaded() => ad.IsLoaded();

        public void Show()
        {
            ad.Show();
        }

        public void SetServerSideVerificationOptions( ServerSideVerificationOptions serverSideVerificationOptions )
        {
            //NOT AVAILABLE ON INTERSTITIALS VIDEOS
        }

        public Reward GetRewardItem() => null;

        public void Destroy()
        {
            ad.Destroy();
            ad = null;
        }

        public ResponseInfo GetResponseInfo() => ad.GetResponseInfo();
    }
    
    
    // FACTORY ABSTRACT CLASS
    internal abstract class Factory<TSelf, TParameter> where TSelf : Factory<TSelf, TParameter>, new()
    {
        protected const string FactoryMessage = "Use YourClass.Create(...) instead";
        public static TSelf Create(TParameter parameter)
        {
            var me = new TSelf();
            me.Initialize( parameter );
            return me;
        }

        [Obsolete( FactoryMessage, true)]
        protected Factory() { }

        protected virtual void Initialize(TParameter parameter) { }
    }
}