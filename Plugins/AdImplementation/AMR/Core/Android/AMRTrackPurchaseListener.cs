using AMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMRTrackPurchaseListener : AndroidJavaProxy
    {
        private AMRTrackPurchaseDelegate tpDelegateObject;

        public AMRTrackPurchaseListener()
            : base("com.amr.unity.ads.UnityTrackPurchaseListener")
        {

        }

        public void setDelegateObject(AMRTrackPurchaseDelegate delegateObject)
        {
            tpDelegateObject = delegateObject;
        }


        #region Callbacks from UnityOfferWallVCListener.

        void onResult(string transactionId, int responseCode)
        {
            if (tpDelegateObject != null)
            {
                tpDelegateObject.onResult(transactionId, (Enums.AMRSDKTrackPurchaseResult)responseCode);
            }
        }

        #endregion
    }
}
