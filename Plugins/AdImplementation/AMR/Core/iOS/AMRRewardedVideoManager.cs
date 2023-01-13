using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AMR.iOS
{
    public class AMRRewardedVideoManager : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern IntPtr _loadRewardedVideoForZoneId(string zoneId);

        [DllImport("__Internal")]
        private static extern void _showRewardedVideo(string zoneId, string tag);

        [DllImport("__Internal")]
        private static extern bool _isRewardedVideoReadyToShow(string zoneId);
#endif


        #region Singleton
        private Dictionary<string, AMRRewardedVideoViewDelegate> delegates = new Dictionary<string, AMRRewardedVideoViewDelegate>();
        private static AMRRewardedVideoManager instance;
        public static AMRRewardedVideoManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("AMRRewardedVideoManager");
                    instance = obj.AddComponent<AMRRewardedVideoManager>();
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

        public void DidReceiveRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidReceiveRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName, ecpm
            if (parameters.Length >= 3 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didReceiveRewardedVideo(parameters[1], Convert.ToDouble(parameters[2]));
            }
        }

        public void DidFailToReceiveRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidFailToReceiveRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, error
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didFailtoReceiveRewardedVideo(parameters[1]);
            }
        }

        public void DidShowRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidShowRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, network, ecpm, adspaceId
            if (parameters.Length >= 4 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didShowRewardedVideo();

                double revenue = Convert.ToDouble(parameters[2]) / 1000.0 / 100.0;
                AMRAd ad = new AMRAd(parameters[0], parameters[1])
                {
                    Revenue = Convert.ToDouble(revenue),
                    AdSpaceId = parameters[3]
                };

                delegates[parameters[0]].didImpressionRewardedVideo(ad);
            }
        }

        public void DidFailToShowRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidFailToShowRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, errorCode
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                if (parameters[1].Equals("1081") || parameters[1].Equals("1078"))
                {
                    delegates[parameters[0]].didFailtoShowRewardedVideo(parameters[1]);
                }
                else
                {
                    delegates[parameters[0]].didFailtoReceiveRewardedVideo(parameters[1]);
                }
            }
        }

        public void DidClickRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidClickRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didClickRewardedVideo(parameters[1]);
            }
        }

        public void DidCompleteRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidCompleteRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId
            if (parameters.Length >= 1 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didCompleteRewardedVideo();
            }
        }

        public void DidDismissRewardedVideo(string stringParams)
        {
            AMRUtil.Log("Event: DidDismissRewardedVideo, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);
            
            // zoneId
            if (parameters.Length >= 1 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didDismissRewardedVideo();
            }
        }

        public void DidRewardedVideoStatusChanged(string stringParams)
        {
            AMRUtil.Log("Event: DidRewardedVideoStatusChanged, Params: " + stringParams);
            string[] parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, state
            if (parameters.Length >= 2 && delegates[parameters[0]] != null)
            {
                delegates[parameters[0]].didStatusChangeRewardedVideo(Convert.ToInt32(parameters[1]));
            }
        }

        #endregion

        #region Public

        public static void LoadRewardedVideo(string zoneId, AMRRewardedVideoViewDelegate delegateObject)
        {
#if UNITY_IOS
            Instance.UpdateDelegate(zoneId, delegateObject);
            _loadRewardedVideoForZoneId(zoneId);
#endif
        }

        public static void ShowRewardedVideo(string zoneId, string tag)
        {
#if UNITY_IOS
            _showRewardedVideo(zoneId, tag);
#endif
        }

        public static bool isReadyToShow(string zoneId)
        {
            if (AMRSDK.initialized() == false)
            {
                return false;
            }

#if UNITY_IOS
            return _isRewardedVideoReadyToShow(zoneId);
#else
        return false;
#endif


        }

        #endregion

        #region Util

        private void UpdateDelegate(string zoneId, AMRRewardedVideoViewDelegate delegateObject) {
            if (delegates.ContainsKey(zoneId))
            {
                delegates[zoneId] = delegateObject;
            } else
            {
                delegates.Add(zoneId, delegateObject);
            }
        }

        #endregion
    }

}

