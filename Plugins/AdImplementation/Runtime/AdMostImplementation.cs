using System;
using System.Collections.Generic;
using System.IO;
using AMR;
using UnityEngine;

namespace com.binouze
{
    public class AdMostImplementation : IAdImplementation, IAdMostAdDelegate
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


        private string       AppID;
        private List<string> AdRewarUnit;
        private List<string> AdInterUnit;
        private bool         AdSupported;
        
        private static bool IsInit;
        private bool IsInitComplete;

        private static bool         AdPlaying;
        private static Action<bool> OnAdPlayComplete;

        private static readonly AdMostAd RewardedAdsControlller    = new (true);
        private static readonly AdMostAd InterstitalAdsControlller = new (false);
        
        
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
        /// true si la configuration actuelle supporte les pubs et que le SDK a bien ete initialise 
        /// </summary>
        public bool IsAdSupported() => AdSupported && IsInitComplete;
        
        /// <summary>
        /// definir les ids des emplacement pubs
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="rewardedId"></param>
        /// <param name="interstitialId"></param>
        public void SetIds( string appID, List<string> rewardedId, List<string> interstitialId )
        {
            AppID       = appID;
            AdRewarUnit = rewardedId;
            AdInterUnit = interstitialId;
            AdSupported = !string.IsNullOrEmpty( AppID ) && (rewardedId?.Count > 0 || interstitialId?.Count > 0);
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
            var ids = new List<string>();
            foreach( var id in AdInterUnit ) { ids.Add( id ); }
            foreach( var id in AdRewarUnit ) { ids.Add( id ); }
            
            AMRSDK.startTestSuite( ids.ToArray() );
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
            config.ApplicationIdIOS = AppID;
            #elif UNITY_ANDROID
            config.ApplicationIdAndroid = AppID;
            #endif

            config.UserConsent   = AdImplementation.IsDebug || AdImplementation.ConsentResponse == "OK" ? "1" : "0";
            config.SubjectToGDPR = AdImplementation.ConsentType     == "GDPR" ? "1" : "0";
            config.SubjectToCCPA = AdImplementation.ConsentResponse == "CCPA" ? "1" : "0";
            config.IsUserChild   = "0";
            
            AMRSDK.startWithConfig( config, OnSDKDidInitialize );
            
            // si il y avait des infos de visionnage en attente d'envoi, on les envoi
            InterstitialAdInfo.SendIfNeeded();
            RewardAdInfo.SendIfNeeded();
            
            /*if( !InterstitialAdInfo.Sent && InterstitialAdInfo.Started )
            {
                Log( $"Sending waiting InterstitialAdInfo:{InterstitialAdInfo}" );
                
                AdImplementation.OnAdViewInfo?.Invoke( InterstitialAdInfo );
                InterstitialAdInfo.Sent = true;
                InterstitialAdInfo.Save();
            }
            if( !RewardAdInfo.Sent && RewardAdInfo.Started )
            {
                Log( $"Sending waiting RewardAdInfo:{RewardAdInfo}" );
                
                AdImplementation.OnAdViewInfo?.Invoke( RewardAdInfo );
                RewardAdInfo.Sent = true;
                RewardAdInfo.Save();
            }*/
            
            #if UNITY_EDITOR
            OnSDKDidInitialize( true, null );
            #endif
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
                // setting user id if it was defined before initialization complete
                if( !string.IsNullOrEmpty(AdImplementation.UserId) )
                    AMRSDK.setUserId(AdImplementation.UserId);

                // if we want to auto load the placements,
                // start loading now
                if( AdImplementation.AutoLoadAds )
                {
                    if( AdRewarUnit?.Count > 0 )
                    {
                        foreach( var zone in AdRewarUnit )
                        {
                            RewardedAdsControlller.LoadAd( zone );
                        }
                    }

                    if( AdInterUnit?.Count > 0 )
                    {
                        foreach( var zone in AdInterUnit )
                        {
                            InterstitalAdsControlller.LoadAd( zone );
                        }
                    }
                }

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

        private static void AdComplete( bool ok, bool rewarded = false )
        {
            var adinfo = rewarded ? RewardAdInfo : InterstitialAdInfo;
            Log( $"AdComplete ok: {ok}, adinfo:{adinfo}" );
            
            AdPlaying = false;
            OnAdPlayComplete?.Invoke( ok );
            OnAdPlayComplete = null;

            // send view statistics about this ad
            adinfo.SendIfNeeded();
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

        public static void Log( string str )
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
        
        /// <summary>
        /// true si on est ok pour lancer une pub interstitial
        /// </summary>
        public bool HasInterstitialAvailable( string zoneID = null )
        {
            if( zoneID == null && AdInterUnit?.Count > 0 )
                zoneID = AdInterUnit[0];
            
            Log( $"HasInterstitialAvailable: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - zoneID:{zoneID} - AdAvailable:{InterstitalAdsControlller.IsAdReady( zoneID )}" );
            return IsInitComplete && !AdPlaying && InterstitalAdsControlller.IsAdReady( zoneID );
        }
        
        /// <summary>
        /// lancer le chargement d'une video intersticielle
        /// </summary>
        /// <param name="zoneID"></param>
        public void LoadInterstitial( string zoneID = null )
        {
            if( zoneID == null )
            {
                if( AdInterUnit?.Count > 0 )
                    zoneID = AdInterUnit[0];
                else 
                    return;
            }

            InterstitalAdsControlller.LoadAd( zoneID );
        }

        /// <summary>
        /// Afficher une video interstitielle
        /// </summary>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        public void ShowInterstitial( Action<bool> OnComplete, string tag = null )
        {
            Log( "ShowInterstitial" );

            if( AdInterUnit?.Count > 0 )
                ShowInterstitial( AdInterUnit[0], OnComplete, tag );
        }

        /// <summary>
        /// Afficher une video interstitielle
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        public void ShowInterstitial( string zoneID, Action<bool> OnComplete, string tag = null )
        {
            Log( $"ShowInterstitial zoneID:{zoneID}" );
            
            if( HasInterstitialAvailable(zoneID) )
            {
                AdPlaying         = true;
                OnAdPlayComplete  = OnComplete;
                IsRewardedPlaying = false;
                var ok = InterstitalAdsControlller.PlayAd( zoneID, this, tag );
                if( ! ok )
                    OnComplete?.Invoke( false );
            }
            else
            {
                OnComplete?.Invoke( false );
            }
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

        /// <summary>
        /// true si on est ok pour lancer une pub rewarded
        /// </summary>
        public bool HasRewardedAvailable( string zoneID = null )
        {
            if( zoneID == null && AdRewarUnit?.Count > 0 )
                zoneID = AdRewarUnit[0];
            
            Log( $"HasRewardedAvailable: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - zoneID:{zoneID} - AdAvailable:{RewardedAdsControlller.IsAdReady( zoneID )}" );
            return IsInitComplete && !AdPlaying && RewardedAdsControlller.IsAdReady( zoneID );
        }

        /// <summary>
        /// lancer le chargement d'une video rewarded
        /// </summary>
        /// <param name="zoneID"></param>
        public void LoadRewarded( string zoneID = null )
        {
            if( zoneID == null )
            {
                if( AdRewarUnit?.Count > 0 )
                    zoneID = AdRewarUnit[0];
                else 
                    return;
            }

            RewardedAdsControlller.LoadAd( zoneID );
        }

        /// <summary>
        /// Afficher une video Rewarded
        /// </summary>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        public void ShowRewarded( Action<bool> OnComplete, string tag = null )
        {
            Log( "ShowRewarded" );
            
            if( AdRewarUnit?.Count > 0 )
                ShowRewarded( AdRewarUnit[0], OnComplete, tag );
        }

        /// <summary>
        /// Afficher une video rewarded
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        public void ShowRewarded( string zoneID, Action<bool> OnComplete, string tag = null )
        {
            Log( $"ShowRewarded zoneID:{zoneID}" );
            
            if( HasRewardedAvailable(zoneID) )
            {
                AdPlaying         = true;
                OnAdPlayComplete  = OnComplete;
                IsRewardedPlaying = true;
                var ok = RewardedAdsControlller.PlayAd( zoneID, this, tag );
                if( ! ok )
                    OnComplete?.Invoke( false );
            }
            else
            {
                OnComplete?.Invoke( false );
            }
        }

        
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//        
//                    ██████  ███████ ██      ███████  ██████   █████  ████████ ███████ ███████ 
//                    ██   ██ ██      ██      ██      ██       ██   ██    ██    ██      ██      
//                    ██   ██ █████   ██      █████   ██   ███ ███████    ██    █████   ███████ 
//                    ██   ██ ██      ██      ██      ██    ██ ██   ██    ██    ██           ██ 
//                    ██████  ███████ ███████ ███████  ██████  ██   ██    ██    ███████ ███████ 
// 
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       


        private static readonly AdViewInfo InterstitialAdInfo = AdViewInfo.Get( false );
        private static readonly AdViewInfo RewardAdInfo       = AdViewInfo.Get( true );
        private static          bool       IsRewardedPlaying;
        
        public void OnAdShow( string networkName, double ecpm )
        {
            Log( $"OnAdShow networkName:{networkName} ecpm:{ecpm}" );
            
            if( IsRewardedPlaying ) { RewardAdInfo.Start( networkName, ecpm ); }
            else                    { InterstitialAdInfo.Start( networkName, ecpm ); }
        }

        public void OnAdImpression( AMRAd ad )
        {
            if( ad == null )
                return;
            
            Log( $"OnAdImpression rewarded:{IsRewardedPlaying} network:{ad.Network} zone:{ad.ZoneId} space:{ad.AdSpaceId} currency:{ad.Currency} revenu:{ad.Revenue}" );

            if( IsRewardedPlaying )
            {
                RewardAdInfo.Revenus         = ad.Revenue;
                RewardAdInfo.RevenusCurrency = ad.Currency;
                RewardAdInfo.Save();
            }
            else
            {
                InterstitialAdInfo.Revenus         = ad.Revenue;
                InterstitialAdInfo.RevenusCurrency = ad.Currency;
                InterstitialAdInfo.Save();
            }

            AdImplementation.OnImpressionDatas?.Invoke( ImpressionDatasFromAdMostDatas( ad, IsRewardedPlaying ) );
        }

        public void OnAdClick()
        {
            string networkName;
            int    nbCLicks;
            
            if( IsRewardedPlaying )
            {
                RewardAdInfo.NbClicks++;
                RewardAdInfo.Save();
                
                networkName = RewardAdInfo.Network;
                nbCLicks    = RewardAdInfo.NbClicks;
            }
            else
            {
                InterstitialAdInfo.NbClicks++;
                InterstitialAdInfo.Save();
                
                networkName = InterstitialAdInfo.Network;
                nbCLicks    = InterstitialAdInfo.NbClicks;
            }
            
            Log( $"OnAdClick rewarded:{IsRewardedPlaying} click:{nbCLicks}" );
            
            AdImplementation.OnAdClicked?.Invoke(networkName, false);
        }

        public void OnAdDismissed()
        {
            Log( $"OnAdDismissed rewarded:{IsRewardedPlaying}" );

            if( IsRewardedPlaying )
            {
                if( AdImplementation.AutoLoadAds )
                    RewardedAdsControlller.LoadAd(null); // null => reload le dernier placement vu
                
                AdComplete( RewardAdInfo.Complete, true );
            }
            else
            {
                if( AdImplementation.AutoLoadAds )
                    InterstitalAdsControlller.LoadAd(null); // null => reload le dernier placement vu
                InterstitialAdInfo.Complete = true;
                
                AdComplete( true );
            }
        }

        public void OnAdComplete()
        {
            Log( $"OnAdComplete rewarded:{IsRewardedPlaying}" );

            if( IsRewardedPlaying )
                RewardAdInfo.Complete = true;
        }

        public void OnAdFailToShow()
        {
            Log( $"OnAdFailToShow rewarded:{IsRewardedPlaying}" );
            AdComplete( false, IsRewardedPlaying );
        }
    }

    [Serializable]
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
        public bool   Sent;
        // ReSharper disable once MemberCanBePrivate.Global
        public bool   Rewarded;
        public bool   Started;
        
        
        /// <summary>
        /// fonction pour enregistrer l'objet en json sur le disque dans le persistent data
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson( this );
                var path = Path.Combine( Application.persistentDataPath, $"AdViewInfo{( Rewarded ? "Rewarded" : "" )}.json" );
                File.WriteAllText( path, json );
                
                AdImplementation.Log( $"[AdViewInfo] SavedToFile OK {json}" );
            }
            catch( Exception e )
            {
                Debug.LogError( $"[AdViewInfo] SavedToFile FAILED {e}" );
            }
        }

