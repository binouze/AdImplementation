using System;
using System.Collections.Generic;
using System.IO;
using AMR;
using UnityEngine;

namespace com.binouze
{
    public class AdMostImplementation : IAdImplementation, IAdMostAdDelegate
    {
        
        /// <summary>
        /// Remettre l'implementation dans son etat de sortie d'usine (voir AdImplementation.ResetStatics).
        /// ⚠ Cet objet est tenu par un statique readonly d'AdImplementation: sans Domain Reload c'est le MEME
        /// d'une session de play a l'autre, ses champs d'INSTANCE survivent donc aussi et doivent etre remis
        /// ici, pas seulement les statiques.
        /// </summary>
        internal void ResetStatics()
        {
            // etat statique
            IsInit            = false;
            AdPlaying         = false;
            IsRewardedPlaying = false;
            OnAdPlayComplete  = null;
            OnAdRewarded      = null;
            DemandeZone       = null;
            DemandeReseau     = null;
            DemandeAgeMinutes = -1;
            DemandeHeure      = 0;

            // etat d'instance (le singleton survit au Play Mode)
            AppID          = null;
            AdRewarUnit    = null;
            AdInterUnit    = null;
            AdSupported    = false;
            IsInitComplete = false;
            CanRequestAds  = null;

            RewardedAdsControlller.ResetStatics();
            InterstitalAdsControlller.ResetStatics();

            // NB: InterstitialAdInfo / RewardAdInfo ne sont volontairement PAS remis a zero. Ils sont persistes
            // sur disque pour rattraper une pub interrompue par un kill de l'app, et Start() les reinitialise
            // de toute facon a chaque nouvelle pub.
        }
        
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
        private static Action       OnAdRewarded;

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

