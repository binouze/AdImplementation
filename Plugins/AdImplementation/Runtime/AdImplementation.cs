using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            Log( $"OpenTestSuite {IsDebug}" );

            if( IsDebug && implementation is AdMostImplementation admost )
            {
                Log( $"OpenTestSuite OK" );
                admost.OpenTestSuite();
            }
        }
        

//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  
//
//              ██████   █████  ██████   █████  ███    ███ ███████ ████████ ███████ ██████  ███████ 
//              ██   ██ ██   ██ ██   ██ ██   ██ ████  ████ ██         ██    ██      ██   ██ ██      
//              ██████  ███████ ██████  ███████ ██ ████ ██ █████      ██    █████   ██████  ███████ 
//              ██      ██   ██ ██   ██ ██   ██ ██  ██  ██ ██         ██    ██      ██   ██      ██ 
//              ██      ██   ██ ██   ██ ██   ██ ██      ██ ███████    ██    ███████ ██   ██ ███████
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  


        /// <summary>
        /// si true, la gestion du consentement ne sera pas geree par le plugin mais en externe (UMP)
        /// </summary>
        public static bool UserConsentManagedExternaly;


        private static float MaxTimeLoadingBeforeShowAds;
        /// <summary>
        /// si lorsqu'on essaye de lancer une video, elle est en cours de chargement,
        /// on va laisser charger la video pendant un temps max avant de dire qu'iol n'u a pas de video dispo
        /// dans ces cas la la fonction OnAdWaitToStart sera appelee
        /// </summary>
        /// <param name="val"></param>
        [UsedImplicitly]
        public static void SetMaxTimeLoadingBeforeShowAd(float val)
        {
            MaxTimeLoadingBeforeShowAds = val;
        }


        private static float MaxTimeBeforeAdShown;
        /// <summary>
        /// temps maximum (en secondes) accorde a la regie pour AFFICHER la pub apres la demande d'affichage.
        /// Passe ce delai sans que la regie ait signale l'affichage (OnAdShow) ni rappele (fermeture / echec),
        /// on considere que la pub ne s'affichera jamais: OnAdClose puis OnComplete(false) sont appeles pour
        /// rendre la main au jeu, et le flag interne AdPlaying de l'implementation est libere — sinon
        /// HasRewardedAvailable et HasInterstitialAvailable renvoient false pour TOUTE LA SESSION (plus aucune
        /// pub, rewarded comme interstitielle, jusqu'au relaunch de l'app).
        /// 0 (defaut) = desactive, comportement inchange.
        ///
        /// ⚠ La demande d'affichage n'est PAS annulee (impossible cote SDK): si la pub finit malgre tout par
        /// s'afficher, tous ses callbacks suivent normalement — OnAdShown, le reward, puis OnComplete une
        /// SECONDE fois. Le jeu doit donc encaisser un OnComplete tardif apres le OnComplete(false) du timeout:
        /// c'est voulu, ca permet de livrer quand meme le gain d'une rewarded qui a fini par se lancer.
        ///
        /// Ne couvre PAS le cas "pub affichee mais jamais fermee": la surveillance s'arrete des que la regie
        /// signale l'affichage (impossible de distinguer ici une pub de 3 minutes d'un blocage). Au jeu de
        /// gerer ce cas s'il le souhaite, avec OnAdShown et ses propres signaux (pause/focus de l'app).
        /// </summary>
        /// <param name="val"></param>
        [UsedImplicitly]
        public static void SetMaxTimeBeforeAdShown(float val)
        {
            MaxTimeBeforeAdShown = val;
        }

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
        
        
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  
//
//                     ██████  █████  ██      ██      ██████   █████   ██████ ██   ██ ███████ 
//                    ██      ██   ██ ██      ██      ██   ██ ██   ██ ██      ██  ██  ██      
//                    ██      ███████ ██      ██      ██████  ███████ ██      █████   ███████ 
//                    ██      ██   ██ ██      ██      ██   ██ ██   ██ ██      ██  ██       ██ 
//                     ██████ ██   ██ ███████ ███████ ██████  ██   ██  ██████ ██   ██ ███████         
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████  


        private static Action OnAdWaitToStart;
        /// <summary>
        /// une fonction a appeler lorsqu'on commence l'attente de fin de chargement avant de montrer ou pas une video
        /// </summary>
        /// <param name="onAdWaitToStart"></param>
        [UsedImplicitly]
        public static void SetOnAdWaitToStart( Action onAdWaitToStart )
        {
            OnAdWaitToStart = onAdWaitToStart;
        }


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
        /// recevoir un event lorsq'on demande l'affichage d'une video au SDK.
        /// ⚠ ce n'est PAS la preuve que la video est apparue a l'ecran: OnAdOpen est appele juste AVANT de
        /// passer la main au SDK. Pour savoir que la pub est reellement affichee, utiliser SetOnAdShown.
        /// </summary>
        /// <param name="onAdOpen"></param>
        [UsedImplicitly]
        public static void SetOnAdOpen( Action onAdOpen )
        {
            OnAdOpen = onAdOpen;
        }

        private static Action OnAdShown;
        /// <summary>
        /// recevoir un event lorsque la regie signale que la video est REELLEMENT affichee a l'ecran
        /// (a ne pas confondre avec OnAdOpen, cf. ci-dessus). Appele sur le main thread.
        /// </summary>
        /// <param name="onAdShown"></param>
        [UsedImplicitly]
        public static void SetOnAdShown( Action onAdShown )
        {
            OnAdShown = onAdShown;
        }

        /// <summary>
        /// appele par les implementations quand la regie signale que la pub est a l'ecran.
        /// peut arriver depuis un thread natif: le flag est pose tout de suite (il sert au filet de securite
        /// SetMaxTimeBeforeAdShown), et seul le callback du jeu passe par le main thread.
        /// </summary>
        internal static void NotifyAdShown()
        {
            AdShownReceived = true;
            AdsAsyncUtils.CallOnMainThread( () =>
            {
                Log( "AD SHOWN" );
                OnAdShown?.Invoke();
            } );
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
        
        
        private static Func<bool> MustShowGDPRPopup;
        /// <summary>
        /// definir la fonction a appeler pour savoir si on doit forcer l'affichage du popup de consentement GDPR
        /// </summary>
        /// <param name="mustShowGDPRPopup"></param>
        [UsedImplicitly]
        public static void SetMustShowGDPRFormFunction( Func<bool> mustShowGDPRPopup )
        {
            MustShowGDPRPopup = mustShowGDPRPopup;
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

            if( UserConsentManagedExternaly )
            {
                privacyConsentRequired( ConsentType );
            }
            else
            {
                // recuperer le type de consentement necessaire avant initialisation, on en aura besoin a l'init
                AMRSDK.setPrivacyConsentRequired( privacyConsentRequired );
                // on met un delai maximum de 10 secondes sur la recuperation du status GDPR, sinon on passe en mode None
                Init2Cancellation = new CancellationTokenSource();
                AdsAsyncUtils.DelayCall( () => privacyConsentRequired("GDPR"), 10_000 );
            }
        }
        

//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
//
//        ███████ ██ ██      ███████ ████████     ███████ ██   ██  ██████  ██     ██
//        ██      ██ ██      ██         ██        ██      ██   ██ ██    ██ ██     ██
//        █████   ██ ██      █████      ██        ███████ ███████ ██    ██ ██  █  ██
//        ██      ██ ██      ██         ██             ██ ██   ██ ██    ██ ██ ███ ██
//        ██      ██ ███████ ███████    ██        ███████ ██   ██  ██████   ███ ███
//
//  ████████████████████████████████████████████████████████████████████████████████████████████████████████████████████


        /// <summary>identifie la demande d'affichage en cours (invalide la surveillance des precedentes)</summary>
        private static int  ShowId;
        /// <summary>la regie a signale l'affichage de la pub en cours</summary>
        private static bool AdShownReceived;
        /// <summary>la regie a rappele (fermeture ou echec) pour la pub en cours</summary>
        private static bool AdCompleteReceived;

        /// <summary>a appeler juste avant de demander l'affichage au SDK</summary>
        private static int NouvelleDemandeAffichage()
        {
            AdShownReceived    = false;
            AdCompleteReceived = false;
            AdsAsyncUtils.AppPauseeDepuisDerniereDemande = false;
            return ++ShowId;
        }

        /// <summary>
        /// Filet de securite: si la regie n'affiche jamais la pub ET ne rappelle jamais (ca arrive), le jeu
        /// reste bloque sur son ecran d'attente pour toujours. Passe MaxTimeBeforeAdShown secondes, on rend la
        /// main au jeu. Voir SetMaxTimeBeforeAdShown pour les details et les limites.
        /// </summary>
        private static async Task SurveillerAffichage( int showId, Action<bool> OnComplete )
        {
            var timeout = MaxTimeBeforeAdShown;
            if( timeout <= 0 )
                return;

            var tend = Time.realtimeSinceStartup + timeout;
            while( Time.realtimeSinceStartup < tend )
            {
                await Task.Yield();

                if( !DoitContinuerASurveiller( showId ) )
                    return;
            }

            if( !DoitContinuerASurveiller( showId ) )
                return;

            Log( $"SHOW TIMEOUT: aucun affichage ni retour de la regie apres {timeout}s, on rend la main au jeu" );

            // liberer le flag AdPlaying de l'implementation, sinon plus aucune pub n'est possible de la session.
            // on ne touche PAS aux callbacks en attente: si la pub finit par s'afficher ils doivent fonctionner.
            implementation?.ForceResetAdPlaying();

            OnAdClose?.Invoke();
            OnComplete?.Invoke( false );
        }

        private static bool DoitContinuerASurveiller( int showId )
        {
            // une autre demande d'affichage a pris la main
            if( showId != ShowId )
                return false;

            // la regie a repondu: soit la pub est a l'ecran, soit elle est deja fermee / en echec
            if( AdShownReceived || AdCompleteReceived )
                return false;

            // l'app est passee en arriere plan depuis la demande: quelque chose s'est bien affiche par dessus
            // le jeu (sur Android une pub s'affiche dans une autre Activity) meme si la regie ne l'a pas dit
            if( AdsAsyncUtils.AppPauseeDepuisDerniereDemande )
                return false;

            return true;
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
        /// true si une video Intersticielle est en train de charger pour une zone definie
        /// </summary>
        [UsedImplicitly]
        public static bool HasInterstitialLoadingForZone(string zoneID = null) => implementation.HasInterstitialLoading(zoneID);


        /// <summary>
        /// lancer le prechargement d'une video intersticielle
        /// </summary>
        /// <param name="zoneID"></param>
        [UsedImplicitly]
        public static void LoadInterstitial( string zoneID )
        {
            implementation.LoadInterstitial( zoneID );
        }

        /// <summary>
        /// lancer l'affichage d'une video Intersticielle pour le placement par defaut
        /// le callback retournera false si aucune video n'est disponible ou qu'il y a eu un probleme d'affichge
        /// </summary>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        [UsedImplicitly]
        public static void ShowInterstitial( Action<bool> OnComplete, string tag = null )
        {
            Log( "ShowInterstitial" );
            _ = ShowInterstitial( null, OnComplete, tag );
        }

        /// <summary>
        /// lancer l'affichage d'une video Intersticielle pour un placement defini
        /// le callback retournera false si aucune video n'est disponible ou qu'il y a eu un probleme d'affichge
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        [UsedImplicitly]
        public static async Task ShowInterstitial( string zoneID, Action<bool> OnComplete, string tag = null )
        {
            Log( $"ShowInterstitial {zoneID}" );
            try 
            {
                if( !HasInterstitialAvailableForZone(zoneID) )
                {
                    var available = false;
                    var loading   = false;
                    
                    if( MaxTimeLoadingBeforeShowAds > 0 && HasInterstitialLoadingForZone( zoneID ) )
                    {
                        loading = true;
                        OnAdWaitToStart?.Invoke();
                        
                        var tend = Time.realtimeSinceStartup + MaxTimeLoadingBeforeShowAds;
                        while( Time.realtimeSinceStartup < tend && !available )
                        {
                            available = HasInterstitialAvailableForZone( zoneID );
                            await Task.Yield();
                        }
                    }

                    if( !available )
                    {
                        if( loading )
                            OnAdClose?.Invoke();
                        
                        Log( "ShowInterstitial NOT AVAILABLE" );
                        OnComplete?.Invoke( false );
                        return;
                    }
                }
                
                ShowGdprIfRequired( () =>
                {
                    var showId = NouvelleDemandeAffichage();
                    OnAdOpen?.Invoke();
                    implementation.ShowInterstitial( zoneID, ok =>
                    {
                        if( showId == ShowId )
                            AdCompleteReceived = true;

                        AdsAsyncUtils.CallOnMainThread( () =>
                        {
                            OnAdClose?.Invoke();
                            OnComplete?.Invoke( ok );
                        });
                    }, tag );

                    // filet: si la regie n'affiche rien et ne rappelle jamais, rendre la main au jeu
                    _ = SurveillerAffichage( showId, OnComplete );
                } );
            }
            catch( Exception e ) 
            {
                Debug.LogException( e );
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
        /// true si une video Rewarded est en cours de chargement pour une zone definie
        /// </summary>
        [UsedImplicitly]
        public static bool HasRewardedLoadingForZone(string zoneID = null) => implementation.HasRewardedLoading(zoneID);
        
        
        /// <summary>
        /// lancer le prechargement d'une video rewarded
        /// </summary>
        /// <param name="zoneID"></param>
        [UsedImplicitly]
        public static void LoadRewarded( string zoneID )
        {
            implementation.LoadRewarded( zoneID );
        }

        /// <summary>
        /// lancer l'affichage d'une video Rewarded
        /// le callback retournera true si la video a ete vue jusqu'au bout et qu'un reward peut etre accorde
        /// </summary>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        /// <param name="ssvExtra"></param>
        /// <param name="OnReward"></param>
        [UsedImplicitly]
        public static void ShowRewarded( Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null )
        {
            Log( "ShowRewarded" );
            _ = ShowRewarded( null, OnComplete, tag, ssvExtra, OnReward );
        }

        /// <summary>
        /// lancer l'affichage d'une video Rewarded
        /// le callback retournera true si la video a ete vue jusqu'au bout et qu'un reward peut etre accorde
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        /// <param name="ssvExtra"></param>
        /// <param name="OnReward"></param>
        [UsedImplicitly]
        public static async Task ShowRewarded( string zoneID, Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null )
        {
            Log( $"ShowRewarded {zoneID}" );
            try 
            {
                if( !HasRewardedAvailableForZone(zoneID) )
                {
                    var available = false;
                    var loading   = false;
                    
                    if( MaxTimeLoadingBeforeShowAds > 0 && HasRewardedLoadingForZone( zoneID ) )
                    {
                        loading = true;
                        OnAdWaitToStart?.Invoke();
                        
                        var tend = Time.realtimeSinceStartup + MaxTimeLoadingBeforeShowAds;
                        while( Time.realtimeSinceStartup < tend && !available )
                        {
                            available = HasRewardedAvailableForZone( zoneID );
                            await Task.Yield();
                        }
                    }

                    if( !available )
                    {
                        if( loading )
                            OnAdClose?.Invoke();
                        
                        Log( "ShowRewarded NOT AVAILABLE" );
                        OnComplete?.Invoke( false );
                        return;
                    }
                }
                
                ShowGdprIfRequired( () =>
                {
                    var showId = NouvelleDemandeAffichage();
                    OnAdOpen?.Invoke();
                    implementation.ShowRewarded( zoneID, ok =>
                    {
                        if( showId == ShowId )
                            AdCompleteReceived = true;

                        Log( "ShowRewarded COMPLETE waitForMainThread" );
                        AdsAsyncUtils.CallOnMainThread( () =>
                        {
                            Log( "ShowRewarded COMPLETE mainThreadOK" );
                            OnAdClose?.Invoke();
                            OnComplete?.Invoke( ok );
                        } );
                    },
                        tag,
                        ssvExtra,
                        () =>
                        {
                            Log( $"ShowRewarded ONREWARD {OnReward} waitForMainThread" );
                            if( OnReward != null )
                            {
                                AdsAsyncUtils.CallOnMainThread( () =>
                                {
                                    Log( "ShowRewarded ONREWARD mainThreadOK" );
                                    OnReward.Invoke();
                                } );
                            }
                        } );

                    // filet: si la regie n'affiche rien et ne rappelle jamais, rendre la main au jeu
                    _ = SurveillerAffichage( showId, OnComplete );
                } );
            }
            catch( Exception e ) 
            {
                Debug.LogException( e );
                OnComplete?.Invoke( false );
            }
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
        /// valid values are GDPR, CCPA, None
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void SetPrivacyConsentType( string type )
        {
            ConsentType = type;
        }
        
        /// <summary>
        /// valid values are "OK", "NON", "UNKNOWN"
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static void SetPrivacyConsentResponse( string response )
        {
            SetGDPRStatus( response );
            //ConsentResponse = response;
        }


        /// <summary>
        /// set canRequestAds value for AdMob
        /// </summary>
        /// <param name="canRequestAds"></param>
        /// <returns></returns>
        public static void SetCanRequestAds( bool canRequestAds )
        {
            implementation.SetCanRequestAds( canRequestAds );
        }

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
            if( IsDebug /*&& !UserConsentManagedExternaly*/ )
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
            var mustShow = MustShowGDPRPopup?.Invoke() ?? MustAskGDPR;
            
            Log( $"ShowGdprIfRequired {MustAskGDPR} / {mustShow}" );
            
            if( mustShow )
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