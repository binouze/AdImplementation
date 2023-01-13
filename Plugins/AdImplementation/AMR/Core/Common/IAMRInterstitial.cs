using System;
namespace AMR
{
	public interface IAMRInterstitial
	{
		void loadInterstitialForZoneId(string zoneId, AMRInterstitialViewDelegate delegateObject);
		void showInterstitial();
        void showInterstitial(String tag);
        void destroyInterstitial();
		bool isReadyToShow();
    }
}

