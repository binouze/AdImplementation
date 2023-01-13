namespace AMR
{
	public interface AMRInterstitialViewDelegate
	{
		void didReceiveInterstitial(string networkName, double ecpm);
		void didFailtoReceiveInterstitial(string error);
        void didFailtoShowInterstitial(string error);
        void didShowInterstitial();
        void didClickInterstitial(string networkName);
        void didImpressionInterstitial(AMRAd ad);
        void didDismissInterstitial();
        void didStatusChangeInterstitial(int status);
	}
}