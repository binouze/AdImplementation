using System;

namespace AMR
{
    public interface IAMRRemoteConfig
    {
        void fetchRemoteConfig(AMRRemoteConfigDelegate delegateObject);
        double getRemoteConfigDouble(string key, double defaultValue);
        string getRemoteConfigString(string key, string defaultValue);
        long getRemoteConfigLong(string key, long defaultValue);
        bool getRemoteConfigBoolean(string key, bool defaultValue);
    }
}
