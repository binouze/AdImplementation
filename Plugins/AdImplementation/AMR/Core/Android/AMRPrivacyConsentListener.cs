using AMR;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMRPrivacyConsentListener : AndroidJavaProxy
    {
        private AMRPrivacyConsentDelegate privacyConsentDelegateObject;

        public AMRPrivacyConsentListener()
            : base("com.amr.unity.ads.UnityPrivacyConsentListener")
        {

        }

        public void setDelegateObject(AMRPrivacyConsentDelegate delegateObject)
        {
            privacyConsentDelegateObject = delegateObject;
        }


        #region Callbacks from UnityOfferWallVCListener.

        void isPrivacyConsentRequired(string consentType)
        {
            if (privacyConsentDelegateObject != null)
            {
                privacyConsentDelegateObject.privacyConsentRequired(consentType);
            }
        }

        #endregion
    }
}
