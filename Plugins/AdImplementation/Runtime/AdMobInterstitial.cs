﻿using System;
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
            Log( $"[AdMobInterstitial] AdFailedLoad {sender} {adFailedToLoadEventArgs.LoadAdError}" );

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

            // All of this information is available via the error's toString() method.
            Log("[AdMobInterstitial] AdFailedLoad - Load error string: " + loadAdError);

            // Get response information, which may include results of mediation requests.
            var responseInfo = loadAdError.GetResponseInfo();
            Log("[AdMobInterstitial] AdFailedLoad - Response info: " + responseInfo);
           
            //TODO1 trouver les cas ou il faut faire un resetup plutot qu'un reload
            
            // par defaut, on reload dans 20secondes
            DelayCall(ReloadAd, 30_000);
            
            /*switch( adFailedToLoadEventArgs.LoadAdError )
            {
                // when it is not connected or initialized, let it break out and try again.
                case LoadAdError:
                case LoadError.NetworkError:
                case LoadError.MissingMandatoryMemberValues:
                case LoadError.Unknown:
                    Debug.Log("[AdsInterstitial] Detected a not connected or unitialized type of error during ad load call. Invoking TryInitServices in 20 secs");
                    DelayCall(Resetup, 20_000);
                    break;
 
                // it is connected.
                case LoadError.TooManyLoadRequests:
                    Debug.Log("[AdsInterstitial] Detected Too many ad load requests type error during ad load. Canceling all invoke calls, and trying to load ad again after period of 20 seconds");
                    DelayCall(ReloadAd, 20_000);
                    break;
 
                case LoadError.NoFill:
                    Debug.Log("[AdsInterstitial] Detected No-Fill, trying a load call in 20 seconds");
                    DelayCall(ReloadAd, 20_000);
                    break;

                case LoadError.AdUnitLoading:
                case LoadError.AdUnitShowing:
                default:
                    Debug.Log("[AdsInterstitial] Detected default condition of 2 possible AdUnit Showing Or ad Unit loading, returning");
                    return;
            }*/
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
            // TODO: Send the impression-level ad revenue information to your
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
            var mediationABTestVariant = extras["mediation_ab_test_variant"];*/
            
            OnImpressionDatas( args, responseInfo, false );
        }
    }
}