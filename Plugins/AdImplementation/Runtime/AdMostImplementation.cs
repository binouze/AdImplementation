using System;
using System.Threading;
using AMR;
using UnityEngine;

namespace com.binouze
{
    public class AdMostImplementation : IAdImplementation
    {
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//          
//                       ██    ██  █████  ██████  ██  █████  ██████  ██      ███████ ███████ 
//                       ██    ██ ██   ██ ██   ██ ██ ██   ██ ██   ██ ██      ██      ██      
//                       ██    ██ ███████ ██████  ██ ███████ ██████  ██      █████   ███████ 
//                        ██  ██  ██   ██ ██   ██ ██ ██   ██ ██   ██ ██      ██           ██ 
//                         ████   ██   ██ ██   ██ ██ ██   ██ ██████  ███████ ███████ ███████ 
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  


        private string AppID;
        private string AdRewarUnit;
        private string AdInterUnit;
        private bool   AdSupported;
        
        private static bool IsInit;
        private bool IsInitComplete;

        private static bool         AdPlaying;
        private static Action<bool> OnAdPlayComplete;

        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//   
//    ██████  ██    ██ ██████  ██      ██  ██████         ███    ███ ███████ ████████ ██   ██  ██████  ██████  ███████ 
//    ██   ██ ██    ██ ██   ██ ██      ██ ██              ████  ████ ██         ██    ██   ██ ██    ██ ██   ██ ██      
//    ██████  ██    ██ ██████  ██      ██ ██              ██ ████ ██ █████      ██    ███████ ██    ██ ██   ██ ███████ 
//    ██      ██    ██ ██   ██ ██      ██ ██              ██  ██  ██ ██         ██    ██   ██ ██    ██ ██   ██      ██ 
//    ██       ██████  ██████  ███████ ██  ██████         ██      ██ ███████    ██    ██   ██  ██████  ██████  ███████ 
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
        
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
        /// true si la configuration actuelle supporte les pubs et que le SDK a bien ete initialise 
        /// </summary>
        public bool IsAdSupported() => AdSupported && IsInitComplete;
        
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

        /// <summary>
        /// definir le User ID du client
        /// </summary>
        /// <param name="id"></param>
        public void SetUserID( string id )
        {
            if( IsInitComplete )
                AMRSDK.setUserId( id );
        }
        
        /// <summary>
        /// TEST ONLY: ouvrir ARM Test Suite
        /// ne fonctionne que si l'IDFA du device a ete ajoute au dashboard AdMost
        /// </summary>
        public void OpenTestSuite()
        {
            AMRSDK.startTestSuite(new [] {AdInterUnit,AdRewarUnit});
        }
        
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
                RewardedComplete = false; 
                AdPlaying        = true;
                OnAdPlayComplete = OnComplete;
                AMRSDK.showRewardedVideo();
            }
            else
            {
                OnComplete?.Invoke( false );
            }
        }
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//   
//            ██ ███    ██ ██ ████████ ██  █████  ██      ██ ███████  █████  ████████ ██  ██████  ███    ██ 
//            ██ ████   ██ ██    ██    ██ ██   ██ ██      ██ ██      ██   ██    ██    ██ ██    ██ ████   ██ 
//            ██ ██ ██  ██ ██    ██    ██ ███████ ██      ██ ███████ ███████    ██    ██ ██    ██ ██ ██  ██ 
//            ██ ██  ██ ██ ██    ██    ██ ██   ██ ██      ██      ██ ██   ██    ██    ██ ██    ██ ██  ██ ██ 
//            ██ ██   ████ ██    ██    ██ ██   ██ ███████ ██ ███████ ██   ██    ██    ██  ██████  ██   ████ 
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       


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

            config.UserConsent   = AdImplementation.IsDebug || AdImplementation.ConsentResponse == "OK" ? "1" : "0";
            config.SubjectToGDPR = AdImplementation.ConsentType     == "GDPR" ? "1" : "0";
            config.SubjectToCCPA = AdImplementation.ConsentResponse == "CCPA" ? "1" : "0";
            config.IsUserChild   = "0";
            
            AMRSDK.startWithConfig(config, OnSDKDidInitialize);
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
                
                // preload Ads
                AMRSDK.loadInterstitial();
                AMRSDK.loadRewardedVideo();

                AdPlaying      = false;
                IsInitComplete = true;
            }
        }

        
