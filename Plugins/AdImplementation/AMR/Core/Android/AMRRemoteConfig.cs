using System;
using UnityEngine;

namespace AMR.Android
{
    public class AMRRemoteConfig : IAMRRemoteConfig
    {
        protected internal AndroidJavaObject config;
        protected internal AndroidJavaObject activity;
        private AMRPlugin.Android.AMRRemoteConfigListener rcListener;

        public AMRRemoteConfig()
        {
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity =
                    playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            config = new AndroidJavaObject("com.amr.unity.ads.RemoteConfig", activity);
            playerClass.Dispose();
            activity.Dispose();
        }

        public void fetchRemoteConfig(AMRRemoteConfigDelegate delegateObject, string appId, Action onComplete)
        {
            if (rcListener == null)
            {
                rcListener = new AMRPlugin.Android.AMRRemoteConfigListener();
            }
            rcListener.setDelegateObject(delegateObject);

            config.Call("fetchRemoteConfig", new object[1] { rcListener });
        }

        public double getRemoteConfigDouble(string key, double defaultValue, bool isTestConfigForEditor = false)
        {
            return config.Call<double>("getRemoteConfigDoubleValue", new object[2] { key, defaultValue });
        }

        public string getRemoteConfigString(string key, string defaultValue, bool isTestConfigForEditor = false)
        {
            return config.Call<string>("getRemoteConfigStringValue", new object[2] { key, defaultValue });
        }

        public long getRemoteConfigLong(string key, long defaultValue, bool isTestConfigForEditor = false)
        {
            return config.Call<long>("getRemoteConfigLongValue", new object[2] { key, defaultValue });
        }

        public bool getRemoteConfigBoolean(string key, bool defaultValue, bool isTestConfigForEditor = false)
        {
            return config.Call<bool>("getRemoteConfigBooleanValue", new object[2] { key, defaultValue });
        }
    }

}

