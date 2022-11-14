using System;
using System.Threading;
using GoogleMobileAds.Api;

using static com.binouze.AdMobImplementation;

namespace com.binouze
{
    public class AdMobInterstitial
    {
        /// <summary>
        /// true si une pub est dispo
        /// </summary>
        public bool AdAvailable => ad?.IsLoaded() ?? false;
        
        private InterstitialAd ad;
        private string         adUnit;
        private int            NbRetryPlay;

        private void Resetup()
        {
            SetupAd( adUnit );
        }
        
        /// <summary>
        /// DO NOT USE
        /// </summary>
        /// <param name="adUnitId"></param>
        public void SetupAd( string adUnitId )
        {
            Log( $"[AdsInterstitial] SetupAd {adUnitId}" );

            // au cas ou
            Dispose();

            //Create
            ad     = new InterstitialAd( adUnitId );
            adUnit = adUnitId;
            
            //Subscribe to events
            ad.OnAdClosed              += AdClosed;
            ad.OnAdLoaded              += AdLoaded;
            ad.OnAdFailedToLoad        += AdFailedLoad;
            ad.OnAdFailedToShow        += AdFailedShow;
            ad.OnAdOpening             += AdShown;
            ad.OnPaidEvent             += AdPaidEvent;
            ad.OnAdDidRecordImpression += AdDidRecordImpression;
            
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
            Log( $"[AdMobInterstitial] CallOnComplete {status} {OnAdComplete}" );
            
            OnAdComplete?.Invoke( status );
            OnAdComplete = null;
            
            if( status )
                NbRetryPlay = 0;
            
            CancelDelayedCall();
        }
        
        /// <summary>
        /// lancer l'affichage d'une pub
        /// </summary>
        public void ShowAd( Action<bool> onComplete )
        {
            Log( $"[AdMobInterstitial] ShowAd {AdAvailable}" );
            
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
            CancelDelayedCall();
            LoadAd();
        }
        private void ResetupAd()
        {
            CancelDelayedCall();
            Resetup();
        }
        private void LoadAd()
        {
            Log( $"[AdMobInterstitial] LoadAd {AdAvailable}" );
            ad.LoadAd( CreateAdRequest() );
        }

        private void AdLoaded( object sender, EventArgs e )
        {
            Log( $"[AdMobInterstitial] AdLoaded {sender} {e}" );
        }

        private CancellationTokenSource AsyncCancellationToken;
        private async void DelayCall( Action a, int ms )
        {
            AsyncCancellationToken?.Cancel();
            AsyncCancellationToken?.Dispose();
            AsyncCancellationToken = new CancellationTokenSource();
            
            var ok = await AdsAsyncUtils.Delay( ms, AsyncCancellationToken.Token );
            if( !ok )
                return;
            
            a?.Invoke();
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
            var underlyingError = loadAdError.GetCause();
            
            Log($"[AdMobInterstitial] AdFailedLoad -> \n\tdomain: {domain}\n\tcode: {code}\n\tmessage: {message}\n\tunderlyingError: {underlyingError}\n\n{adFailedToLoadEventArgs.LoadAdError}");
            
            // gerer l'erreur
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
        
        private static void AdShown( object sender, EventArgs eventArgs )
        {
            Log( "[AdMobInterstitial] AdShown" );
        }
        
        private void AdClosed( object sender, EventArgs e )
        {
            Log( "[AdMobInterstitial] AdClosed" );
            CallOnComplete( true );
            ReloadAd();
        }
        
        private void AdDidRecordImpression( object sender, EventArgs e )
        {
            Log( $"[AdMobInterstitial] AdDidRecordImpression {e}" );
        }

        private void AdFailedShow( object sender, AdErrorEventArgs adErrorEventArgs )
        {
            Log( $"[AdMobInterstitial] AdFailedShow {adErrorEventArgs.AdError}" );
            
            if( NbRetryPlay++ < 5 )
            {
                Log( $"[AdMobInterstitial] AdFailedShow -> retry {NbRetryPlay}/5" );
                ShowAd( OnAdComplete );
            }
            else
            {
                CallOnComplete( false );
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
            
            OnImpressionDatas( args, responseInfo, false );
        }
    }
}