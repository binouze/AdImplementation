namespace AMR
{
    public interface AMROfferWallViewDelegate
    {
        void didReceiveOfferWall(string networkName, double ecpm);
        void didFailToReceiveOfferWall(string error);
        void didDismissOfferWall();
    }
}