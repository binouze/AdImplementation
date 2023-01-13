namespace AMR
{
    public interface AMRRemoteConfigDelegate
    {
        void onFetchComplete();
        void onFetchFail(string message);
    }
}