//  █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████     
//        
//  ██████  ██████  ██ ██    ██  █████  ████████ ███████         ███    ███ ███████ ████████ ██   ██  ██████  ██████  ███████ 
//  ██   ██ ██   ██ ██ ██    ██ ██   ██    ██    ██              ████  ████ ██         ██    ██   ██ ██    ██ ██   ██ ██      
//  ██████  ██████  ██ ██    ██ ███████    ██    █████           ██ ████ ██ █████      ██    ███████ ██    ██ ██   ██ ███████ 
//  ██      ██   ██ ██  ██  ██  ██   ██    ██    ██              ██  ██  ██ ██         ██    ██   ██ ██    ██ ██   ██      ██ 
//  ██      ██   ██ ██   ████   ██   ██    ██    ███████         ██      ██ ███████    ██    ██   ██  ██████  ██████  ███████ 
//
//  █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████            

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
            AdImplementation.Log( $"[AdMostImplementation] {str}" );
        }
        
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//        
//           ██ ███    ██ ████████ ███████ ██████  ███████ ████████ ██ ████████ ██  █████  ██      ███████ 
//           ██ ████   ██    ██    ██      ██   ██ ██         ██    ██    ██    ██ ██   ██ ██      ██      
//           ██ ██ ██  ██    ██    █████   ██████  ███████    ██    ██    ██    ██ ███████ ██      ███████ 
//           ██ ██  ██ ██    ██    ██      ██   ██      ██    ██    ██    ██    ██ ██   ██ ██           ██ 
//           ██ ██   ████    ██    ███████ ██   ██ ███████    ██    ██    ██    ██ ██   ██ ███████ ███████
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       


        private static readonly AdViewInfo              InterstitialAdInfo = new ();
        private static          int                     NbFailInter;
        private static          CancellationTokenSource AsyncCancellationTokenInterstitial;
        private static void ReloadInterstitialDelayed( int ms )
        {
            Log( $"ReloadInterstitialDelayed {ms}" );
            
            AsyncCancellationTokenInterstitial?.Cancel();
            AsyncCancellationTokenInterstitial?.Dispose();
            AsyncCancellationTokenInterstitial = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( ReloadInterstitial, ms, AsyncCancellationTokenInterstitial.Token );
        }
        
        private static void ReloadInterstitial()
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
        private static void OnInterstitialReady( string networkName, double ecpm )
        {
            InterstitialAdInfo.Init( networkName, ecpm );
            NbFailInter = 0;
            Log( $"OnInterstitialReady networkName:{networkName} ecpm:{ecpm}" );
        }

        /// <summary>
        /// It indicates that the interstitial ad received no-fill response from all of its placements. Therefore, the ad can not be shown. You may choose to try loading it again.
        /// </summary>
        /// <param name="errorMessage"></param>
        private static void OnInterstitialFail( string errorMessage )
        {
            var delay = ++NbFailInter * 5;
            if( delay > 60 )
                delay = 60;
            delay *= 1000;
            
            Log( $"OnInterstitialFail delayretry:{delay} errorMessage:{errorMessage}" );
            ReloadInterstitialDelayed( delay );
        }

        /// <summary>
        /// It indicates that the interstitial ad is failed to show.
        /// </summary>
        private static void OnInterstitialFailToShow()
        {
            Log( "OnInterstitialFailToShow" );
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the loaded interstitial ad is shown to the user.
        /// </summary>
        private static void OnInterstitialShow()
        {
            Log( "OnInterstitialShow" );
        }

        /// <summary>
        /// It indicates that the impression counted by the ad network and ad revenue paid.
        /// </summary>
        /// <param name="ad"></param>
        private static void OnInterstitialImpression( AMRAd ad )
        {
            Log( $"OnInterstitialImpression ad:{ad.Network}-{ad.ZoneId}-{ad.Currency}-{ad.Revenue}" );
            
            InterstitialAdInfo.Revenus         = ad.Revenue;
            InterstitialAdInfo.RevenusCurrency = ad.Currency;
            AdImplementation.OnImpressionDatas?.Invoke( ImpressionDatasFromAdMostDatas( ad, false ) );
        }

        /// <summary>
        /// It indicates that the interstitial ad is clicked.
        /// </summary>
        /// <param name="networkName"></param>
        private static void OnInterstitialClick( string networkName )
        {
            Log( $"OnInterstitialClick networkName:{networkName}" );
            InterstitialAdInfo.NbClicks++;
            AdImplementation.OnAdClicked?.Invoke(networkName, false);
        }

        /// <summary>
        /// It indicates that the interstitial ad is closed by clicking cross button/back button
        /// </summary>
        private static void OnInterstitialDismiss()
        {
            Log( "OnInterstitialDismiss" );
            ReloadInterstitial();
            AdComplete( true );
            
            // send info statistic about this ad
            AdImplementation.OnAdViewInfo?.Invoke( InterstitialAdInfo );
        }

        /// <summary>
        /// It indicates that the interstitial ad status changed. (ex: frequency capping finished)
        /// </summary>
        /// <param name="status"></param>
        private static void OnInterstitialStatusChange( int status )
        {
            Log( $"OnInterstitialStatusChange status:{status}" );
        }
        
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//        
//                        ██████  ███████ ██     ██  █████  ██████  ██████  ███████ ██████                              
//                        ██   ██ ██      ██     ██ ██   ██ ██   ██ ██   ██ ██      ██   ██                             
//                        ██████  █████   ██  █  ██ ███████ ██████  ██   ██ █████   ██   ██                             
//                        ██   ██ ██      ██ ███ ██ ██   ██ ██   ██ ██   ██ ██      ██   ██                             
//                        ██   ██ ███████  ███ ███  ██   ██ ██   ██ ██████  ███████ ██████      
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████


        private static readonly AdViewInfo              RewardAdInfo = new ();
        private static          bool                    RewardedComplete;
        private static          int                     NbFailReward;
        private static          CancellationTokenSource AsyncCancellationTokenRewarded;
        private static void ReloadRewardedDelayed( int ms )
        {
            Log( $"ReloadRewardedDelayed {ms}" );
            
            AsyncCancellationTokenRewarded?.Cancel();
            AsyncCancellationTokenRewarded?.Dispose();
            AsyncCancellationTokenRewarded = new CancellationTokenSource();
            
            AdsAsyncUtils.DelayCall( ReloadRewarded, ms, AsyncCancellationTokenRewarded.Token );
        }
        
        private static void ReloadRewarded()
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
        private static void OnVideoReady( string networkName, double ecpm )
        {
            RewardAdInfo.Init( networkName, ecpm, true );
            NbFailReward = 0;
            Log( $"OnVideoReady networkName:{networkName} ecpm:{ecpm}" );
        }

        /// <summary>
        /// It indicates that the rewarded video ad received no-fill response from all of its placements.
        /// Therefore, the ad can not be shown. You may choose to try loading it again.
        /// </summary>
        /// <param name="errorMessage"></param>
        private static void OnVideoFail( string errorMessage )
        {
            var delay = ++NbFailReward * 5;
            if( delay > 60 )
                delay = 60;
            delay *= 1000;
            
            Log( $"OnVideoFail delay:{delay} errorMessage:{errorMessage}" );
            ReloadRewardedDelayed( delay );
        }

        /// <summary>
        /// It indicates that the loaded rewarded video ad is shown to the user.(Note: It does not mean that the user deserves a reward)
        /// It is immediately called after the loaded ad is shown to the user using AMR.AMRSDK.showRewardedVideo()
        /// </summary>
        private static void OnVideoShow()
        {
            Log( "OnVideoShow" );
        }

        /// <summary>
        /// It indicates that the rewarded video ad is failed to show.
        /// </summary>
        private static void OnVideoFailToShow()
        {
            Log( "OnVideoFailToShow" );
            AdComplete( false );
        }

        /// <summary>
        /// It indicates that the impression has been counted by the ad network and ad revenue got paid.
        /// </summary>
        /// <param name="ad"></param>
        private static void OnVideoImpression( AMRAd ad )
        {
            Log( $"OnVideoImpression ad:{ad.Network}-{ad.ZoneId}-{ad.Currency}-{ad.Revenue}" );
            
            RewardAdInfo.Revenus         = ad.Revenue;
            RewardAdInfo.RevenusCurrency = ad.Currency;
            AdImplementation.OnImpressionDatas?.Invoke( ImpressionDatasFromAdMostDatas( ad, true ) );
        }

        /// <summary>
        /// It indicates that the rewarded video ad is clicked.
        /// </summary>
        /// <param name="networkName"></param>
        private static void OnVideoClick( string networkName )
        {
            Log( $"OnVideoClick networkName:{networkName}" );
            
            RewardAdInfo.NbClicks++;
            AdImplementation.OnAdClicked?.Invoke(networkName, true);
        }

        /// <summary>
        /// It indicates that the rewarded video ad is closed by clicking cross button/back button.
        /// It does not mean that the user deserves to receive a reward. You need to check whether OnVideoComplete callback is called or not.
        /// </summary>
        private static void OnVideoDismiss()
        {
            Log( "OnVideoDismiss" );
            ReloadRewarded();
            AdComplete( RewardedComplete );
            
            // send info statistic about this ad
            AdImplementation.OnAdViewInfo?.Invoke( RewardAdInfo );
        }

        /// <summary>
        /// It indicates that the user deserves to receive a reward. You may need to store this information in a variable and give a reward
        /// to the user after OnVideoDismiss() callback is called by showing some animations for instance.
        /// Note: If OnVideoComplete callback is called for the ad, it is always called before OnVideoDismiss() callback.
        /// </summary>
        private static void OnVideoComplete()
        {
            Log( "OnVideoComplete" );
            RewardedComplete      = true;
            RewardAdInfo.Complete = true;
        }

        /// <summary>
        /// It indicates that rewarded video status changed. (ex: frequency capping finished)
        /// </summary>
        /// <param name="status"></param>
        private static void OnVideoStatusChange( int status )
        {
            Log( $"OnVideoStatusChange status:{status}" );
        }
    }

    public class AdViewInfo
    {
        public string Network;
        public bool   Complete;
        public int    NbClicks;
        public string Type;
        public double eCPM;
        public double Revenus;
        public string RevenusCurrency;
        public string UserID;

        public void Init( string network, double ecpm, bool rewarded = false)
        {
            Network         = network;
            eCPM            = ecpm;
            Revenus         = 0;
            RevenusCurrency = "USD";
            Complete        = false;
            NbClicks        = 0;
            Type            = rewarded ? "rewarded" : "interstitial";
            UserID          = AdImplementation.UserId;
        }
    }
}