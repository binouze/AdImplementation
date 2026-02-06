using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AMR.Core.UnityEditor
{
    public class AMRRemoteConfig : IAMRRemoteConfig
    {
        private RemoteConfigRoot remoteConfigObject;

        public async void fetchRemoteConfig(AMRRemoteConfigDelegate delegateObject, string appId, Action onComplete)
        {
            if (remoteConfigObject == null)
            {
                try
                {
                    string url = "https://cdn-api.admost.com/v5/config/remote/" + appId + "/version/" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    remoteConfigObject = await GetConfig<RemoteConfigRoot>(url);
                    onComplete?.Invoke();
                }
                catch (Exception e)
                {
                    AMRUtil.LogException("Exception while fetching remote config | e = " + e);
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public double getRemoteConfigDouble(string key, double defaultValue, bool isTestConfigForEditor = false)
        {
            Dictionary<string, object> dict = GetDict(isTestConfigForEditor);

            if (dict != null && dict.TryGetValue(key, out object value))
            {
                if (double.TryParse(value.ToString(), out double result))
                    return result;
            }

            return defaultValue;
        }

        public string getRemoteConfigString(string key, string defaultValue, bool isTestConfigForEditor = false)
        {
            Dictionary<string, object> dict = GetDict(isTestConfigForEditor);

            if (dict != null && dict.TryGetValue(key, out object value))
            {
                return value?.ToString() ?? defaultValue;
            }

            return defaultValue;
        }

        public long getRemoteConfigLong(string key, long defaultValue, bool isTestConfigForEditor = false)
        {
            Dictionary<string, object> dict = GetDict(isTestConfigForEditor);

            if (dict != null && dict.TryGetValue(key, out object value))
            {
                if (long.TryParse(value.ToString(), out long result))
                    return result;
            }

            return defaultValue;
        }

        public bool getRemoteConfigBoolean(string key, bool defaultValue, bool isTestConfigForEditor = false)
        {
            Dictionary<string, object> dict = GetDict(isTestConfigForEditor);
            if (dict != null && dict.TryGetValue(key, out object value))
            {
                if (bool.TryParse(value.ToString(), out bool result))
                    return result;

                string s = value.ToString();
                if (s == "1") return true;
                if (s == "0") return false;
            }

            return defaultValue;
        }

        private Dictionary<string, object> GetDict(bool isTestConfigForEditor)
        {
            if (remoteConfigObject == null)
            {
                AMRUtil.LogException("AMRRemoteConfig is null");
                return new Dictionary<string, object>();
            }

            return isTestConfigForEditor ? remoteConfigObject.TestRemoteConfig : remoteConfigObject.RemoteConfig;
        }

        private static async Task<RemoteConfigRoot> GetConfig<T>(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.timeout = 10;

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Request Error: " + www.error);
            }

            try
            {
                RemoteConfigRoot remoteConfigObject = new RemoteConfigRoot();
                string json = www.downloadHandler.text;
                Dictionary<string, object> root = MiniJson.Deserialize(json) as Dictionary<string, object>;
                remoteConfigObject.TestRemoteConfig = root["TestRemoteConfig"] as Dictionary<string, object>;
                remoteConfigObject.RemoteConfig = root["RemoteConfig"] as Dictionary<string, object>;
                return remoteConfigObject;
            }
            catch (Exception ex)
            {
                throw new Exception("JSON Parse Error: " + ex.Message);
            }
        }
        
        private class RemoteConfigRoot
        {
            public Dictionary<string, object> RemoteConfig;
            public Dictionary<string, object> TestRemoteConfig;
        }
    }
}