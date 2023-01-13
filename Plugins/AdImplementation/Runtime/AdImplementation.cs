using System;
using AMR;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;

namespace com.binouze
{
    public enum TargetChildren
    {
        TRUE,
        FALSE,
        UNDEFINED
    }
    
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
        
        public static TargetChildren TargetChildrenType { get; private set; } = TargetChildren.FALSE;
        [UsedImplicitly]
        public static void SetTargetChildrenType( TargetChildren targetChildrenType )
        {
            TargetChildrenType = targetChildrenType;
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

        public static string TestDevice { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetTestDeviceAdMob( string testDevice )
        {
            TestDevice = testDevice;
        }
        
        public static bool IsActive { get; private set; } = true;
        [UsedImplicitly]
        public static void SetIsActive( bool isActive )
        {
            IsActive = isActive;
        }
        
        public static string UserId { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetUserID( string userId )
        {
            UserId = userId;
        }

        [UsedImplicitly]
        public static void SetUnitIds( string appID, string rewardedId, string interstitialId )
        {
            implementation.SetIds( appID, rewardedId, interstitialId );
        }

        private static bool IsReady;
        
        [UsedImplicitly]
        public static bool IsAdSupported() => IsReady;

        [UsedImplicitly]
        public static void Initialize()
        {
            if( IsReady )
                return;
            
            AdsAsyncUtils.SetInstance();
            implementation.Initialize();
            
            AMRSDK.setPrivacyConsentRequired(privacyConsentRequired);
        }

        private static string ConsentType;
        // The function below will be called just once when you subscribe the callback function as shown above
        public static void privacyConsentRequired(string consentType)
        {
            Debug.Log("ADMOST - privacyConsentRequired : " + consentType);
            ConsentType = consentType;
        }

        private static void OnSDKDidInitialize( bool success, string error )
        {
            if( !success )
            {
                Debug.Log( $"[AdImplementation] FAil iniitialize SDK {error}" );
                Debug.LogException( new Exception("[AdImplementation] FAil iniitialize SDK") );
            }
            else
            {
                IsReady = true;
                
                AMRSDK.loadInterstitial();
                AMRSDK.loadRewardedVideo();
                
            }
        }
        
        [UsedImplicitly]
        public static bool HasRewardedAvailable => IsReady && implementation.HasRewardedAvailable();
        
        [UsedImplicitly]
        public static bool HasInterstitialAvailable => IsReady && implementation.HasInterstitialAvailable();
        
        [UsedImplicitly]
        public static void ShowInterstitial( Action<bool> OnComplete )
        {
            if( !HasInterstitialAvailable )
            {
                OnComplete?.Invoke( false );
                return;
            }
            
            OnAdOpen?.Invoke();
            GoogleUserMessagingPlatform.ShowFormIfRequired( _ =>
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
            if( !HasRewardedAvailable )
            {
                OnComplete?.Invoke( false );
                return;
            }
            
            OnAdOpen?.Invoke();
            GoogleUserMessagingPlatform.ShowFormIfRequired( _ =>
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