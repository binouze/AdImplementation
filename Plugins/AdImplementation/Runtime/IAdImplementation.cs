using System;
using System.Collections.Generic;

namespace com.binouze
{
    internal interface IAdImplementation
    {
        public void SetIds( string appID, List<string> rewardedId, List<string> interstitialId );
        public void Initialize();
        public void SetUserID( string id );
        
        public bool HasRewardedAvailable( string zoneID = null );
        public bool HasInterstitialAvailable( string zoneID = null );
        public bool IsAdSupported();

        public void LoadInterstitial( string zoneID = null );
        public void LoadRewarded( string zoneID = null );
        
        public void ShowInterstitial( string zoneID, Action<bool> OnComplete, string tag = null );
        public void ShowRewarded( string zoneID, Action<bool> OnComplete, string tag = null );
        public void ShowInterstitial( Action<bool> OnComplete, string tag = null );
        public void ShowRewarded( Action<bool> OnComplete, string tag = null );
    }
}