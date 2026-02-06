using System;

namespace AMR
{
    public interface IAMRRemoteConfig
    {
        void fetchRemoteConfig(AMRRemoteConfigDelegate delegateObject, string appId, Action onComplete);
        double getRemoteConfigDouble(string key, double defaultValue, bool isTestConfigForEditor = false);
        string getRemoteConfigString(string key, string defaultValue, bool isTestConfigForEditor = false);
        long getRemoteConfigLong(string key, long defaultValue, bool isTestConfigForEditor = false);
        bool getRemoteConfigBoolean(string key, bool defaultValue, bool isTestConfigForEditor = false);
    }
}