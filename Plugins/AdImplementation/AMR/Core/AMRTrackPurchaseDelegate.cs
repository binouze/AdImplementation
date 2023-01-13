
namespace AMR
{
    public interface AMRTrackPurchaseDelegate
    {
        void onResult(string purchaseId, AMR.Enums.AMRSDKTrackPurchaseResult responseCode);
    }
}