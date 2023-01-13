using System;
namespace AMR
{
	public class Enums
	{
		public enum AMRSDKBannerPosition
		{
			BannerPositionTop = 0,
			BannerPositionBottom = 1,
			BannerPositionCenter = 2
		}
        public enum AMRSDKTrackPurchaseResult
        {
            SuccessfullyValidated = 0,
            FailedToValidate = 1,
            Exception = 2
        }
    }
}

