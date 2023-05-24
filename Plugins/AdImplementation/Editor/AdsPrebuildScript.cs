using System.IO;
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
            // creer les settings
            var settings = AdImplementationSettingsEditor.LoadSettingsInstance();
            if( settings == null || !settings.IsValide() )
            {
                Debug.LogWarning( "AdMost settings not valid please check AdIntegration/Settings..." );
            }
            
            // copier les fichiers necessaires avant la compilation
            CopyAssetsIntoProject();
        }
        
        [MenuItem( "LagoonPlugins/AdImplementation/Run Pre-Build Script")]
        public static void CopyAssetsIntoProject()
        {
            if( !AssetDatabase.IsValidFolder("Assets/LagoonPlugins") )
            {
                AssetDatabase.CreateFolder("Assets", "LagoonPlugins");
            }

            // en cas  de mise a jour du plugin : Moving fromm old to new localtion
            if( AssetDatabase.IsValidFolder( "Assets/AdImplementation" ) )
            {
                AssetDatabase.MoveAsset( "Assets/AdImplementation", "Assets/LagoonPlugins/AdImplementation" );
            }
            
            if( !AssetDatabase.IsValidFolder("Assets/LagoonPlugins/AdImplementation") )
            {
                AssetDatabase.CreateFolder("Assets/LagoonPlugins", "AdImplementation");
            }
            
            if( !AssetDatabase.IsValidFolder("Assets/LagoonPlugins/AdImplementation/Editor") )
            {
                AssetDatabase.CreateFolder("Assets/LagoonPlugins/AdImplementation", "Editor");
            }
            
            if( !AssetDatabase.IsValidFolder("Assets/LagoonPlugins/AdImplementation/Resources") )
            {
                AssetDatabase.CreateFolder("Assets/LagoonPlugins/AdImplementation", "Resources");
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER) )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER} NOT FOUND");
                return;
            }

            if( !AssetDatabase.IsValidFolder(BASE_FOLDER + "Editor") )
            {
                Debug.LogError($"FOLDER {BASE_FOLDER}Editor NOT FOUND");
                return;
            }
            
            // Copy SKAdNetworkItems.txt into project
            CopyAsset( BASE_FOLDER+"Editor/SKAdNetworkItems.txt", "Assets/LagoonPlugins/AdImplementation/Editor/SKAdNetworkItems.txt" );
            
            
            #if UNITY_ANDROID
            AdsPreProcessAndroid.PreprocessAndroid();
            #endif
        }

        /// <summary>
        /// Copier un asset unity
        ///  - AssetDatabase.CopyAsset change le GUID des fichier existant et c'est un peu genant, du coup cette fonction va verifier si le fichier cible existe, si il existe on va faire un File.Copy sinon on utilisera le AssetDatabase.CopyAsset
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static bool CopyAsset(string from, string to)
        {
            var fullPath = Path.Combine(Application.dataPath, "..", from);
            if (!File.Exists(fullPath))
            {
                Debug.LogError(from + ": file not found.");
                return false;
            }
   
            var fullNewPath = Path.Combine(Application.dataPath, "..", to);
            if (File.Exists(fullNewPath))
            {
                File.Copy(fullPath, fullNewPath, true);
                return true;
            }
            else
            {
                return AssetDatabase.CopyAsset(from, to);
            }
        }
    }
}