        /// <summary>
        /// recuperer l'objet depuis le disque
        /// </summary>
        /// <param name="rewarded"></param>
        /// <returns></returns>
        public static AdViewInfo Get( bool rewarded )
        {
            // essayer de charger l'objet serialise en json depuis le disque
            var path = Path.Combine( Application.persistentDataPath, $"AdViewInfo{( rewarded ? "Rewarded" : "" )}.json" );
            if( File.Exists( path ) )
            {
                try
                {
                    var json = File.ReadAllText( path );
                    AdImplementation.Log( $"[AdViewInfo] ReadingFromFile {path} -> {json}" );
                    var info = JsonUtility.FromJson<AdViewInfo>( json );
                    if( info != null )
                        return info;
                }
                catch( Exception e )
                {
                    Debug.LogError( e );
                }
            }

            return new AdViewInfo
            {
                Network  = "N/A",
                Type     = rewarded ? "Rewarded" : "Interstitial",
                UserID   = AdImplementation.UserId,
                Rewarded = rewarded,
                Started  = false
            };
        }
        
        public void Start( string network, double ecpm )
        {
            Network         = network;
            eCPM            = ecpm / 100; // ecpm are in cents, on les met en dollars puisque on les envoi en float au serveur
            Revenus         = 0;
            RevenusCurrency = "USD";
            Complete        = false;
            NbClicks        = 0;
            UserID          = AdImplementation.UserId;
            Sent            = false;
            Started         = true;
            
            Save();
        }
        
        public void SendIfNeeded()
        {
            if( !Sent && Started )
            {
                AdMostImplementation.Log( $"Sending waiting {(Rewarded ? "Rewarded" : "Interstitial")}AdInfo:{this}" );
                AdImplementation.OnAdViewInfo?.Invoke( this );
                Sent = true;
                Save();
            }
        }
        
        public override string ToString()
        {
            return $"AdViewInfo::{nameof( Network )}: {Network}, "     +
                   $"{nameof( Complete )}: {Complete}, "               +
                   $"{nameof( NbClicks )}: {NbClicks}, "               +
                   $"{nameof( Type )}: {Type}, "                       +
                   $"{nameof( eCPM )}: {eCPM}, "                       +
                   $"{nameof( Revenus )}: {Revenus}, "                 +
                   $"{nameof( RevenusCurrency )}: {RevenusCurrency}, " +
                   $"{nameof( UserID )}: {UserID}, "                   +
                   $"{nameof( Sent )}: {Sent}, "                       +
                   $"{nameof( Started )}: {Started}";
        }
    }
}