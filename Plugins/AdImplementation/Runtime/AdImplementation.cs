using System;
using JetBrains.Annotations;
using UnityEngine;

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

        public static string AdMobTestDevice { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetTestDeviceAdMob( string adMobDevice )
        {
            AdMobTestDevice = adMobDevice;
        }
        
        public static string UMPTestDevice { get; private set; } = string.Empty;
        [UsedImplicitly]
        public static void SetTestDeviceUMP( string umpDevice )
        {
            UMPTestDevice = umpDevice;
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
            implementation.Initialize();
        }
        
        [UsedImplicitly]
        public static bool HasRewardedAvailable => implementation.HasRewardedAvailable();
        
        [UsedImplicitly]
        public static bool HasInterstitialAvailable => implementation.HasInterstitialAvailable();
        
        [UsedImplicitly]
        public static void ShowInterstitial( Action<bool> OnComplete )
        {
            GoogleUserMessagingPlatform.ShowFormIfRequired( () =>
            {
                implementation.ShowInterstitial(OnComplete);
            } );
        }
        
        [UsedImplicitly]
        public static void ShowRewarded( Action<bool> OnComplete )
        {
            GoogleUserMessagingPlatform.ShowFormIfRequired( () =>
            {
                implementation.ShowRewarded(OnComplete);
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
    }
}