using System;
using System.ComponentModel;

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
        public enum AMRSDKAdFormat
        {
            INTER = 0,
            REWARDED= 1,
            REWARDED_INTER = 2,
            APPOPEN = 3,
            BANNER = 4,
            NATIVE = 5,
            LEADER = 6,
            MREC = 7
        }
    }
}

