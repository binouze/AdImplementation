using System.IO;
using com.binouze.Editor;
using UnityEditor;
using UnityEngine;

namespace com.binouze
{
    [CustomEditor( typeof(AdImplementationSettings))]
    public class AdImplementationSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _appIdAndroid;
        private SerializedProperty _appIdiOS;

        private SerializedProperty _AppLovinSDKKey;

        private SerializedProperty _admobIdAndroid;
        private SerializedProperty _admobIdiOS;

        public static AdImplementationSettings LoadSettingsInstance()
        {
            var instance = AdImplementationSettings.LoadInstance();
            // Create instance if null.
            if( instance == null )
            {
                Directory.CreateDirectory(AdImplementationSettings.AdImplementationSettingsResDir);
                instance = CreateInstance<AdImplementationSettings>();
                var assetPath = Path.Combine( AdImplementationSettings.AdImplementationSettingsResDir, AdImplementationSettings.AdImplementationSettingsFile + AdImplementationSettings.AdImplementationSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }
            return instance;
        }
        
        [MenuItem("LagoonPlugins/AdImplementation Settings")]
        public static void OpenInspector()
        {
            Selection.activeObject = LoadSettingsInstance();
        }

        public void OnEnable()
        {
            _appIdAndroid   = serializedObject.FindProperty("_AndroidAppId");
            _appIdiOS       = serializedObject.FindProperty("_IOSAppId");
            _AppLovinSDKKey = serializedObject.FindProperty("_AppLovinSDKKey");
            _admobIdAndroid = serializedObject.FindProperty("_AndroidAdmobId");
            _admobIdiOS     = serializedObject.FindProperty("_IOSAdmobId");
        }

        public override void OnInspectorGUI()
        {
            // Make sure the Settings object has all recent changes.
            serializedObject.Update();

            var settings = (AdImplementationSettings)target;

            if( settings == null )
            {
              Debug.LogError("AdImplementationSettings is null.");
              return;
            }

            // -- ADMOST
            
            EditorGUILayout.LabelField("Ad Most APP IDs", EditorStyles.boldLabel);
            //EditorGUILayout.HelpBox( "enter your AdMost applications ids here", MessageType.Info);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_appIdAndroid, new GUIContent("Android"));
            EditorGUILayout.PropertyField(_appIdiOS,     new GUIContent("iOS"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            
            // -- APP LOVIN
            
            EditorGUILayout.LabelField("App Lovin", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_AppLovinSDKKey, new GUIContent("SDK KEY"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            
            // -- ADMOB --

            EditorGUILayout.LabelField("Google AdMob App IDs", EditorStyles.boldLabel);
            //EditorGUILayout.HelpBox( "Google Mobile Ads App ID will look similar to this sample ID: ca-app-pub-3940256099942544~3347511713", MessageType.Info);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_admobIdAndroid, new GUIContent("Android"));
            EditorGUILayout.PropertyField(_admobIdiOS,     new GUIContent("iOS"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            
            if( GUILayout.Button("Run Pre-Build Script") )
            {
                AdsPrebuildScript.CopyAssetsIntoProject();
            }
            
            EditorGUILayout.Separator();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}