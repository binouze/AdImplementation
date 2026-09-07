using System;
using System.Collections.Generic;

namespace com.binouze
{
    internal interface IAdImplementation
    {
        public void SetIds( string appID, List<string> rewardedId, List<string> interstitialId );
        public void Initialize();
        public void SetUserID( string      id );
        public void SetCanRequestAds( bool canRequestAd );
        
        public bool HasRewardedAvailable( string     zoneID = null );
        public bool HasInterstitialAvailable( string zoneID = null );
        public bool HasRewardedLoading( string       zoneID = null );
        public bool HasInterstitialLoading( string   zoneID = null );
        public bool IsAdSupported();

        public void LoadInterstitial( string zoneID = null );
        public void LoadRewarded( string     zoneID = null );
        
        public void ShowInterstitial( string zoneID, Action<bool> OnComplete, string tag = null );
        public void ShowRewarded( string zoneID, Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null );
        public void ShowInterstitial( Action<bool> OnComplete, string tag = null );
        public void ShowRewarded( Action<bool> OnComplete, string tag = null, Dictionary<string,string> ssvExtra = null, Action OnReward = null );

        /// <summary>
        /// liberer le flag interne "une pub est en cours" quand la regie n'a jamais rappele apres un show
        /// (voir AdImplementation.SetMaxTimeBeforeAdShown). Sans ca, plus aucune pub n'est possible de la session.
        /// Ne doit PAS annuler les callbacks en attente: si la pub finit par s'afficher, ils doivent fonctionner.
        /// </summary>
        public void ForceResetAdPlaying();
    }
}