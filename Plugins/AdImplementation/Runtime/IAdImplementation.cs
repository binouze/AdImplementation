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
        
        
        public void ShowInterstitial( string zoneID, Action<bool> OnComplete );
        public void ShowRewarded( string zoneID,Action<bool> OnComplete );
        public void ShowInterstitial( Action<bool> OnComplete );
        public void ShowRewarded( Action<bool> OnComplete );
    }
}