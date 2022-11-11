using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.binouze.Editor
{
    public class AdsPrebuildScript : MonoBehaviour, IPreprocessBuildWithReport
    {
        public const string BASE_FOLDER = "Packages/com.binouze.adimplementation/Plugins/AdImplementation/";
        
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("AdsPrebuildScript.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);

            // copier les fichiers necessaires avant compilation
            CopyAssetsIntoProject();
        }
        
        [MenuItem( "Assets/Google Mobile Ads/Copy Necesssary Files")]
        private static void CopyAssetsIntoProject()
        {
            if( !AssetDatabase.IsValidFolder("Assets/GoogleMobileAds") )
            {
                AssetDatabase.CreateFolder("Assets", "GoogleMobileAds");
            }
            
            if( !AssetDatabase.IsValidFolder("Assets/GoogleMobileAds/Editor") )
            {
                AssetDatabase.CreateFolder("Assets/GoogleMobileAds", "Editor");
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER) )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER} NOT FOUND");
                return;
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER + "GoogleMobileAds") )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER}GoogleMobileAds NOT FOUND");
                return;
            }
            // Copy link.xml into project
            AssetDatabase.CopyAsset( BASE_FOLDER+"GoogleMobileAds/link.xml",
                "Assets/GoogleMobileAds/link.xml" );

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER + "Editor") )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER}Editor NOT FOUND");
                return;
            }
            // Copy GoogleMobileAdsSKAdNetworkItems.xml into project
            AssetDatabase.CopyAsset( BASE_FOLDER+"Editor/GoogleMobileAdsSKAdNetworkItems.xml",
                "Assets/GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml" );
        }
    }
}