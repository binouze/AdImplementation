using System;
using System.Threading;
using AMR;
using UnityEngine;

namespace com.binouze
{
    public class AdMostImplementation : IAdImplementation
    {
        private string AppID       = "testAPP";
        private string AdRewarUnit = "testRewarded";
        private string AdInterUnit = "testInter";
        private bool   AdSupported;
        
        private static bool IsInit;
        private bool IsInitComplete;

        private static bool         AdPlaying;
        private static Action<bool> OnAdPlayComplete;

        /// <summary>
        /// true si on est ok pour lancer une pub rewarded
        /// </summary>
        public bool HasRewardedAvailable()
        {
            Log( $"HasRewardedAvailable: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - AdAvailable:{AMRSDK.isRewardedVideoReady()}" );
            return IsInitComplete && !AdPlaying && AMRSDK.isRewardedVideoReady();
        }

        /// <summary>
        /// true si on est ok pour lancer une pub interstitial
        /// </summary>
        public bool HasInterstitialAvailable()
        {
            Log( $"HasInterstitialAvailable: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - AdAvailable:{AMRSDK.isInterstitialReady()}" );
            return IsInitComplete && !AdPlaying && AMRSDK.isInterstitialReady();
        }

        /// <summary>
        /// true si la configuration actuelle supporte les pubs
        /// </summary>
        public bool IsAdSupported() => AdSupported && IsInitComplete;
        
        #region INITIALISATION

        /// <summary>
        /// definir les ids des emplacement pubs
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="rewardedId"></param>
        /// <param name="interstitialId"></param>
        public void SetIds( string appID , string rewardedId, string interstitialId )
        {
            AppID       = appID;
            AdRewarUnit = rewardedId;
            AdInterUnit = interstitialId;
            AdSupported = !string.IsNullOrEmpty( AppID ) && (!string.IsNullOrEmpty( AdRewarUnit ) || !string.IsNullOrEmpty( AdInterUnit ));
        }

        public void SetUserID( string id )
        {
            if( IsInitComplete )
                AMRSDK.setUserId( id );
        }
        
        public void Initialize()
        {
            if( !AdSupported )
                return;

            if( IsInit )
                return;
            IsInit = true;

            Log( $"Initialize debug:{AdImplementation.IsDebug}" );
            
            var config = new AMRSdkConfig();
            
            #if UNITY_IOS
            config.ApplicationIdIOS       = AppID;
            config.InterstitialIdIOS      = AdInterUnit;
            config.RewardedVideoIdIOS     = AdRewarUnit;
            #elif UNITY_ANDROID
            config.ApplicationIdAndroid   = AppID;
            config.InterstitialIdAndroid  = AdInterUnit;
            config.RewardedVideoIdAndroid = AdRewarUnit;
            #endif

            config.UserConsent   = AdImplementation.ConsentResponse == "OK"   ? "1" : "0";
            config.SubjectToGDPR = AdImplementation.ConsentType     == "GDPR" ? "1" : "0";
            config.SubjectToCCPA = AdImplementation.ConsentResponse == "CCPA" ? "1" : "0";
            config.IsUserChild   = "0";
            
            AMRSDK.startWithConfig(config, OnSDKDidInitialize);
        }

        public void OpenTestSuite()
        {
            // en mode debug, on active le test suite pour les rewarded et les intersticielles
            if( AdImplementation.IsDebug )
                AMRSDK.startTestSuite(new [] {AdInterUnit,AdRewarUnit});
        }
        
        private void OnSDKDidInitialize( bool success, string error )
        {
            if( !success )
            {
                Debug.Log( $"[AdMostImplementation] FAil iniitalize SDK {error}" );
                Debug.LogException( new Exception("[AdMostImplementation] FAil initialize SDK") );
            }
            else
            {
                // interstitials
                AMRSDK.setOnInterstitialReady(OnInterstitialReady);
                AMRSDK.setOnInterstitialFail(OnInterstitialFail);
                AMRSDK.setOnInterstitialFailToShow(OnInterstitialFailToShow);
                AMRSDK.setOnInterstitialShow(OnInterstitialShow);
                AMRSDK.setOnInterstitialImpression(OnInterstitialImpression);
                AMRSDK.setOnInterstitialClick(OnInterstitialClick);
                AMRSDK.setOnInterstitialDismiss(OnInterstitialDismiss);
                AMRSDK.setOnInterstitialStatusChange(OnInterstitialStatusChange);
                
                // rewarded
                AMRSDK.setOnRewardedVideoReady(OnVideoReady);
                AMRSDK.setOnRewardedVideoFail(OnVideoFail);
                AMRSDK.setOnRewardedVideoFailToShow(OnVideoFailToShow);
                AMRSDK.setOnRewardedVideoShow(OnVideoShow);
                AMRSDK.setOnRewardedVideoImpression(OnVideoImpression);
                AMRSDK.setOnRewardedVideoClick(OnVideoClick);
                AMRSDK.setOnRewardedVideoDismiss(OnVideoDismiss);
                AMRSDK.setOnRewardedVideoComplete(OnVideoComplete);
                AMRSDK.setOnRewardedVideoStatusChange(OnVideoStatusChange);
                
                // setting user id if it was defined before initialization complete
                if( !string.IsNullOrEmpty(AdImplementation.UserId) )
                    AMRSDK.setUserId(AdImplementation.UserId);
                
                PreloadAds();
                
                IsInitComplete = true;
            }
        }

