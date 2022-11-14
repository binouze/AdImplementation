using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.AdColony;

namespace com.binouze
{
    public class AdMobImplementation : IAdImplementation
    {
        private string AdRewarUnit = "testRewarded";
        private string AdInterUnit = "testInter";
        private bool   AdSupported;
        
        private static bool IsInit;
        private bool IsInitComplete;

        private AdMobRewarded     _rewardedAd;
        private AdMobInterstitial _interstitialAd;
        
        private static bool         AdPlaying;
        private static Action<bool> OnAdPlayComplete;

        private static AdMobInterstitial AdInterstitial; 
        private static AdMobRewarded     AdRewarded; 
        
        /// <summary>
        /// true si on est ok pour lancer une pub rewarded
        /// </summary>
        public bool HasRewardedAvailable() => IsInitComplete && !AdPlaying && (AdRewarded?.AdAvailable ?? false);
        /// <summary>
        /// true si on est ok pour lancer une pub interstitial
        /// </summary>
        public bool HasInterstitialAvailable() => IsInitComplete && !AdPlaying && (AdInterstitial?.AdAvailable ?? false);
        
        
        #region INITIALISATION
        
        /// <summary>
        /// definir les ids des emplacement pubs
        /// </summary>
        /// <param name="rewardedId"></param>
        /// <param name="interstitialId"></param>
        public void SetUnitIds( string rewardedId, string interstitialId )
        {
            AdRewarUnit = rewardedId;
            AdInterUnit = interstitialId;
            AdSupported = !string.IsNullOrEmpty( AdRewarUnit ) || !string.IsNullOrEmpty( AdInterUnit );
        }

        public void Initialize()
        {
            if( !AdSupported )
                return;
            
            if( IsInit )
                return;
            IsInit = true;

            if( AdImplementation.IsDebug )
            {
                GoogleUserMessagingPlatform.SetDebugLogging( true );
                GoogleUserMessagingPlatform.SetDebugMode( AdImplementation.UMPDevice, true );
            }
            GoogleUserMessagingPlatform.Initialize();
            
            //GoogleUserMessagingPlatform.SetOnStatusChangedListener( MajConsentStatus );
            //GoogleUserMessagingPlatform.Initialize();
            // ADCOLONY SPECIFIC
            //AdColonyAppOptions.SetGDPRRequired(true);
            //AdColonyAppOptions.SetGDPRConsentString("1");
            // APPLOVIN SPECIFIC
            //AppLovin.SetHasUserConsent(true);
            //AppLovin.SetIsAgeRestrictedUser(false);
            // UNITYADS SPECIFIC
            //UnityAds.SetConsentMetaData("gdpr.consent",    true);
            //UnityAds.SetConsentMetaData("privacy.consent", true);
            // VUNGLE SPECIFIC
            //Vungle.UpdateConsentStatus(VungleConsent.ACCEPTED);
            
            MobileAds.SetiOSAppPauseOnBackground(true);

            if( AdImplementation.IsDebug )
            {
                var deviceIds = new List<string>
                {
                    AdRequest.TestDeviceSimulator,
                    AdImplementation.AdMobDevice
                };

                // Configure TagForChildDirectedTreatment and test device IDs.
                var requestConfiguration = new RequestConfiguration.Builder()
                                          .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.False)
                                          .SetTestDeviceIds(deviceIds).build();
                MobileAds.SetRequestConfiguration(requestConfiguration);
            }
            else
            {
                // Configure TagForChildDirectedTreatment and test device IDs.
                var requestConfiguration = new RequestConfiguration.Builder()
                                          .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.False).build();
                MobileAds.SetRequestConfiguration(requestConfiguration);
            }

