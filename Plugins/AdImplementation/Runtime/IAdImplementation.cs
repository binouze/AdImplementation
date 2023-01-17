using System;

namespace com.binouze
{
    internal interface IAdImplementation
    {
        public void SetIds( string appID, string rewardedId, string interstitialId );
        public void Initialize();
        public void SetUserID( string id );
        
        public bool HasRewardedAvailable();
        public bool HasInterstitialAvailable();
        public bool IsAdSupported();
        
        public void ShowInterstitial( Action<bool> OnComplete );
        public void ShowRewarded( Action<bool>     OnComplete );
    }
}