#if UNITY_ANDROID
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace com.binouze
{
    public class AdsPreProcessAndroid
    {
        private const    string     MANIFEST_RELATIVE_PATH  = "Plugins/Android/AndroidManifest.xml";
        private const    string     METADATA_APPLICATION_ID = "com.google.android.gms.ads.APPLICATION_ID";
        private const    string     METADATA_APPLOVIN_KEY   = "applovin.sdk.key";
        private static readonly XNamespace ns               = "http://schemas.android.com/apk/res/android";

        public int callbackOrder => 1;

        public static void PreprocessAndroid()
        {
            var manifestPath = Path.Combine( Application.dataPath, MANIFEST_RELATIVE_PATH );
            
            var manifest        = XDocument.Load( manifestPath );
            var elemManifest    = manifest.Element( "manifest" );
            var elemApplication = elemManifest.Element( "application" );
            
            var instance = AdImplementationSettings.LoadInstance();

            var metas = elemApplication.Descendants().Where( elem => elem.Name.LocalName.Equals( "meta-data" ) );
            
            // -- ADMOB
            
            var adMobAppId = instance.AndroidAdmobId;
            if( !string.IsNullOrEmpty( adMobAppId ) )
            {
                SetMetadataElement( elemApplication, metas, METADATA_APPLICATION_ID, adMobAppId );
            }

            // -- APP LOVIN
            
            var appLovinSDKKey = instance.AppLovinSDKKey;
            if( !string.IsNullOrEmpty(appLovinSDKKey) )
            {
                SetMetadataElement( elemApplication, metas, METADATA_APPLOVIN_KEY, appLovinSDKKey );
            }

            /*SetMetadataElement( elemApplication,
                metas,
                METADATA_DELAY_APP_MEASUREMENT_INIT,
                instance.DelayAppMeasurementInit );

            SetMetadataElement( elemApplication,
                metas,
                METADATA_OPTIMIZE_INITIALIZATION,
                instance.OptimizeInitialization );

            SetMetadataElement( elemApplication,
                metas,
                METADATA_OPTIMIZE_AD_LOADING,
                instance.OptimizeAdLoading );*/

            elemManifest.Save( manifestPath );
        }

        private static XElement CreateMetaElement( string name, object value )
        {
            return new XElement( "meta-data",
                new XAttribute( ns + "name", name ), new XAttribute( ns + "value", value ) );
        }

        private static XElement GetMetaElement( IEnumerable<XElement> metas, string metaName )
        {
            foreach( var elem in metas )
            {
                var attrs = elem.Attributes();
                foreach( var attr in attrs )
                {
                    if( attr.Name.Namespace.Equals( ns )
                     && attr.Name.LocalName.Equals( "name" ) && attr.Value.Equals( metaName ) )
                    {
                        return elem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Utility for setting a metadata element
        /// </summary>
        /// <param name="elemApplication">application element</param>
        /// <param name="metas">all metadata elements</param>
        /// <param name="metadataName">name of the element to set</param>
        /// <param name="metadataValue">value to set</param>
        private static void SetMetadataElement( XElement              elemApplication,
                                                IEnumerable<XElement> metas,
                                                string                metadataName,
                                                string                metadataValue )
        {
            var element = GetMetaElement( metas, metadataName );
            if( element == null )
            {
                elemApplication.Add( CreateMetaElement( metadataName, metadataValue ) );
            }
            else
            {
                element.SetAttributeValue( ns + "value", metadataValue );
            }
        }

        /// <summary>
        /// Utility for setting a metadata element
        /// </summary>
        /// <param name="elemApplication">application element</param>
        /// <param name="metas">all metadata elements</param>
        /// <param name="metadataName">name of the element to set</param>
        /// <param name="metadataValue">value to set</param>
        /// <param name="defaultValue">If metadataValue is default, node will be removed.</param>
        private static void SetMetadataElement( XElement              elemApplication,
                                                IEnumerable<XElement> metas,
                                                string                metadataName,
                                                bool                  metadataValue,
                                                bool                  defaultValue = false )
        {
            var element = GetMetaElement( metas, metadataName );
            if( metadataValue != defaultValue )
            {
                if( element == null )
                {
                    elemApplication.Add( CreateMetaElement( metadataName, metadataValue ) );
                }
                else
                {
                    element.SetAttributeValue( ns + "value", metadataValue );
                }
            }
            else
            {
                element?.Remove();
            }
        }
    }
}
#endif