namespace AMR
{
    public interface AMRInitializeDelegate
    {
        void didSDKInitialize(bool isInitialized, string errorMessage);
    }
}