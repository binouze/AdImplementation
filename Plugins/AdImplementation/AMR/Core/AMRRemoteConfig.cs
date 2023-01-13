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
        }


        #region Remote Config Value
        public static double getDouble(string key, double defaultValue)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigDouble(key, defaultValue);
        }

        public static long getLong(string key, long defaultValue)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigLong(key, defaultValue);
        }

        public static string getString(string key, string defaultValue)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigString(key, defaultValue);
        }

        public static bool getBoolean(string key, bool defaultValue)
        {
            if (AMRSDK.initialized() == false || !isKeyApplicable(key))
            {
                return defaultValue;
            }

            return Instance.platformConfig.getRemoteConfigBoolean(key, defaultValue);
        }

        private static bool isKeyApplicable(string key)
        {
            if (key == null || key.Equals(""))
            {
                return false;
            }

            return true;
        }


        public static void fetch(AMRRemoteConfigDelegate rmDelegate)
        {
            if (AMRSDK.initialized() == false) { return; }
            Instance.platformConfig.fetchRemoteConfig(rmDelegate);
        }

        #endregion
    }

}
