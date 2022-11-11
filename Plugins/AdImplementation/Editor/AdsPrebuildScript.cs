using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AdImplementation.Editor
{
    public class AdsPrebuildScript : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("AdsPrebuildScript.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
            
            // Copy link.xml into project
            AssetDatabase.CopyAsset( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/link.xml",
                                     "Assets/GoogleMobileAds/link.xml" );

            // Copy GoogleMobileAdsSKAdNetworkItems.xml into project
            AssetDatabase.CopyAsset( "Packages/com.binouze.adimplementation/Plugins/GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml",
                                     "Assets/GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml" );
        }
    }
}