namespace AMR
{
	public interface AMRRewardedVideoViewDelegate
	{
		void didReceiveRewardedVideo(string networkName, double ecpm);
		void didFailtoReceiveRewardedVideo(string error);
        void didShowRewardedVideo();
		void didFailtoShowRewardedVideo(string errorCode);
        void didClickRewardedVideo(string networkName);
		void didImpressionRewardedVideo(AMRAd ad);
		void didCompleteRewardedVideo();
		void didDismissRewardedVideo();
        void didStatusChangeRewardedVideo(int status);
		void didRewardRewardedVideo(double rewardAmount);
	}
}