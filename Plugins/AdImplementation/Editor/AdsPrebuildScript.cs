using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.binouze.Editor
{
    public class AdsPrebuildScript : MonoBehaviour, IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("AdsPrebuildScript.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);

            // copier les fichiers necessaires avant compilation
            CopyAssetsIntoProject();
        }
        
        [MenuItem( "Assets/Google Mobile Ads/Copy Necesssary Files")]
        static void CopyAssetsIntoProject()
        {
            if( !AssetDatabase.IsValidFolder("Assets/GoogleMobileAds") )
            {
                AssetDatabase.CreateFolder("Assets", "GoogleMobileAds");
            }

            if( !AssetDatabase.IsValidFolder("Packages/com.binouze.adimplementation") )
            {
                Debug.LogError("PACKAGE com.binouze.adimplementation NOT INSTALLED");
                return;
            }

            if( !AssetDatabase.IsValidFolder( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds" ) )
            {
                Debug.LogError("FOLDER Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds DOES NOT EXISTS");
                return;
            }
            if( !AssetDatabase.IsValidFolder( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/Editor/" ) )
            {
                Debug.LogError("FOLDER Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/Editor/ DOES NOT EXISTS");
                return;
            }
            
            
            // Copy link.xml into project
            AssetDatabase.CopyAsset( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/link.xml",
                "Assets/GoogleMobileAds/link.xml" );

            // Copy GoogleMobileAdsSKAdNetworkItems.xml into project
            AssetDatabase.CopyAsset( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml",
                "Assets/GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml" );
        }
    }
}