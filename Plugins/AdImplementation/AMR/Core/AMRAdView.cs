namespace AMR
{
    public class AMRAdView
    {
        public delegate void EventDelegateReady(string networkName, double ecpm);
        public delegate void EventDelegateFail(string errorMessage);
        public delegate void EventDelegateShow();
	    public delegate void EventDelegateFailToShow();
        public delegate void EventDelegateClick(string networkName);
        public delegate void EventDelegateImpression(AMRAd ad);
        public delegate void EventDelegateComplete();
        public delegate void EventDelegateDismiss();
        public delegate void EventDelegateStatusChange(int status);
        public delegate void EventDelegateReward(double rewardAmount);

        public delegate void AdDelegateReady(string zoneId, string networkName, double ecpm);
        public delegate void AdDelegateFail(string zoneId, string errorMessage);
        public delegate void AdDelegateShow(string zoneId);
        public delegate void AdDelegateFailToShow(string zoneId);
        public delegate void AdDelegateClick(string zoneId, string networkName);
        public delegate void AdDelegateImpression(AMRAd ad);
        public delegate void AdDelegateComplete(string zoneId);
        public delegate void AdDelegateDismiss(string zoneId);
        public delegate void AdDelegateReward(string zoneId, double amount);
    }
}