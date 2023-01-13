using System;
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

        public static string[] ArrayFromString(string paramsString)
        {
            string[] paramsArray = paramsString.Split(new string[] { "<>" }, StringSplitOptions.None);
            return paramsArray;
        }
    }
}
