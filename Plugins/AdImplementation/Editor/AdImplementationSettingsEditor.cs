using UnityEditor;
using UnityEngine;

namespace com.binouze
{
    [InitializeOnLoad]
    [CustomEditor( typeof(AdImplementationSettings))]
    public class AdImplementationSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty _appIdAndroid;
        SerializedProperty _appIdiOS;
        
        SerializedProperty _AppLovinSDKKey;
        
        SerializedProperty _admobIdAndroid;
        SerializedProperty _admobIdiOS;
        
        [MenuItem("Assets/AdImplementation/Settings...")]
        public static void OpenInspector()
        {
            Selection.activeObject = AdImplementationSettings.LoadInstance();
        }

        public void OnEnable()
        {
            _appIdAndroid   = serializedObject.FindProperty("_AndroidAppId");
            _appIdiOS       = serializedObject.FindProperty("_IOSAppId");
            
            _AppLovinSDKKey = serializedObject.FindProperty("_AppLovinSDKKey");
            
            _admobIdAndroid   = serializedObject.FindProperty("_AndroidAdmobId");
            _admobIdiOS       = serializedObject.FindProperty("_IOSAdmobId");
        }

        public override void OnInspectorGUI()
        {
            // Make sure the Settings object has all recent changes.
            serializedObject.Update();

            var settings = (AdImplementationSettings)target;

            if(settings == null)
            {
              Debug.LogError("AdImplementationSettings is null.");
              return;
            }

            // -- ADMOST
            
            EditorGUILayout.LabelField("Ad Most APP IDs", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_appIdAndroid, new GUIContent("Android"));

            EditorGUILayout.PropertyField(_appIdiOS, new GUIContent("iOS"));

            EditorGUILayout.HelpBox(
                    "enter your AdMost applications ids here",
                    MessageType.Info);

            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();

            
            // -- APP LOVIN
            
            EditorGUILayout.LabelField("App Lovin", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_AppLovinSDKKey, new GUIContent("SDK KEY"));
            

            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();
            
            
            // -- ADMOB --

            EditorGUILayout.LabelField("Google AdMob App IDs", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_admobIdAndroid, new GUIContent("Android"));

            EditorGUILayout.PropertyField(_admobIdiOS, new GUIContent("iOS"));

            EditorGUILayout.HelpBox(
                "Google Mobile Ads App ID will look similar to this sample ID: ca-app-pub-3940256099942544~3347511713",
                MessageType.Info);

            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }
    }
}