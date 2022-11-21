using System;
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
            implementation = new AdMobImplementation();
        }
        
        internal static void Log( string str )
        {
            if( LogEnabled )
                Debug.Log( $"[AdImplementation] {str}" );
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

        public static string AdMobTestDevice { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetTestDeviceAdMob( string adMobDevice )
        {
            AdMobTestDevice = adMobDevice;
        }
        
        public static string UMPTestDevice { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetUMPTestDevice( string umpDevice )
        {
            UMPTestDevice = umpDevice;
        }
        
        public static bool UMPResetForm { get; private set; }
        [UsedImplicitly]
        public static void SetUMPResetForm( bool _UMPResetForm )
        {
            UMPResetForm = _UMPResetForm;
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
        public static void SetUnitIds( string rewardedId, string interstitialId )
        {
            implementation.SetUnitIds( rewardedId, interstitialId );
        }
        
        [UsedImplicitly]
        public static bool IsAdSupported() => implementation.IsAdSupported();

        [UsedImplicitly]
        public static void Initialize()
        {
            AdsAsyncUtils.SetInstance();
            implementation.Initialize();
        }
        
        [UsedImplicitly]
        public static bool HasRewardedAvailable => implementation.HasRewardedAvailable();
        
        [UsedImplicitly]
        public static bool HasInterstitialAvailable => implementation.HasInterstitialAvailable();
        
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
        [UsedImplicitly] public float  ImpressionRevenue;
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