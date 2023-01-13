namespace AMR
{
	public interface AMRBannerViewDelegate
	{
		void didReceiveBanner(string networkName, double ecpm);
		void didFailtoReceiveBanner(string error);
		void didImpressionBanner(AMRAd ad);
		void didClickBanner(string networkName);
	}
}