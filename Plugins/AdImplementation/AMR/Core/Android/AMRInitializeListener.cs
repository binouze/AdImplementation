using AMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AMRPlugin.Android
{
    public class AMRInitializeListener : AndroidJavaProxy
    {
        private AMRInitializeDelegate initializeDelegateObject;

        public AMRInitializeListener()
            : base("com.amr.unity.ads.UnityInitializationListener")
        {

        }

        public void setDelegateObject(AMRInitializeDelegate delegateObject)
        {
            initializeDelegateObject = delegateObject;
        }


        #region Callbacks from AMRInitializeListener.

        void onInitFailed(int errorCode)
        {
            if (initializeDelegateObject != null)
            {
                initializeDelegateObject.didSDKInitialize(false, errorCode + "");
            }
        }

        void onInitCompleted()
        {
            if (initializeDelegateObject != null)
            {
                initializeDelegateObject.didSDKInitialize(true,"");
            }
        }

        #endregion
    }
}
