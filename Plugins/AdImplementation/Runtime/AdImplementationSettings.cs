using UnityEngine;

namespace com.binouze
{
    public class AdImplementationSettings : ScriptableObject
    {
        public const string AdImplementationSettingsFile          = "AdImplementationSettings";
        public const string AdImplementationSettingsResDir        = "Assets/AdImplementation/Resources";
        public const string AdImplementationSettingsFileExtension = ".asset";

        public static AdImplementationSettings LoadInstance()
        {
            // Read from resources.
            var instance = Resources.Load<AdImplementationSettings>(AdImplementationSettingsFile);
            
            // Create instance if null.
            /*if( instance == null )
            {
                Directory.CreateDirectory(AdImplementationSettingsResDir);
                instance = CreateInstance<AdImplementationSettings>();
                var assetPath = Path.Combine( AdImplementationSettingsResDir, AdImplementationSettingsFile + AdImplementationSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }*/

            return instance;
        }

        public bool IsValide()
        {
            return
                !string.IsNullOrEmpty( _AndroidAppId )   &&
                !string.IsNullOrEmpty( _IOSAppId )       &&
                !string.IsNullOrEmpty( _AppLovinSDKKey ) &&
                !string.IsNullOrEmpty( _AndroidAdmobId ) &&
                !string.IsNullOrEmpty( _IOSAdmobId );
        }
        
        [SerializeField]
        private string _AndroidAppId = string.Empty;

        [SerializeField]
        private string _IOSAppId = string.Empty;

        
        [SerializeField]
        private string _AppLovinSDKKey;

        
        [SerializeField]
        private string _AndroidAdmobId = string.Empty;

        [SerializeField]
        private string _IOSAdmobId = string.Empty;
        
        public string AndroidAppId
        {
            get => _AndroidAppId;
            set => _AndroidAppId = value;
        }

        public string IOSAppId
        {
            get => _IOSAppId;
            set => _IOSAppId = value;
        }
        
        public string AppLovinSDKKey
        {
            get => _AppLovinSDKKey;
            set => _AppLovinSDKKey = value;
        }
        
        
        public string AndroidAdmobId
        {
            get => _AndroidAdmobId;
            set => _AndroidAdmobId = value;
        }

        public string IOSAdmobId
        {
            get => _IOSAdmobId;
            set => _IOSAdmobId = value;
        }
    }
}