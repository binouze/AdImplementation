using System;
using System.Runtime.InteropServices;

namespace AMR.iOS
{
    public class AMRRemoteConfig : IAMRRemoteConfig
    {
        #if UNITY_IOS
		[DllImport("__Internal")]
        private static extern string _getRemoteConfigString(string key, string defaultValue);

		[DllImport("__Internal")]
		private static extern double _getRemoteConfigDouble(string key, double defaultValue);

		[DllImport("__Internal")]
		private static extern long _getRemoteConfigLong(string key, long defaultValue);

		[DllImport("__Internal")]
		private static extern bool _getRemoteConfigBoolean(string key, bool defaultValue);
        #endif

        public AMRRemoteConfig() {}

        public void fetchRemoteConfig(AMRRemoteConfigDelegate delegateObject, string appId, Action onComplete)
        {
            throw new NotImplementedException();
        }

        public double getRemoteConfigDouble(string key, double defaultValue, bool isTestConfigForEditor = false)
        {
            #if UNITY_IOS
			return _getRemoteConfigDouble(key, defaultValue);

            #else
            return 0;
            #endif
        }

        public string getRemoteConfigString(string key, string defaultValue, bool isTestConfigForEditor = false)
        {
            #if UNITY_IOS
			return _getRemoteConfigString(key, defaultValue);
            #else
            return "";
            #endif
        }

        public long getRemoteConfigLong(string key, long defaultValue, bool isTestConfigForEditor = false)
        {
            #if UNITY_IOS
			return _getRemoteConfigLong(key, defaultValue);
            #else
            return 0;

            #endif
        }

        public bool getRemoteConfigBoolean(string key, bool defaultValue, bool isTestConfigForEditor = false)
        {
            #if UNITY_IOS
			return _getRemoteConfigBoolean(key, defaultValue);
            #else
            return false;

            #endif
        }
    }
}