using System;
using UnityEngine;
using System.Runtime.InteropServices;
using AMR;

namespace AMRPlugin.iOS
{
    public class AMROfferWall: IAMROfferWall
    {
#if UNITY_IOS
	    [DllImport("__Internal")]
	    private static extern void _setOfferWallSuccessCallback(OfferWallSuccessCallback cb);

	    [DllImport("__Internal")]
	    private static extern void _setOfferWallFailCallback(OfferWallFailCallback cb);

	    [DllImport("__Internal")]
	    private static extern void _setOfferWallDismissCallback(OfferWallDismissCallback cb);

	    [DllImport("__Internal")]
	    private static extern IntPtr _loadOfferWallForZoneId(string zoneId, IntPtr offerWallHandle);

	    [DllImport("__Internal")]
	    private static extern void _showOfferWall(IntPtr offerWallPtr);
	
		[DllImport("__Internal")]
	    private static extern void _showOfferWallWithTag(String tag, IntPtr offerWallPtr);
#endif

		private delegate void OfferWallSuccessCallback(IntPtr offerWallHandlePtr, string networkName, double ecpm);
		private delegate void OfferWallFailCallback(IntPtr offerWallHandlePtr, string error);
		private delegate void OfferWallDismissCallback(IntPtr offerWallHandlePtr);

		private IntPtr offerWallPtr;

		[MonoPInvokeCallback(typeof(OfferWallSuccessCallback))]
		private static void offerWallSuccessCallback(IntPtr offerWallHandlePtr, string networkName, double ecpm)
		{	
			GCHandle offerWallHandle = (GCHandle)offerWallHandlePtr;
			AMROfferWallViewDelegate delegateObject = offerWallHandle.Target as AMROfferWallViewDelegate;
			delegateObject.didReceiveOfferWall(networkName, ecpm);
		}

		[MonoPInvokeCallback(typeof(OfferWallFailCallback))]
		private static void offerWallFailCallback(IntPtr offerWallHandlePtr, string error)
		{
			GCHandle offerWallHandle = (GCHandle)offerWallHandlePtr;
			AMROfferWallViewDelegate delegateObject = offerWallHandle.Target as AMROfferWallViewDelegate;
			delegateObject.didFailToReceiveOfferWall(error);
		}

        [MonoPInvokeCallback(typeof(OfferWallDismissCallback))]
        private static void offerWallDismissCallback(IntPtr offerWallHandlePtr)
        {
            GCHandle offerWallHandle = (GCHandle)offerWallHandlePtr;
	        AMROfferWallViewDelegate delegateObject = offerWallHandle.Target as AMROfferWallViewDelegate;
            delegateObject.didDismissOfferWall();
        }

#region - IAMROfferWall

		public void loadOfferWallForZoneId(string zoneId, AMROfferWallViewDelegate delegateObject)
		{
#if UNITY_IOS
				_setOfferWallSuccessCallback(offerWallSuccessCallback);
				_setOfferWallFailCallback(offerWallFailCallback);
				_setOfferWallDismissCallback(offerWallDismissCallback);

				GCHandle handle = GCHandle.Alloc(delegateObject);
				IntPtr parameter = (IntPtr)handle;

				offerWallPtr = _loadOfferWallForZoneId(zoneId, parameter);
#endif
		}

		public void showOfferWall()
		{
#if UNITY_IOS
				_showOfferWall(offerWallPtr);
#endif
		}

        public void showOfferWall(String tag)
        {
#if UNITY_IOS
				_showOfferWallWithTag(tag, offerWallPtr);
#endif
        }

        public void destroyOfferWall()
        {
	        
        }
		
#endregion
    }
}