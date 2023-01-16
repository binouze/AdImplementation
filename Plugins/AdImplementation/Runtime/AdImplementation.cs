using System;
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
        
        private static bool LogEnabled;
        [UsedImplicitly]
        public static void SetLogEnabled( bool enabled )
        {
            LogEnabled = enabled;
        }
        
        public static bool IsDebug { get; private set; }
        [UsedImplicitly]
        public static void SetIsDebug( bool isDebug )
        {
            IsDebug = isDebug;
        }


        private static bool IsGDRPForced;
        [UsedImplicitly]
        public static void SetForceGDRP( bool force )
        {
            IsGDRPForced = force;
        }

        private static bool IsGDRPReset;
        [UsedImplicitly]
        public static void SetResetGDRP( bool reset )
        {
            IsGDRPReset = reset;
        }

        public static Action<ImpressionDatas> OnImpressionDatas { get; private set; }
        [UsedImplicitly]
        public static void SetImpressionDataHandler( Action<ImpressionDatas> onImpressionDatas )
        {
            OnImpressionDatas = onImpressionDatas;
        }

        private static Action OnAdOpen;
        [UsedImplicitly]
        public static void SetOnAdOpen( Action onAdOpen )
        {
            OnAdOpen = onAdOpen;
        }

        private static Action OnAdClose;
        [UsedImplicitly]
        public static void SetOnAdClose( Action onAdClose )
        {
            OnAdClose = onAdClose;
        }
        
        public static Action<string,bool> OnAdClicked;
        [UsedImplicitly]
        public static void SetOnAdClicked( Action<string, bool> onAdClicked )
        {
            OnAdClicked = onAdClicked;
        }

        private static Action<Action<bool>> ShowGDPRPopup;
        [UsedImplicitly]
        public static void SetGDPRFormFunction( Action<Action<bool>> showGDPRPopup )
        {
            ShowGDPRPopup = showGDPRPopup;
        }

        public static string UserId { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetUserID( string userId )
        {
            UserId = userId;
        }

        [UsedImplicitly]
        public static void SetIds( /*string appID,*/ string rewardedId, string interstitialId )
        {
            #if UNITY_ANDROID 
            var appid = AdImplementationSettings.LoadInstance().AndroidAppId;
            #elif UNITY_IOS
            var appid = AdImplementationSettings.LoadInstance().IOSAppId;
            #else
            var appid = "0";
            #endif
            
            implementation.SetIds( appid, rewardedId, interstitialId );
        }

        [UsedImplicitly]
        public static bool IsAdSupported() => implementation.IsAdSupported();

        private static bool                    IsInIt;
        private static bool                    IsInIt2;
        private static CancellationTokenSource Init2Cancellation;
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

        public static string ConsentType     { get; private set; }
        public static string ConsentResponse { get; private set; }

        // The function below will be called just once when you subscribe the callback function as shown above
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

        [UsedImplicitly]
        public static bool IsGDPRFormRequired() => ConsentType != "None";

        [UsedImplicitly]
        public static bool HasRewardedAvailable => implementation.HasRewardedAvailable();
        
        [UsedImplicitly]
        public static bool HasInterstitialAvailable => implementation.HasInterstitialAvailable();
        
        [UsedImplicitly]
        public static void ShowInterstitial( Action<bool> OnComplete )
        {
            Log( $"ShowInterstitial {HasInterstitialAvailable}" );
            
            if( !HasInterstitialAvailable )
            {
                OnComplete?.Invoke( false );
                return;
            }
            
            OnAdOpen?.Invoke();
            ShowGdprIfRequired( () =>
            {
                implementation.ShowInterstitial( ok =>
                {
                    AdsAsyncUtils.CallOnMainThread( () =>
                    {
                        OnAdClose?.Invoke();
                        OnComplete?.Invoke( ok );
                    });
                } );
            } );
        }

        [UsedImplicitly]
        public static void ShowRewarded( Action<bool> OnComplete )
        {
            Log( $"ShowRewarded {HasRewardedAvailable}" );
        
            if( !HasRewardedAvailable )
            {
                OnComplete?.Invoke( false );
                return;
            }
            
            OnAdOpen?.Invoke();
            ShowGdprIfRequired( () =>
            {
                implementation.ShowRewarded( ok =>
                {
                    AdsAsyncUtils.CallOnMainThread( () =>
                    {
                        OnAdClose?.Invoke();
                        OnComplete?.Invoke( ok );
                    } );
                } );
            } );
        }

        private static void ShowGdprIfRequired( Action complete )
        {
            Log( $"ShowGdprIfRequired {MustAskGDPR}" );
            
            if( MustAskGDPR )
                ShowGdprForm( complete );
            else
                complete?.Invoke();
        }
        
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

        private static void SetGDPRStatus( string status )
        {
            Log( $"SetGDPRStatus {status}" );

            if( status != "OK" && status != "NON" )
                status = "UNKNOWN";
            
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