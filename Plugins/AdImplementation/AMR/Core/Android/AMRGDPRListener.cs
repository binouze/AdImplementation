using AMR;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMRGDPRListener : AndroidJavaProxy
    {
        private AMRGDPRDelegate gdprDelegateObject;

        public AMRGDPRListener()
            : base("com.amr.unity.ads.UnityGDPRListener")
        {

        }

        public void setDelegateObject(AMRGDPRDelegate delegateObject)
        {
            gdprDelegateObject = delegateObject;
        }


        #region Callbacks from UnityOfferWallVCListener.

        void isGDPRApplicable(bool isApplicable)
        {
            if (gdprDelegateObject != null)
            {
                gdprDelegateObject.isGDPRApplicable(isApplicable);
            }
        }

        #endregion
    }
}
