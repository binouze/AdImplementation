using System;
using UnityEngine;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MonoPInvokeCallbackAttribute : Attribute
{
	public MonoPInvokeCallbackAttribute(Type t) { }
}

namespace AMR.iOS
{
	public class AMRBanner:IAMRBanner
	{
		private delegate void BannerSuccessCallback(IntPtr bannerHandlePtr, string networkName, double ecpm);
		private delegate void BannerFailCallback(IntPtr bannerHandlePtr, string error);
		private delegate void BannerClickCallback(IntPtr bannerHandlePtr, string networkName);
		private delegate void BannerShowCallback(IntPtr bannerHandlePtr, string zoneId, string networkName, double ecpm, string adSpaceId);

		private IntPtr bannerPtr;

		[MonoPInvokeCallback(typeof(BannerSuccessCallback))]
		private static void bannerSuccessCallback(IntPtr bannerHandlePtr, string networkName, double ecpm) { }

        [MonoPInvokeCallback(typeof(BannerFailCallback))]
		private static void bannerFailCallback(IntPtr bannerHandlePtr, string error) { }

		[MonoPInvokeCallback(typeof(BannerShowCallback))]
		private static void bannerShowCallback(IntPtr bannerHandlePtr, string zoneId, string networkName, double ecpm, string adSpaceId) { }

		[MonoPInvokeCallback(typeof(BannerClickCallback))]
		private static void bannerClickCallback(IntPtr bannerHandlePtr, string networkName) { }

		#region - IAMRBanner

		public void loadBannerForZoneId(string zoneId, 
		                                AMR.Enums.AMRSDKBannerPosition position,
                                        int offset,
		                                AMRBannerViewDelegate delegateObject) { }

		public void loadBannerForZoneIdWithPosition(string zoneId,
					    					double positionX,
						    				double positionY,
							    			AMRBannerViewDelegate delegateObject) { }

		public void showBanner() { }

		public void hideBanner() { }

        public void destroyBanner() { }

		#endregion
	}
}