            MobileAds.Initialize( initStatus =>
            {
                Log( $"[AdMobImplementation] Initialize Complete: {initStatus}" );
                var map = initStatus.getAdapterStatusMap();
                CheckAdapterInitialization( map );
            } );
        }

        private async void CheckAdapterInitialization( Dictionary<string,AdapterStatus> map )
        {
            var cnt = 0;
            while( !IsInitComplete )
            {
                Log( "[AdMobImplementation] CheckAdapterInitialization" );
                
                var ok = true;
                foreach( var (className, status) in map )
                {
                    switch( status.InitializationState )
                    {
                        case AdapterState.NotReady:
                            // The adapter initialization did not complete.
                            Log($"[AdMobImplementation] Adapter NOT READY: {className}");
                            break;
                        
                        case AdapterState.Ready:
                            // The adapter was successfully initialized.
                            Log($"[AdMobImplementation] Adapter READY: {className}");
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    ok &= status.InitializationState == AdapterState.Ready;
                }

                if( ok || cnt++ > 50 )
                {
                    // si il y a des soucis de config des adaptateurs externe au bout de 50 tours, tampis on s'en passera
                    ok = true;
                    
                    AdInterstitial = new AdMobInterstitial();
                    AdInterstitial.SetupAd( AdInterUnit );

                    AdRewarded = new AdMobRewarded();
                    AdRewarded.SetupAd( AdRewarUnit );
                }
                
                IsInitComplete = ok;
                
                await AdsAsyncUtils.Delay( 500 );
            }
            
            // a priori ici on est bon
        }
        
        #endregion
        
        /// <summary>
        /// Afficher une video interstitielle
        /// </summary>
        /// <param name="OnComplete"></param>
        public void ShowInterstitial( Action<bool> OnComplete )
        {
            if( HasInterstitialAvailable() )
            {
                AdPlaying        = true;
                OnAdPlayComplete = OnComplete;
                AdInterstitial.ShowAd( AdComplete );
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
            if( HasRewardedAvailable() )
            {
                AdPlaying        = true;
                OnAdPlayComplete = OnComplete;
                AdRewarded.ShowAd( AdComplete );
            }
            else
            {
                OnComplete?.Invoke( false );
            }
        }

        private static void AdComplete( bool ok )
        {
            AdPlaying = false;
            OnAdPlayComplete?.Invoke( ok );
            OnAdPlayComplete = null;
        }
        
        public static void OnImpressionDatas( AdValueEventArgs valueEvent, ResponseInfo responseInfos, bool rewarded )
        {
            if( valueEvent?.AdValue != null )
            {
                Log( $"[AdMobImplementation] OnImpressionDatas responseInfos {responseInfos}" );
                var datas = ImpressionDatasFromAdMobDatas( valueEvent.AdValue, responseInfos, rewarded );
                AdImplementation.OnImpressionDatas?.Invoke( datas );
            }
        }
        
        public static AdRequest CreateAdRequest()
        {
            // Set app-level configurations
            if( !string.IsNullOrEmpty( AdImplementation.UserId ) )
                AdColonyAppOptions.SetUserId(AdImplementation.UserId);
            AdColonyAppOptions.SetTestMode(AdImplementation.IsDebug);
            
            // Set ad request parameters
            //var extras = new AdColonyMediationExtras();
            //extras.SetShowPrePopup(true);
            //extras.SetShowPostPopup(true);
            
            return new AdRequest.Builder().Build();
        }
        
        private static ImpressionDatas ImpressionDatasFromAdMobDatas( AdValue adValue, ResponseInfo responseInfo, bool rewarded )
        {
            var loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
            var extras                    = responseInfo.GetResponseExtras();
            var mediationGroupName        = extras["mediation_group_name"];
                
            var currency      = string.IsNullOrEmpty(adValue.CurrencyCode) ? "USD" : adValue.CurrencyCode;
            var revenue       = adValue.Value / 1000000f;
            var source        = loadedAdapterResponseInfo.AdSourceName;
            var placementName = loadedAdapterResponseInfo.AdSourceInstanceName;
            
            return new ImpressionDatas
            {
                ImpressionRevenue = revenue,
                Precision         = adValue.Precision.ToString(),
                Rewarded          = rewarded,
                CurrencyCode      = currency,
                AdSourceName      = source,
                AdPlacementName   = placementName,
                AdGroupName       = mediationGroupName
            };
        }

        public static void Log( string str )
        {
            AdImplementation.Log( $"[AdMobImplementation] {str}" );
        }
    }
}