namespace AMR
{
    public interface IAMROfferWall
    {
        void loadOfferWallForZoneId(string zoneId, AMROfferWallViewDelegate delegateObject);
        void showOfferWall();
        void showOfferWall(string tag);
        void destroyOfferWall();
    }
}