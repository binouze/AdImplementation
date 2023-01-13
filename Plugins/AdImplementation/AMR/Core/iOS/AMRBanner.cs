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
#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _setBannerSuccessCallback(BannerSuccessCallback cb);

		[DllImport("__Internal")]
		private static extern void _setBannerFailCallback(BannerFailCallback cb);

		[DllImport("__Internal")]
		private static extern void _setBannerShowCallback(BannerShowCallback cb);

		[DllImport("__Internal")]
		private static extern void _setBannerClickCallback(BannerClickCallback cb);

		[DllImport("__Internal")]
		private static extern IntPtr _loadBannerForZoneId(string zoneId,
			AMR.Enums.AMRSDKBannerPosition position,
            int offset,
			IntPtr bannerHandle);

		[DllImport("__Internal")]
		private static extern IntPtr _loadBannerForZoneIdWithPosition(string zoneId,
			double positionX,
			double positionY,
			IntPtr bannerHandle);

		[DllImport("__Internal")]
		private static extern void _showBanner(IntPtr bannerPtr);

		[DllImport("__Internal")]
		private static extern void _hideBanner(IntPtr bannerPtr);

		[DllImport("__Internal")]
		private static extern void _destroyBanner(IntPtr bannerPtr);

#endif
		private delegate void BannerSuccessCallback(IntPtr bannerHandlePtr, string networkName, double ecpm);
		private delegate void BannerFailCallback(IntPtr bannerHandlePtr, string error);
		private delegate void BannerClickCallback(IntPtr bannerHandlePtr, string networkName);
		private delegate void BannerShowCallback(IntPtr bannerHandlePtr, string zoneId, string networkName, double ecpm, string adSpaceId);

		private IntPtr bannerPtr;

		[MonoPInvokeCallback(typeof(BannerSuccessCallback))]
		private static void bannerSuccessCallback(IntPtr bannerHandlePtr, string networkName, double ecpm)
		{
			GCHandle bannerHandle = (GCHandle)bannerHandlePtr;
			AMRBannerViewDelegate delegateObject = bannerHandle.Target as AMRBannerViewDelegate;
			delegateObject.didReceiveBanner(networkName, ecpm);

		}

        [MonoPInvokeCallback(typeof(BannerFailCallback))]
		private static void bannerFailCallback(IntPtr bannerHandlePtr, string error)
		{
			GCHandle bannerHandle = (GCHandle)bannerHandlePtr;
			AMRBannerViewDelegate delegateObject = bannerHandle.Target as AMRBannerViewDelegate;
			delegateObject.didFailtoReceiveBanner(error);
		}

		[MonoPInvokeCallback(typeof(BannerShowCallback))]
		private static void bannerShowCallback(IntPtr bannerHandlePtr, string zoneId, string networkName, double ecpm, string adSpaceId)
		{
			// zoneId, network, ecpm, adspaceId
			double revenue = Convert.ToDouble(ecpm) / 1000.0 / 100.0;
			AMRAd ad = new AMRAd(zoneId, networkName)
			{
				Revenue = Convert.ToDouble(revenue),
				AdSpaceId = adSpaceId
			};

			GCHandle bannerHandle = (GCHandle)bannerHandlePtr;
			AMRBannerViewDelegate delegateObject = bannerHandle.Target as AMRBannerViewDelegate;
			delegateObject.didImpressionBanner(ad);

		}

		[MonoPInvokeCallback(typeof(BannerClickCallback))]
		private static void bannerClickCallback(IntPtr bannerHandlePtr, string networkName)
		{
			GCHandle bannerHandle = (GCHandle)bannerHandlePtr;
			AMRBannerViewDelegate delegateObject = bannerHandle.Target as AMRBannerViewDelegate;
			delegateObject.didClickBanner(networkName);

		}

		#region - IAMRBanner

		public void loadBannerForZoneId(string zoneId, 
		                                AMR.Enums.AMRSDKBannerPosition position,
                                        int offset,
		                                AMRBannerViewDelegate delegateObject)
		{

#if UNITY_IOS
            _setBannerSuccessCallback(bannerSuccessCallback);
            _setBannerClickCallback(bannerClickCallback);
			_setBannerFailCallback(bannerFailCallback);

            GCHandle handle = GCHandle.Alloc(delegateObject);
			IntPtr parameter = (IntPtr)handle;
            // call WinAPi and pass the parameter here
            bannerPtr = _loadBannerForZoneId(zoneId,
				                                position,
                                                offset,
				                                parameter);
#endif
        }

		public void loadBannerForZoneIdWithPosition(string zoneId,
					    					double positionX,
						    				double positionY,
							    			AMRBannerViewDelegate delegateObject)
		{

#if UNITY_IOS
			_setBannerSuccessCallback(bannerSuccessCallback);
			_setBannerClickCallback(bannerClickCallback);
			_setBannerFailCallback(bannerFailCallback);

			GCHandle handle = GCHandle.Alloc(delegateObject);
			IntPtr parameter = (IntPtr)handle;
			// call WinAPi and pass the parameter here
			bannerPtr = _loadBannerForZoneIdWithPosition(zoneId,
												positionX,
												positionY,
												parameter);
#endif
		}

		public void showBanner()
		{
#if UNITY_IOS
            _showBanner(bannerPtr);
#endif
		}

		public void hideBanner()
		{
#if UNITY_IOS
            _hideBanner(bannerPtr);
#endif
		}

        public void destroyBanner()
        {
#if UNITY_IOS
			_destroyBanner(bannerPtr);
#endif
		}

		#endregion
	}
}

