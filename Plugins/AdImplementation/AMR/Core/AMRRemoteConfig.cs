using System;
using UnityEngine;
using AMR;

namespace AMR
{
    public class AMRRemoteConfig
    {
        private static AMRRemoteConfig instance;

        private static AMRRemoteConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AMRRemoteConfig();
                }

                return instance;
            }
        }

        private readonly IAMRRemoteConfig platformConfig;

        private AMRRemoteConfig()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                platformConfig = new iOS.AMRRemoteConfig();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                platformConfig = new Android.AMRRemoteConfig();
            }
            else if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                platformConfig = new Core.UnityEditor.AMRRemoteConfig();
            }
        }


        #region Remote Config Value

        public static double getDouble(string key, double defaultValue, bool isTestConfigForEditor = false)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigDouble(key, defaultValue, isTestConfigForEditor);
        }

        public static long getLong(string key, long defaultValue, bool isTestConfigForEditor = false)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigLong(key, defaultValue, isTestConfigForEditor);
        }

        public static string getString(string key, string defaultValue, bool isTestConfigForEditor = false)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigString(key, defaultValue, isTestConfigForEditor);
        }

        public static bool getBoolean(string key, bool defaultValue, bool isTestConfigForEditor = false)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigBoolean(key, defaultValue, isTestConfigForEditor);
        }

        private static bool isKeyApplicable(string key)
        {
            if (key == null || key.Equals(""))
            {
                return false;
            }

            return true;
        }


        public static void fetch(AMRRemoteConfigDelegate rmDelegate, string appId, Action onComplete)
        {
            if (AMRSDK.initialized() == false)
            {
                AMRUtil.Log("AMRSDK not initialized!");
                return;
            }

            Instance.platformConfig.fetchRemoteConfig(rmDelegate, appId, onComplete);
        }

        #endregion
    }
}
