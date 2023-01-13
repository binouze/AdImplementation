using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AMR.iOS
{
    public class AMRInterstitialManager : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern IntPtr _loadInterstitialForZoneId(string zoneId);

        [DllImport("__Internal")]
        private static extern void _showInterstitial(string zoneId, string tag);

        [DllImport("__Internal")]
        private static extern bool _isInterstitialReadyToShow(string zoneId);
#endif


        #region Singleton
        private Dictionary<string, AMRInterstitialViewDelegate> delegates = new Dictionary<string, AMRInterstitialViewDelegate>();
        private static AMRInterstitialManager instance;
        public static AMRInterstitialManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("AMRInterstitialManager");
                    instance = obj.AddComponent<AMRInterstitialManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Delegates

        public void DidReceiveInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidReceiveInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName, ecpm
            if (parameters.Length >= 3 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didReceiveInterstitial(parameters[1], Convert.ToDouble(parameters[2]));
            }
        }

        public void DidFailToReceiveInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidFailToReceiveInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, error
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didFailtoReceiveInterstitial(parameters[1]);
            }
        }

        public void DidShowInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidShowInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, network, ecpm, adspaceId
            if (parameters.Length >= 4 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didShowInterstitial();

                double revenue = Convert.ToDouble(parameters[2]) / 1000.0 / 100.0;
                AMRAd ad = new AMRAd(parameters[0], parameters[1])
                {
                    Revenue = revenue,
                    AdSpaceId = parameters[3]
                };

                delegates[parameters[0]].didImpressionInterstitial(ad);
            }
        }

        public void DidFailToShowInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidFailToShowInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, errorCode
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                if (parameters[1].Equals("1081") || parameters[1].Equals("1078"))
                {
                    delegates[parameters[0]].didFailtoShowInterstitial(parameters[1]);
                }
                else
                {
                    delegates[parameters[0]].didFailtoReceiveInterstitial(parameters[1]);
                }
            }
        }

        public void DidClickInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidClickInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didClickInterstitial(parameters[1]);
            }
        }

        public void DidDismissInterstitial(string stringParams)
        {
            AMRUtil.Log("Event: DidDismissInterstitial, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId
            if (parameters.Length >= 1 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didDismissInterstitial();
            }
        }

        public void DidInterstitialStatusChanged(string stringParams)
        {
            AMRUtil.Log("Event: DidInterstitialStatusChanged, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, state
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didStatusChangeInterstitial(Convert.ToInt32(parameters[1]));
            }
        }

        #endregion

        #region Public

        public static void LoadInterstitial(string zoneId, AMRInterstitialViewDelegate delegateObject)
        {
#if UNITY_IOS
            Instance.UpdateDelegate(zoneId, delegateObject);
            _loadInterstitialForZoneId(zoneId);
#endif
        }

        public static void ShowInterstitial(string zoneId, string tag)
        {
#if UNITY_IOS
            _showInterstitial(zoneId, tag);
#endif
        }

        public static bool isReadyToShow(string zoneId)
        {
            if (AMRSDK.initialized() == false)
            {
                return false;
            }

#if UNITY_IOS
            return _isInterstitialReadyToShow(zoneId);
#else
        return false;
#endif


        }

        #endregion

        #region Util

        private void UpdateDelegate(string zoneId, AMRInterstitialViewDelegate delegateObject)
        {
            if (delegates.ContainsKey(zoneId))
            {
                delegates[zoneId] = delegateObject;
            }
            else
            {
                delegates.Add(zoneId, delegateObject);
            }
        }

        #endregion
    }

}

