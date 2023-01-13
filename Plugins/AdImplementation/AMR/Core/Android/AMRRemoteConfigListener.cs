using AMR;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMRRemoteConfigListener : AndroidJavaProxy
    {
        private AMRRemoteConfigDelegate rcDelegateObject;

        public AMRRemoteConfigListener()
            : base("com.amr.unity.ads.UnityRemoteConfigListener")
        {

        }

        public void setDelegateObject(AMRRemoteConfigDelegate delegateObject)
        {
            rcDelegateObject = delegateObject;
        }


        #region Callbacks from UnityRemoteConfigListener.

        void onFetchComplete()
        {
            if (rcDelegateObject != null)
            {
                rcDelegateObject.onFetchComplete();
            }
        }

        void onFetchFail(string message)
        {
            if (rcDelegateObject != null)
            {
                rcDelegateObject.onFetchFail(message);
            }
        }

        #endregion
    }
}
