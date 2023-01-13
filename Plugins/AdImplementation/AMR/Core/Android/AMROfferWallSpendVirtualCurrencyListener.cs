using AMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMROfferWallSpendVirtualCurrencyListener : AndroidJavaProxy
    {
        private AMRVirtualCurrencyDelegate vcDelegateObject;

        public AMROfferWallSpendVirtualCurrencyListener()
            : base("com.amr.unity.ads.UnityOfferWallVCListener")
        {

        }

        public void setDelegateObject(AMRVirtualCurrencyDelegate delegateObject)
        {
            vcDelegateObject = delegateObject;
        }


        #region Callbacks from UnityOfferWallVCListener.

        void didSpendVirtualCurrency(string network, string currency, double amount)
        {
            if (vcDelegateObject != null)
            {
                vcDelegateObject.didSpendVirtualCurrency(network, currency, amount);
            }
        }

        #endregion
    }
}
