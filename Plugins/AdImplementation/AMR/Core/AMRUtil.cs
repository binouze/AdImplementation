using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AMR
{
    public class AMRUtil
    {
        public static void Log(string message)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log("<AMRUnity> " + message);
            }
        }
        public static void LogException(string message)
        {
            if (Debug.isDebugBuild)
            {
                Debug.LogException(new Exception("<AMRUnity> " + message));
            }
        }

        public static string[] ArrayFromString(string paramsString)
        {
            string[] paramsArray = paramsString.Split(new string[] { "<>" }, StringSplitOptions.None);
            return paramsArray;
        }
        
        public static string DictionaryToJson(IDictionary<string, string> dict)
        {
            var entries = dict.Select(d =>
                string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }
        
        public static string BoolDictionaryToJson(IDictionary<string, bool> dict)
        {
            var entries = dict.Select(d =>
                string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }

        public static bool IsPlatformEditor()
        {
            return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor;
        }
    }
}