        private static void PreloadAds()
        {
            Log( "PreloadAds" );
            
            AMRSDK.loadInterstitial();
            AMRSDK.loadRewardedVideo();

            AdPlaying = false;
        }

        #endregion
        
        /// <summary>
        /// Afficher une video interstitielle
        /// </summary>
        /// <param name="OnComplete"></param>
        public void ShowInterstitial( Action<bool> OnComplete )
        {
            Log( "ShowInterstitial" );
            
            if( HasInterstitialAvailable() )
            {
                AdPlaying        = true;
                OnAdPlayComplete = OnComplete;
                AMRSDK.showInterstitial();
            }
            else
            {
                OnComplete?.Invoke( false );
            }
        }
        
        /// <summary>
        /// Afficher une video Rewarded
        /// </summary>
        /// <param name="OnComplete"></param>
        public void ShowRewarded( Action<bool> OnComplete )
        {
            Log( "ShowRewarded" );
            
            if( HasRewardedAvailable() )
            {
                AdPlaying        = true;
                OnAdPlayComplete = OnComplete;
                AMRSDK.showRewardedVideo();
            }
            else
            {
                OnComplete?.Invoke( false );
            }
        }

        private static void AdComplete( bool ok )
        {
            Log( $"AdComplete ok: {ok}" );
            
            AdPlaying = false;
            OnAdPlayComplete?.Invoke( ok );
            OnAdPlayComplete = null;
        }

        private static ImpressionDatas ImpressionDatasFromAdMostDatas( AMRAd ad, bool rewarded )
        {
            return new ImpressionDatas
            {
                ImpressionRevenue = ad.Revenue,
                Precision         = "",
                Rewarded          = rewarded,
                CurrencyCode      = ad.Currency,
                AdSourceName      = ad.Network,
                AdPlacementName   = ad.ZoneId,
                AdGroupName       = ad.AdSpaceId
            };
        }

        private static void Log( string str )
        {
            AdImplementation.Log( $"[AdMobImplementation] {str}" );
        }
        
        
        
        // GESTION INTERSTITALS

        private CancellationTokenSource AsyncCancellationTokenInterstitial;
        private void ReloadInterstitialDelayed( int ms )
        {
            Log( $"ReloadInterstitialDelayed {ms}" );
            
            AsyncCancellationTokenInterstitial?.Cancel();
            AsyncCancellationTokenInterstitial?.Dispose();
            AsyncCancellationTokenInterstitial = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( ReloadInterstitial, ms, AsyncCancellationTokenInterstitial.Token );
        }
        
        private void ReloadInterstitial()
        {
            Log( "ReloadInterstitial" );
            
            AsyncCancellationTokenInterstitial?.Cancel();
            AsyncCancellationTokenInterstitial?.Dispose();
            AsyncCancellationTokenInterstitial = null;
            
            AMRSDK.loadInterstitial();
        }
        
        /// <summary>
        /// It indicates that the interstitial ad is loaded and ready to show.
        /// </summary>
        /// <param name="networkName"></param>
        /// <param name="ecpm"></param>
        private void OnInterstitialReady( string networkName, double ecpm )
        {
            Log( $"OnInterstitialReady networkName:{networkName} ecpm:{ecpm}" );
        }

        /// <summary>
        /// It indicates that the interstitial ad received no-fill response from all of its placements. Therefore, the ad can not be shown. You may choose to try loading it again.
        /// </summary>
        /// <param name="errorMessage"></param>
        private void OnInterstitialFail( string errorMessage )
        {
            Log( $"OnInterstitialFail errorMessage:{errorMessage}" );
            ReloadInterstitialDelayed( 60_000 );
        }

