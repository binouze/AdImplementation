using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.binouze.Editor
{
    public class AdsPrebuildScript : IPreprocessBuildWithReport
    {
        public const string BASE_FOLDER = "Packages/com.binouze.adimplementation/Plugins/AdImplementation/";
        
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("AdsPrebuildScript.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
            // copier les fichiers necessaires avant la compilation
            CopyAssetsIntoProject();
        }
        
        [MenuItem( "Assets/AdImplementation/Copy Necesssary Files")]
        private static void CopyAssetsIntoProject()
        {
            if( !AssetDatabase.IsValidFolder("Assets/AdImplementation") )
            {
                AssetDatabase.CreateFolder("Assets", "AdImplementation");
            }
            
            if( !AssetDatabase.IsValidFolder("Assets/AdImplementation/Editor") )
            {
                AssetDatabase.CreateFolder("Assets/AdImplementation", "Editor");
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER) )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER} NOT FOUND");
                return;
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER + "AdImplementation") )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER}AdImplementation NOT FOUND");
                return;
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER + "Editor") )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER}Editor NOT FOUND");
                return;
            }
            
            // Copy SKAdNetworkItems.txt into project
            AssetDatabase.CopyAsset( BASE_FOLDER+"Editor/SKAdNetworkItems.txt", "Assets/AdImplementation/Editor/SKAdNetworkItems.txt" );
            
            
            #if UNITY_ANDROID
            AdsPreProcessAndroid.PreprocessAndroid();
            #endif
        }
    }
}