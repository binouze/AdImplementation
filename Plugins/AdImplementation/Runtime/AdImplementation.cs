using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AMR;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.binouze
{
    public static class AdImplementation
    {
        private static readonly IAdImplementation implementation;
        static AdImplementation()
        {
            implementation = new AdMostImplementation();
        }
        
        internal static void Log( string str )
        {
            if( LogEnabled )
                Debug.Log( $"[AdImplementation] {str}" );
        }
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████       
//          
//                                   ██████  ███████ ██████  ██    ██  ██████  
//                                   ██   ██ ██      ██   ██ ██    ██ ██       
//                                   ██   ██ █████   ██████  ██    ██ ██   ███ 
//                                   ██   ██ ██      ██   ██ ██    ██ ██    ██ 
//                                   ██████  ███████ ██████   ██████   ██████  
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  
        
        private static bool LogEnabled;
        /// <summary>
        /// Activer ou desactiver les logs du plugin
        /// </summary>
        /// <param name="enabled"></param>
        [UsedImplicitly]
        public static void SetLogEnabled( bool enabled )
        {
            LogEnabled = enabled;
        }
        
        public static bool IsDebug { get; private set; }
        /// <summary>
        /// definir si on est en mode DEBUG ou pas.
        /// en mode DEBUG:
        ///  - on prend en compte les fonctionnalites GDPR reset / force
        ///  - on prend en compte les fonctionalites TestSuite
        /// </summary>
        /// <param name="isDebug"></param>
        [UsedImplicitly]
        public static void SetIsDebug( bool isDebug )
        {
            IsDebug = isDebug;
        }
        
        private static bool IsGDRPForced;
        /// <summary>
        /// TEST ONLY: s'assurer d'etre en mode GDPR EU
        /// </summary>
        [UsedImplicitly]
        public static void SetForceGDRP( bool force )
        {
            IsGDRPForced = force;
        }

        private static bool IsGDRPReset;
        /// <summary>
        /// TEST ONLY: reinitialiser le user consent du GDPR pour etre sur de devoir afficher le popup GDPR
        /// </summary>
        [UsedImplicitly]
        public static void SetResetGDRP( bool reset )
        {
            IsGDRPReset = reset;
        }


        public static bool AutoLoadAds { get; private set; } = true;
        /// <summary>
        /// Activer ou desactiver les logs du plugin
        /// </summary>
        /// <param name="autoload"></param>
        [UsedImplicitly]
        public static void SetAutoLoadAds( bool autoload )
        {
            AutoLoadAds = autoload;
        }
        
        
        /// <summary>
        /// TEST ONLY: ouvrir ARM Test Suite
        /// ne fonctionne que si l'IDFA du device a ete ajoute au dashboard AdMost
        /// </summary>
        [UsedImplicitly]
        public static void OpenTestSuite()
        {
            if( IsDebug && implementation is AdMostImplementation admost )
                admost.OpenTestSuite();
        }


//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  
//
//                     ██████  █████  ██      ██      ██████   █████   ██████ ██   ██ ███████ 
//                    ██      ██   ██ ██      ██      ██   ██ ██   ██ ██      ██  ██  ██      
//                    ██      ███████ ██      ██      ██████  ███████ ██      █████   ███████ 
//                    ██      ██   ██ ██      ██      ██   ██ ██   ██ ██      ██  ██       ██ 
//                     ██████ ██   ██ ███████ ███████ ██████  ██   ██  ██████ ██   ██ ███████         
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  

        public static Action<ImpressionDatas> OnImpressionDatas { get; private set; }
        /// <summary>
        /// recevoir les infos d'impression des videos
        /// </summary>
        /// <param name="onImpressionDatas"></param>
        [UsedImplicitly]
        public static void SetImpressionDataHandler( Action<ImpressionDatas> onImpressionDatas )
        {
            OnImpressionDatas = onImpressionDatas;
        }

        private static Action OnAdOpen;
        /// <summary>
        /// recevoir un event lorsq'une video est affichee
        /// </summary>
        /// <param name="onAdOpen"></param>
        [UsedImplicitly]
        public static void SetOnAdOpen( Action onAdOpen )
        {
            OnAdOpen = onAdOpen;
        }

        private static Action OnAdClose;
        /// <summary>
        /// recevoir un event lorsq'une video est fermee
        /// </summary>
        /// <param name="onAdClose"></param>
        [UsedImplicitly]
        public static void SetOnAdClose( Action onAdClose )
        {
            OnAdClose = onAdClose;
        }
        
        public static Action<string,bool> OnAdClicked;
        /// <summary>
        /// recevoir un event lorsque le joueur clique sur une video
        /// </summary>
        /// <param name="onAdClicked"></param>
        [UsedImplicitly]
        public static void SetOnAdClicked( Action<string, bool> onAdClicked )
        {
            OnAdClicked = onAdClicked;
        }
        
        public static Action<AdViewInfo> OnAdViewInfo;
        /// <summary>
        /// recevoir les infos de visionage de la video en fin de lecture
        /// </summary>
        /// <param name="onAdAdViewInfo"></param>
        [UsedImplicitly]
        public static void SetOnAdViewInfos( Action<AdViewInfo> onAdAdViewInfo )
        {
            OnAdViewInfo = onAdAdViewInfo;
        }

        private static Action<Action<bool>> ShowGDPRPopup;
        /// <summary>
        /// definir la fonction a appeler pour afficher le popup de consentement GDPR
        /// </summary>
        /// <param name="showGDPRPopup"></param>
        [UsedImplicitly]
        public static void SetGDPRFormFunction( Action<Action<bool>> showGDPRPopup )
        {
            ShowGDPRPopup = showGDPRPopup;
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
        
        public static string UserId { get; private set; } = string.Empty;
        /// <summary>
        /// definir l'identifiant du joueur
        /// </summary>
        /// <param name="userId"></param>
        [UsedImplicitly]
        public static void SetUserID( string userId )
        {
            UserId = userId;
            implementation.SetUserID( userId );
        }

        /// <summary>
        /// definir les id des zones interstitial/rewarded uniques
        /// </summary>
        /// <param name="rewardedId"></param>
        /// <param name="interstitialId"></param>
        [UsedImplicitly]
        public static void SetIds( string rewardedId, string interstitialId )
        {
            SetIds( new List<string>{rewardedId}, new List<string>{interstitialId} );
        }
        
        /// <summary>
        /// definir les id des zones interstitial/rewarded
        /// </summary>
        /// <param name="rewardedIds"></param>
        /// <param name="interstitialIds"></param>
        [UsedImplicitly]
        public static void SetIds( List<string> rewardedIds, List<string> interstitialIds )
        {
            #if UNITY_ANDROID 
            var appid = AdImplementationSettings.LoadInstance().AndroidAppId;
            #elif UNITY_IOS
            var appid = AdImplementationSettings.LoadInstance().IOSAppId;
            #else
            var appid = string.Empty;
            #endif
            
            implementation.SetIds( appid, rewardedIds, interstitialIds );
        }

        /// <summary>
        /// true si la plateforme supporte les ads et que l'initialisation est terminee
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public static bool IsAdSupported() => implementation.IsAdSupported();

        private static bool                    IsInIt;
        private static bool                    IsInIt2;
        private static CancellationTokenSource Init2Cancellation;
        
        /// <summary>
        /// Lancer l'initialisation du module publicitaire
        /// </summary>
        [UsedImplicitly]
        public static void Initialize()
        {
            Log( $"Initialize {IsInIt}" );

            if( IsInIt )
                return;
            IsInIt = true;
            
            AdsAsyncUtils.SetInstance();
            // recuperer le type de consentement necessaire avant initialisation, on en aura besoin a l'init
            AMRSDK.setPrivacyConsentRequired( privacyConsentRequired );

            // on met un delai maximum de 10 secondes sur la recuperation du status GDPR, sinon on passe en mode GDPR
            Init2Cancellation = new CancellationTokenSource();
            AdsAsyncUtils.DelayCall( () => privacyConsentRequired("GDPR"), 10_000 );
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
        /// true si une video Intersticielle est prete a etre affichee pour la zone par defaut
        /// </summary>
        [UsedImplicitly]
        public static bool HasInterstitialAvailable => implementation.HasInterstitialAvailable();

        /// <summary>
        /// true si une video Intersticielle est prete a etre affichee pour une zone definie
        /// </summary>
        [UsedImplicitly]
        public static bool HasInterstitialAvailableForZone(string zoneID = null) => implementation.HasInterstitialAvailable(zoneID);


        /// <summary>
        /// lancer l'affichage d'une video Intersticielle pour le placement par defaut
        /// le callback retournera false si aucune video n'est disponible ou qu'il y a eu un probleme d'affichge
        /// </summary>
        /// <param name="OnComplete"></param>
        [UsedImplicitly]
        public static void ShowInterstitial( Action<bool> OnComplete )
        {
            Log( "ShowInterstitial" );
            ShowInterstitial( null, OnComplete );
        }
        
        /// <summary>
        /// lancer l'affichage d'une video Intersticielle pour un placement defini
        /// le callback retournera false si aucune video n'est disponible ou qu'il y a eu un probleme d'affichge
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        [UsedImplicitly]
        public static void ShowInterstitial( string zoneID, Action<bool> OnComplete )
        {
            Log( $"ShowInterstitial {zoneID}" );
            
            if( !HasInterstitialAvailableForZone(zoneID) )
            {
                Log( "ShowInterstitial NOT AVAILABLE" );
                OnComplete?.Invoke( false );
                return;
            }
            
            ShowGdprIfRequired( () =>
            {
                OnAdOpen?.Invoke();
                implementation.ShowInterstitial( zoneID, ok =>
                {
                    AdsAsyncUtils.CallOnMainThread( () =>
                    {
                        OnAdClose?.Invoke();
                        OnComplete?.Invoke( ok );
                    });
                } );
            } );
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
        /// true si une video Rewarded est prete a etre affichee pour la zone par defaut
        /// </summary>
        [UsedImplicitly]
        public static bool HasRewardedAvailable => implementation.HasRewardedAvailable();
        
        /// <summary>
        /// true si une video Rewarded est prete a etre affichee pour une zone definie
        /// </summary>
        [UsedImplicitly]
        public static bool HasRewardedAvailableForZone(string zoneID = null) => implementation.HasRewardedAvailable(zoneID);
        
        /// <summary>
        /// lancer l'affichage d'une video Rewarded
        /// le callback retournera true si la video a ete vue jusqu'au bout et qu'un reward peut etre accorde
        /// </summary>
        /// <param name="OnComplete"></param>
        [UsedImplicitly]
        public static void ShowRewarded( Action<bool> OnComplete )
        {
            Log( "ShowRewarded" );
            ShowRewarded( null, OnComplete );
        }

        /// <summary>
        /// lancer l'affichage d'une video Rewarded
        /// le callback retournera true si la video a ete vue jusqu'au bout et qu'un reward peut etre accorde
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        [UsedImplicitly]
        public static void ShowRewarded( string zoneID, Action<bool> OnComplete )
        {
            Log( $"ShowRewarded {zoneID}" );
        
            if( !HasRewardedAvailableForZone(zoneID) )
            {
                Log( "ShowRewarded NOT AVAILABLE" );
                OnComplete?.Invoke( false );
                return;
            }
            
            ShowGdprIfRequired( () =>
            {
                OnAdOpen?.Invoke();
                implementation.ShowRewarded( zoneID, ok =>
                {
                    AdsAsyncUtils.CallOnMainThread( () =>
                    {
                        OnAdClose?.Invoke();
                        OnComplete?.Invoke( ok );
                    } );
                } );
            } );
        }
        
        

//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
//                                  
//                                       ██████  ██████  ██████  ██████  
//                                      ██       ██   ██ ██   ██ ██   ██ 
//                                      ██   ███ ██   ██ ██████  ██████  
//                                      ██    ██ ██   ██ ██      ██   ██ 
//                                       ██████  ██████  ██      ██   ██ 
//   
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████          
        

        /// <summary>
        /// le type de consentement requis
        /// Possible consentType values: "CCPA", "GDPR", "None"
        /// </summary>
        public static string ConsentType     { get; private set; }

        /// <summary>
        /// la reponse au consentement GDRP
        /// ossible ConsentResponse values: "OK", "NON", "UNKNOWN"
        /// </summary>
        public static string ConsentResponse { get; private set; } = "UNKNOWN";

        /// <summary>
        /// this function will be called just once after calling AMRSDK.setPrivacyConsentRequired
        /// but could be called before if the function AMRSDK.setPrivacyConsentRequired takes too long to respond
        /// </summary>
        /// <param name="consentType"></param>
        private static void privacyConsentRequired(string consentType)
        {
            Log( $"ADMOST - privacyConsentRequired : {consentType}" );

            Init2Cancellation?.Cancel();
            Init2Cancellation?.Dispose();
            Init2Cancellation = null;
            
            // en mode debug on test si on doit forcer le GDPR consent et le reset des infos GDPR
            if( IsDebug )
            {
                if( IsGDRPReset )
                    SetGDPRStatus( "UNKNOWN" );
                
                if( IsGDRPForced )
                    consentType = "GDPR";
            }

            ConsentResponse = GetGDPRStatus(); // Possible ConsentResponse values: "OK" ,   "NON" ,  "UNKNOWN"
            ConsentType     = consentType;     // Possible consentType values:     "CCPA" , "GDPR" , "None"
            
            if( IsInIt2 )
                return;
            IsInIt2 = true;
            
            implementation.Initialize();
        }

        private static bool MustAskGDPR => ConsentType != "None" && ConsentResponse != "OK" && ConsentResponse != "NON";

        /// <summary>
        /// true si le joueur est dans un pays demandant un consentement GDPR ou CCPA
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public static bool IsGDPRFormRequired() => ConsentType != "None";

        /// <summary>
        /// fonction interne appelee avant le lancement d'une video si le consentement GDPR est requis
        /// </summary>
        /// <param name="complete"></param>
        private static void ShowGdprIfRequired( Action complete )
        {
            Log( $"ShowGdprIfRequired {MustAskGDPR}" );
            
            if( MustAskGDPR )
                ShowGdprForm( complete );
            else
                complete?.Invoke();
        }
        
        /// <summary>
        /// Lancer l'affichage du popup de consentement GDPR
        /// </summary>
        /// <param name="complete"></param>
        [UsedImplicitly]
        public static void ShowGdprForm( Action complete = null )
        {
            Log( $"ShowGdprForm" );
            if( ShowGDPRPopup != null )
            {
                Log( "Show GDPR Form" );
                
                ShowGDPRPopup?.Invoke( reponse =>
                {
                    Log( $"GDPR Form response {reponse}" );
                    
                    SetGDPRStatus( reponse ? "OK" : "NON" );
                    complete?.Invoke();
                } );
            }
            else
            {
                Debug.LogError( "NO FORM TO SHOW, PLEASE CONFIGURE THE PLUGIN" );
                complete?.Invoke();
            }
        }


        /// <summary>
        /// fonction interne pour recuperer lle status de consentement GDPR du joueur
        /// </summary>
        /// <returns></returns>
        private static string GetGDPRStatus()
        {
            try
            {
                // on essaye de lire sur le disque en premier
                var path   = Application.persistentDataPath;
                var result = File.ReadAllText( Path.Combine( path, "admostgdpr" ) );
                Log( $"GetGDPRStatus From File {result}" );
                return result;
            }
            catch( Exception e )
            {
                // si on y arrive pas, on lit en playerprefs
                var respref = PlayerPrefs.GetString( "admostgdpr", "UNKNOWN" );
                Log( $"GetGDPRStatus From PlayerPref {respref} {e.Message}" );
                return respref;
            }
        }

        /// <summary>
        /// Fonction interne pour enregistrer le consentement GDPR du joueur
        /// </summary>
        /// <param name="status"></param>
        private static void SetGDPRStatus( string status )
        {
            Log( $"SetGDPRStatus {status}" );

            if( status != "OK" && status != "NON" )
                status = "UNKNOWN";

            // on met a jour le status actuel
            ConsentResponse = status;
            // on sauve en player prefs
            PlayerPrefs.SetString( "admostgdpr", status );
            
            try
            {
                // on essaye d'ecrire sur le disque
                var path = Application.persistentDataPath;
                File.WriteAllText( Path.Combine( path, "admostgdpr" ), status );
            }
            catch( Exception e )
            {
                Log( $"Error writing status to file {status} {e.Message}" );
            }
        }
    }
    
    public class ImpressionDatas
    {
        [UsedImplicitly] public double ImpressionRevenue;
        [UsedImplicitly] public string Precision;
        [UsedImplicitly] public bool   Rewarded;
        [UsedImplicitly] public string CurrencyCode;
        [UsedImplicitly] public string AdSourceName;
        [UsedImplicitly] public string AdPlacementName;
        [UsedImplicitly] public string AdGroupName;

        public override string ToString()
        {
            return $"[ImpressionDatas] {nameof( ImpressionRevenue )}: {ImpressionRevenue}, {nameof( Precision )}: {Precision}, {nameof( Rewarded )}: {Rewarded}, {nameof( CurrencyCode )}: {CurrencyCode}, {nameof( AdSourceName )}: {AdSourceName}, {nameof( AdPlacementName )}: {AdPlacementName}, {nameof( AdGroupName )}: {AdGroupName}";
        }
    }
}