        private bool? CanRequestAds;
        /// <summary>
        /// set the canRequestAd for AdMob
        /// </summary>
        /// <param name="canRequestAd"></param>
        /// <returns></returns>
        public void SetCanRequestAds( bool canRequestAds )
        {
            CanRequestAds = canRequestAds;
            if( IsInitComplete )
            {
                Log( $"SetCansRequestAds {canRequestAds}" );
                AMRSDK.setCanRequestAds( canRequestAds );
            }
            else
            {
                Log( $"SetCansRequestAds {canRequestAds} WAIT FOR INIT" );
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

            Log( $"Initialize debug:{AdImplementation.IsDebug} ConsentType:{AdImplementation.ConsentType} ConsentResponse:{AdImplementation.ConsentResponse}" );
            
            var config = new AMRSdkConfig();
            
            #if UNITY_IOS
            config.ApplicationIdIOS = AppID;
            #elif UNITY_ANDROID
            config.ApplicationIdAndroid = AppID;
            #endif

            /*if( !AdImplementation.UserConsentManagedExternaly )
            {
                config.CanRequestAds = AdImplementation.IsDebug || AdImplementation.ConsentResponse is "OK" or "NON" ? "1" : "0";
                config.UserConsent   = AdImplementation.IsDebug || AdImplementation.ConsentResponse == "OK" ? "1" : "0";
                config.SubjectToGDPR = AdImplementation.ConsentType == "GDPR" ? "1" : "0";
                config.SubjectToCCPA = AdImplementation.ConsentType == "CCPA" ? "1" : "0";
            }
            else
            {
                
            }*/
            
            // il faudra absolument gerer le user consent avant l'initialisation
            config.CanRequestAds = AdImplementation.ConsentResponse is "OK" or "NON" ? "1" : "0";
            config.SubjectToGDPR = AdImplementation.ConsentType == "GDPR" ? "1" : "0";
            config.SubjectToCCPA = AdImplementation.ConsentType == "CCPA" ? "1" : "0";
            config.UserConsent   = AdImplementation.ConsentResponse == "OK" ? "1" : "0";
            
            config.IsUserChild = "0";
            
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

                if( CanRequestAds != null )
                {
                    Log( $"SetCansRequestAds after init complete {CanRequestAds}" );
                    AMRSDK.setCanRequestAds( CanRequestAds == true );
                }
                
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
            OnAdRewarded     = null;

            // heure de fermeture (ou d'echec) du visionnage en cours, pour mesurer la duree de la pub
            if( adinfo.Started && !adinfo.Sent && adinfo.HeureFermeture == 0 )
                adinfo.HeureFermeture = AdViewInfo.Maintenant();

            // send view statistics about this ad
            adinfo.SendIfNeeded();
        }

        /// <summary>
        /// La regie n'a jamais rappele apres un show (voir AdImplementation.SetMaxTimeBeforeAdShown): liberer
        /// AdPlaying, sinon HasRewardedAvailable / HasInterstitialAvailable renvoient false pour toute la
        /// session et plus aucune pub n'est possible jusqu'au relaunch de l'app.
        /// On ne touche PAS a OnAdPlayComplete / OnAdRewarded: si la pub finit malgre tout par s'afficher, ses
        /// callbacks doivent continuer a fonctionner (le jeu peut ainsi livrer le gain d'une rewarded tardive).
        /// </summary>
        public void ForceResetAdPlaying()
        {
            if( !AdPlaying )
                return;

            Log( "ForceResetAdPlaying: AdPlaying etait reste a true, on le libere" );
            AdPlaying = false;

            // compter cet echec (la regie n'a rien affiche dans le delai) : envoye avec Echec + SansReponse. Si la pub
            // finit par s'afficher, OnAdShow demarre un nouveau suivi avec la MEME HeureDemande : le serveur peut
            // rapprocher les deux et garder le visionnage. Un echec tardif de cette meme demande n'est pas recompte
            // (cf. OnAdFailToShow).
            var adinfo = IsRewardedPlaying ? RewardAdInfo : InterstitialAdInfo;
            if( !adinfo.Started || adinfo.Sent )
                DemarrerSuivi( adinfo, DemandeReseau ?? "N/A", 0 );
            adinfo.Echec          = true;
            adinfo.SansReponse    = true;
            adinfo.Complete       = false;
            adinfo.HeureFermeture = AdViewInfo.Maintenant();
            adinfo.Save();
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
        /// true si une video intersticielle est en cours de chargement
        /// </summary>
        public bool HasInterstitialLoading( string zoneID = null )
        {
            if( zoneID == null && AdInterUnit?.Count > 0 )
                zoneID = AdInterUnit[0];
            
            Log( $"HasInterstitialLoading: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - zoneID:{zoneID} - AdLoading:{InterstitalAdsControlller.IsLoading( zoneID )}" );
            return IsInitComplete && !AdPlaying && InterstitalAdsControlller.IsLoading( zoneID );
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
                NoterDemandeAffichage( InterstitalAdsControlller, zoneID );
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
        /// true si une video rewarded est en cours de chargement
        /// </summary>
        public bool HasRewardedLoading( string zoneID = null )
        {
            if( zoneID == null && AdRewarUnit?.Count > 0 )
                zoneID = AdRewarUnit[0];
            
            Log( $"HasRewardedLoading: IsInitComplete:{IsInitComplete} - AdPlaying:{AdPlaying} - zoneID:{zoneID} - AdLoading:{RewardedAdsControlller.IsLoading( zoneID )}" );
            return IsInitComplete && !AdPlaying && RewardedAdsControlller.IsLoading( zoneID );
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
        /// <param name="ssvExtra"></param>
        /// <param name="OnReward"></param>
        public void ShowRewarded( Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null )
        {
            Log( "ShowRewarded" );
            
            if( AdRewarUnit?.Count > 0 )
                ShowRewarded( AdRewarUnit[0], OnComplete, tag, ssvExtra, OnReward );
        }

        /// <summary>
        /// Afficher une video rewarded
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="OnComplete"></param>
        /// <param name="tag"></param>
        /// <param name="ssvExtra"></param>
        /// <param name="OnReward"></param>
        public void ShowRewarded( string zoneID, Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null )
        {
            Log( $"ShowRewarded zoneID:{zoneID}" );
            
            if( HasRewardedAvailable(zoneID) )
            {
                AdPlaying         = true;
                OnAdPlayComplete  = OnComplete;
                OnAdRewarded      = OnReward;
                IsRewardedPlaying = true;
                NoterDemandeAffichage( RewardedAdsControlller, zoneID );
                var ok = RewardedAdsControlller.PlayAd( zoneID, this, tag, ssvExtra );
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

        // la demande d'affichage en cours, notee juste avant PlayAd: de quoi mesurer l'age de la pub prechargee et
        // l'attente du joueur (cf. AdViewInfo). Reinitialisee avec le reste dans ResetStatics.
        private static string DemandeZone;
        private static string DemandeReseau;
        private static int    DemandeAgeMinutes = -1;
        private static long   DemandeHeure;

        private static void NoterDemandeAffichage( AdMostAd controleur, string zoneID )
        {
            DemandeZone       = zoneID;
            DemandeReseau     = controleur.GetReseau( zoneID );
            DemandeAgeMinutes = controleur.GetAgeMinutes( zoneID );
            DemandeHeure      = AdViewInfo.Maintenant();
        }

        /// <summary>
        /// demarrer le suivi d'un visionnage avec les infos de la demande d'affichage en cours
        /// </summary>
        private static void DemarrerSuivi( AdViewInfo adinfo, string networkName, double ecpm )
        {
            adinfo.Start( networkName, ecpm );
            adinfo.Zone              = DemandeZone;
            adinfo.NetworkChargement = networkName;
            adinfo.AgeMinutes        = DemandeAgeMinutes;
            adinfo.HeureDemande      = DemandeHeure;
            adinfo.Save();
        }

        public void OnAdShow( string networkName, double ecpm )
        {
            Log( $"OnAdShow networkName:{networkName} ecpm:{ecpm}" );

            // PAS de NotifyAdShown ici : AdMost envoie onShown AVANT de savoir si la regie affiche vraiment la pub
            // (constate le 05/10/2026 : onShown puis « The ad has expired » chez Unity Ads). Prevenir le jeu ici
            // desarmait les filets de securite (SetMaxTimeBeforeAdShown, et celui du jeu) pour une pub qui ne
            // s'affichera peut-etre jamais. Le jeu est prevenu a l'impression (OnAdImpression).
            // Sauf dans l'editeur : la simulation AMR envoie onShown mais jamais d'impression, et sa fenetre de test
            // ne bloque pas le jeu (sans ca, SetOnAdShown ne partirait jamais et la securite se declencherait).
            #if UNITY_EDITOR
            AdImplementation.NotifyAdShown();
            #endif

            DemarrerSuivi( IsRewardedPlaying ? RewardAdInfo : InterstitialAdInfo, networkName, ecpm );
        }

        public void OnAdImpression( AMRAd ad )
        {
            if( ad == null )
                return;
            
            Log( $"OnAdImpression rewarded:{IsRewardedPlaying} network:{ad.Network} zone:{ad.ZoneId} space:{ad.AdSpaceId} currency:{ad.Currency} revenu:{ad.Revenue}" );

            // la pub est REELLEMENT a l'ecran (y compris celle qu'AdMost trouve a la volee apres un refus) : prevenir
            // le jeu (SetOnAdShown) et desarmer le filet de securite. Sur Android, l'activite de la pub met aussi
            // l'app en pause, ce que les filets detectent deja pour une regie qui n'enverrait pas d'impression.
            AdImplementation.NotifyAdShown();

            var adinfo = IsRewardedPlaying ? RewardAdInfo : InterstitialAdInfo;

            // la securite a deja envoye cette demande comme echec SansReponse, mais la pub (souvent une pub de
            // remplacement trouvee a la volee par AdMost) s'affiche finalement. AdMost ne renvoie PAS de onShown dans
            // ce cas (logs du 05/10/2026 : un seul OnAdShow, puis l'impression de la regie de remplacement) : on
            // redemarre donc le suivi ici, avec la meme HeureDemande, pour que le visionnage soit envoye a la
            // fermeture et que le serveur remplace l'echec par ce visionnage.
            if( adinfo.Sent && adinfo.SansReponse && adinfo.HeureDemande == DemandeHeure )
                DemarrerSuivi( adinfo, adinfo.NetworkChargement, adinfo.eCPM * 100 );

            adinfo.Revenus         = ad.Revenue;
            adinfo.RevenusCurrency = ad.Currency;
            // la regie qui a REELLEMENT servi la pub: quand la pub prechargee echoue a l'affichage (expiree), AdMost
            // en recharge une a la volee, souvent chez une autre regie (Unity Ads -> AppLovin le 05/10/2026). Celle
            // du chargement, posee par OnAdShow, serait fausse dans les stats envoyees au serveur.
            if( !string.IsNullOrEmpty( ad.Network ) )
                adinfo.Network = ad.Network;
            if( adinfo.HeureImpression == 0 )
                adinfo.HeureImpression = AdViewInfo.Maintenant();
            adinfo.Save();

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
            Log( $"OnAdComplete rewarded:{IsRewardedPlaying} {OnAdRewarded}" );

            if( IsRewardedPlaying )
            {
                RewardAdInfo.Complete = true;
                OnAdRewarded?.Invoke();
                OnAdRewarded = null;
            }
        }
        
        public void OnAdReward(double amount)
        {
            Log( $"OnAdReward {amount} rewarded:{IsRewardedPlaying} {OnAdRewarded}" );

            if( IsRewardedPlaying )
            {
                RewardAdInfo.Complete = true;
                OnAdRewarded?.Invoke();
                OnAdRewarded = null;
            }
        }

        public void OnAdFailToShow()
        {
            Log( $"OnAdFailToShow rewarded:{IsRewardedPlaying}" );

            // La pub n'a pas ete vue: c'est un ECHEC, envoye comme tel (Echec = true) et non comme un visionnage
            // « non complete ». AdMost signale souvent onShown AVANT l'echec (constate le 05/10/2026 sur une pub Unity
            // Ads expiree): le suivi est alors deja demarre, on le complete. Sinon on le demarre ici, pour que chaque
            // echec soit compte. Le jeu doit traiter Echec a part (ni visionnage, ni stats anti-fraude).
            var adinfo = IsRewardedPlaying ? RewardAdInfo : InterstitialAdInfo;

            // echec deja compte pour cette demande par la securite SetMaxTimeBeforeAdShown (ForceResetAdPlaying) :
            // on ne le recompte pas, on rend juste la main
            if( adinfo.Sent && adinfo.Echec && adinfo.HeureDemande == DemandeHeure )
            {
                AdComplete( false, IsRewardedPlaying );
                return;
            }

            if( !adinfo.Started || adinfo.Sent )
                DemarrerSuivi( adinfo, DemandeReseau ?? "N/A", 0 );
            adinfo.Echec    = true;
            adinfo.Complete = false;
            adinfo.Save();

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

        // -- mesure (06/10/2026) : de quoi regler la duree de vie des pubs par regie et suivre l'attente des joueurs --
        /// <summary>l'emplacement AdMost de la pub</summary>
        public string Zone;
        /// <summary>la regie de la pub PRECHARGEE. Network = celle de l'impression : si elles different, la pub
        /// prechargee a ete refusee et AdMost en a trouve une autre a la volee</summary>
        public string NetworkChargement;
        /// <summary>age de la pub prechargee au moment de la demande d'affichage, en minutes (-1 = inconnu)</summary>
        public int    AgeMinutes = -1;
        /// <summary>true si l'affichage a ECHOUE (pub refusee, rien trouve d'autre) : la pub n'a pas ete vue</summary>
        public bool   Echec;
        /// <summary>true si l'echec vient de la securite SetMaxTimeBeforeAdShown : la regie n'a rien affiche ni
        /// rappele dans le delai. Si la pub s'affiche plus tard, un visionnage avec la meme HeureDemande suit.</summary>
        public bool   SansReponse;
        /// <summary>timestamps unix (s, UTC) de la demande d'affichage, de l'impression et de la fermeture (ou de
        /// l'echec). 0 = inconnu. Impression - Demande = attente du joueur ; Fermeture - Impression = duree de la pub</summary>
        public long   HeureDemande;
        public long   HeureImpression;
        public long   HeureFermeture;

        public static long Maintenant() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        
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

            Zone              = null;
            NetworkChargement = network;
            AgeMinutes        = -1;
            Echec             = false;
            SansReponse       = false;
            HeureDemande      = 0;
            HeureImpression   = 0;
            HeureFermeture    = 0;

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
                   $"{nameof( Started )}: {Started}, "                 +
                   $"{nameof( Zone )}: {Zone}, "                       +
                   $"{nameof( NetworkChargement )}: {NetworkChargement}, " +
                   $"{nameof( AgeMinutes )}: {AgeMinutes}, "           +
                   $"{nameof( Echec )}: {Echec}, "                     +
                   $"{nameof( SansReponse )}: {SansReponse}, "         +
                   $"{nameof( HeureDemande )}: {HeureDemande}, "       +
                   $"{nameof( HeureImpression )}: {HeureImpression}, " +
                   $"{nameof( HeureFermeture )}: {HeureFermeture}";
        }
    }
}