        /// <summary>
        /// It indicates that the interstitial ad is failed to show.
        /// </summary>
        private void OnInterstitialFailToShow()
        {
            Log( "OnInterstitialFailToShow" );
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the loaded interstitial ad is shown to the user.
        /// </summary>
        private void OnInterstitialShow()
        {
            Log( "OnInterstitialShow" );
        }

        /// <summary>
        /// It indicates that the impression counted by the ad network and ad revenue paid.
        /// </summary>
        /// <param name="ad"></param>
        private void OnInterstitialImpression( AMRAd ad )
        {
            Log( "OnInterstitialImpression" );
            AdImplementation.OnImpressionDatas?.Invoke( ImpressionDatasFromAdMostDatas( ad, false ) );
        }

        /// <summary>
        /// It indicates that the interstitial ad is clicked.
        /// </summary>
        /// <param name="networkName"></param>
        private void OnInterstitialClick( string networkName )
        {
            Log( $"OnInterstitialClick networkName:{networkName}" );
            AdImplementation.OnAdClicked?.Invoke(networkName, false);
        }

        /// <summary>
        /// It indicates that the interstitial ad is closed by clicking cross button/back button
        /// </summary>
        private void OnInterstitialDismiss()
        {
            Log( "OnInterstitialDismiss" );
            ReloadInterstitial();
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the interstitial ad status changed. (ex: frequency capping finished)
        /// </summary>
        /// <param name="status"></param>
        private void OnInterstitialStatusChange( int status )
        {
            Log( $"OnInterstitialStatusChange status:{status}" );
        }
        
        
        
        // Rewarded

        private CancellationTokenSource AsyncCancellationTokenRewarded;
        private void ReloadRewardedDelayed( int ms )
        {
            Log( $"ReloadRewardedDelayed {ms}" );
            
            AsyncCancellationTokenRewarded?.Cancel();
            AsyncCancellationTokenRewarded?.Dispose();
            AsyncCancellationTokenRewarded = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( ReloadRewarded, ms, AsyncCancellationTokenRewarded.Token );
        }
        
        private void ReloadRewarded()
        {
            Log( "ReloadRewarded" );
            
            AsyncCancellationTokenRewarded?.Cancel();
            AsyncCancellationTokenRewarded?.Dispose();
            AsyncCancellationTokenRewarded = null;
            
            AMRSDK.loadRewardedVideo();
        }
        
        /// <summary>
        /// It indicates that the rewarded video ad is loaded and ready to show.
        /// </summary>
        /// <param name="networkName"></param>
        /// <param name="ecpm"></param>
        public void OnVideoReady( string networkName, double ecpm )
        {
            Log( $"OnVideoReady networkName:{networkName} ecpm:{ecpm}" );
        }

        /// <summary>
        /// It indicates that the rewarded video ad received no-fill response from all of its placements.
        /// Therefore, the ad can not be shown. You may choose to try loading it again.
        /// </summary>
        /// <param name="errorMessage"></param>
        public void OnVideoFail( string errorMessage )
        {
            Log( $"OnVideoFail errorMessage:{errorMessage}" );
            ReloadRewardedDelayed( 60_000 );
        }

        /// <summary>
        /// It indicates that the loaded rewarded video ad is shown to the user.(Note: It does not mean that the user deserves a reward)
        /// It is immediately called after the loaded ad is shown to the user using AMR.AMRSDK.showRewardedVideo()
        /// </summary>
        public void OnVideoShow()
        {
            Log( "OnVideoShow" );
        }

        /// <summary>
        /// It indicates that the rewarded video ad is failed to show.
        /// </summary>
        public void OnVideoFailToShow()
        {
            Log( "OnVideoFailToShow" );
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the impression has been counted by the ad network and ad revenue got paid.
        /// </summary>
        /// <param name="ad"></param>
        public void OnVideoImpression( AMRAd ad )
        {
            Log( $"OnVideoImpression ad:{ad}" );
            AdImplementation.OnImpressionDatas?.Invoke( ImpressionDatasFromAdMostDatas( ad, true ) );
        }

        /// <summary>
        /// It indicates that the rewarded video ad is clicked.
        /// </summary>
        /// <param name="networkName"></param>
        public void OnVideoClick( string networkName )
        {
            Log( $"OnVideoClick networkName:{networkName}" );
            AdImplementation.OnAdClicked?.Invoke(networkName, true);
        }

        /// <summary>
        /// It indicates that the rewarded video ad is closed by clicking cross button/back button.
        /// It does not mean that the user deserves to receive a reward. You need to check whether OnVideoComplete callback is called or not.
        /// </summary>
        public void OnVideoDismiss()
        {
            Log( "OnVideoDismiss" );
            ReloadRewarded();
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the user deserves to receive a reward. You may need to store this information in a variable and give a reward
        /// to the user after OnVideoDismiss() callback is called by showing some animations for instance.
        /// Note: If OnVideoComplete callback is called for the ad, it is always called before OnVideoDismiss() callback.
        /// </summary>
        public void OnVideoComplete()
        {
            Log( "OnVideoComplete" );
            AdComplete( true );
        }

        /// <summary>
        /// It indicates that rewarded video status changed. (ex: frequency capping finished)
        /// </summary>
        /// <param name="status"></param>
        public void OnVideoStatusChange( int status )
        {
            Log( $"OnVideoStatusChange status:{status}" );
        }
    }
}