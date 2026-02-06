using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AMR.iOS
{
    public class AMRBannerManager : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern IntPtr _loadBannerForZoneId(string zoneId, int position, int offset);
        
        [DllImport("__Internal")]
        private static extern IntPtr _loadBannerWithCoordinateForZoneId(string zoneId, double positionX, double positionY);

        [DllImport("__Internal")]
        private static extern void _showBanner();

        [DllImport("__Internal")]
        private static extern void _hideBanner();
        
        [DllImport("__Internal")]
        private static extern void _destroyBanner();
#endif
        
        #region Singleton

        private AMRBannerViewDelegate bannerDelegate = null;
        private static AMRBannerManager instance;
        public static AMRBannerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject("AMRBannerManager");
                    instance = obj.AddComponent<AMRBannerManager>();
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
        
        public void DidReceiveBanner(string stringParams)
        {
            AMRUtil.Log("Event: DidReceiveBanner, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName, ecpm
            if (parameters.Length >= 3)
            {
                bannerDelegate?.didReceiveBanner(parameters[1], Convert.ToDouble(parameters[2]));
            }
        }

        public void DidFailToReceiveBanner(string stringParams)
        {
            AMRUtil.Log("Event: DidFailToReceiveBanner, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, error
            if (parameters.Length >= 2)
            {
                bannerDelegate?.didFailtoReceiveBanner(parameters[1]);
            }
        }

        public void DidShowBanner(string stringParams)
        {
            AMRUtil.Log("Event: DidShowBanner, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, network, ecpm, adspaceId
            if (parameters.Length >= 4)
            {
                //bannerDelegate?.didShowBanner();

                double revenue = Convert.ToDouble(parameters[2]) / 1000.0 / 100.0;
                AMRAd ad = new AMRAd(parameters[0], parameters[1])
                {
                    Revenue = revenue,
                    AdSpaceId = parameters[3]
                };

                bannerDelegate?.didImpressionBanner(ad);
            }
        }
        
        public void DidClickBanner(string stringParams)
        {
            AMRUtil.Log("Event: DidClickBanner, Params: " + stringParams);
            var parameters = AMRUtil.ArrayFromString(stringParams);

            // zoneId, networkName
            if (parameters.Length >= 2)
            {
                bannerDelegate?.didClickBanner(parameters[1]);
            }
        }
        
        #endregion

        #region Public
        
        public static void LoadBanner(string zoneId, 
            AMR.Enums.AMRSDKBannerPosition position,
            int offset,
            AMRBannerViewDelegate delegateObject)
        {
#if UNITY_IOS
            Instance.UpdateDelegate(zoneId, delegateObject);
            _loadBannerForZoneId(zoneId, (int)position, offset);
#endif
        }
        
        public static void LoadBanner(string zoneId, 
            double positionX,
            double positionY,
            AMRBannerViewDelegate delegateObject)
        {
#if UNITY_IOS
            Instance.UpdateDelegate(zoneId, delegateObject);
            _loadBannerWithCoordinateForZoneId(zoneId, positionX, positionY);
#endif
        }

        public static void ShowBanner()
        {
#if UNITY_IOS
            _showBanner();
#endif
        }

        public static void HideBanner()
        {
#if UNITY_IOS
            _hideBanner();
#endif
        }
        
        public static void DestroyBanner()
        {
#if UNITY_IOS
            _destroyBanner();
#endif
        }

        #endregion

        #region Util

        private void UpdateDelegate(string zoneId, AMRBannerViewDelegate delegateObject)
        {
            bannerDelegate = delegateObject;
        }

        #endregion
    }

}

