#if UNITY_IOS
using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace com.binouze
{
    public class AdsPostProcessIOS : IPostprocessBuildWithReport
    {
        private const string SKADNETWORKS_RELATIVE_PATH = "AdImplementation/Editor/SKAdNetworkItems.txt";
        
        /// <summary>
        ///   <para>Returns the relative callback order for callbacks.  Callbacks with lower values are called before ones with higher values.</para>
        /// </summary>
        public int callbackOrder { get; } = 1;
        
        public void OnPostprocessBuild( BuildReport report )
        {
            var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            var plist     = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var instance = AdImplementationSettings.LoadInstance();
            
            // AD MOB
            
            var admobId  = instance.IOSAdmobId;
            if( !string.IsNullOrEmpty( admobId ) )
            {
                plist.root.SetString("GADApplicationIdentifier", admobId);
            }
            
            // APP LOVIN
            
            var appLovinSDKKey = instance.AppLovinSDKKey;
            if( !string.IsNullOrEmpty( appLovinSDKKey ) )
            {
                plist.root.SetString("AppLovinSdkKey", appLovinSDKKey);
            }
            
            // AD COLONY
            
            // AdColony needs ATS configuration
            if( !plist.root.values.ContainsKey( "NSAppTransportSecurity" ) )
                plist.root.CreateDict( "NSAppTransportSecurity" );
            
            var NSAppTransportSecurityDic = plist.root["NSAppTransportSecurity"].AsDict();
            NSAppTransportSecurityDic.SetBoolean( "NSAllowsArbitraryLoads",             true );
            NSAppTransportSecurityDic.SetBoolean( "NSAllowsLocalNetworking",            true );
            NSAppTransportSecurityDic.SetBoolean( "NSAllowsArbitraryLoadsInWebContent", true );
            
            // AdColony needs these query scheme
            if( !plist.root.values.ContainsKey( "LSApplicationQueriesSchemes" ) )
                plist.root.CreateArray( "LSApplicationQueriesSchemes" );
            
            var LSApplicationQueriesSchemesArr = plist.root["LSApplicationQueriesSchemes"].AsArray();
            AddKeyIfNotExistsInPlistArray( LSApplicationQueriesSchemesArr, "fb" );
            AddKeyIfNotExistsInPlistArray( LSApplicationQueriesSchemesArr, "instagram" );
            AddKeyIfNotExistsInPlistArray( LSApplicationQueriesSchemesArr, "tumblr" );
            AddKeyIfNotExistsInPlistArray( LSApplicationQueriesSchemesArr, "twitter" );
            
            // AdColony needs Motion Sensor
            plist.root.SetString( "NSMotionUsageDescription", "Interactive ad controls" );

            // -- ADMOST NSExceptionDomains

            if( !NSAppTransportSecurityDic.values.TryGetValue( "NSExceptionDomains", out var NSExceptionDomainsElem ) )
                NSExceptionDomainsElem = NSAppTransportSecurityDic.CreateDict( "NSExceptionDomains" );
            var NSExceptionDomainsDic = NSExceptionDomainsElem.AsDict();
            
            if( !NSExceptionDomainsDic.values.TryGetValue( "admost.com", out var admostexeption ) )
                admostexeption = NSExceptionDomainsDic.CreateDict( "admost.com" );
            var admostexeptionDic = admostexeption.AsDict();
            
            admostexeptionDic.SetBoolean( "NSExceptionAllowsInsecureHTTPLoads", true );
            admostexeptionDic.SetBoolean( "NSIncludesSubdomains",               true );
            
            // - SKADNETWORK ITEMS
            
            // adding SKAdNetworkItems
            if( plist.root.values.ContainsKey( "SKAdNetworkItems" ) )
                plist.root.values.Remove( "SKAdNetworkItems" );
            
            
            var pathskad  = Path.Combine(Application.dataPath, SKADNETWORKS_RELATIVE_PATH);
            var plistskad = new PlistDocument();
            plistskad.ReadFromFile(pathskad);

            if( plistskad.root.values.TryGetValue( "SKAdNetworkItems", out var val_skad ) )
            {
                var skad_local = val_skad.AsArray();
                var skad_plist = plist.root.CreateArray( "SKAdNetworkItems" );
                foreach( var value in skad_local.values )
                {
                    var id_local = value.AsDict();
                    var id_plist = skad_plist.AddDict();
                    
                    foreach( var v in id_local.values )
                    {
                        id_plist.SetString( v.Key, v.Value.AsString() );
                    }
                }
            }
            
            File.WriteAllText(plistPath, plist.WriteToString());
            
            // Appending the SKADNETWORKS into the plist file
            //var path = Path.Combine(Application.dataPath, SKADNETWORKS_RELATIVE_PATH);
            //using Stream input  = File.OpenRead(path);
            //using Stream output = new FileStream(plistPath, FileMode.Append, FileAccess.Write, FileShare.None);
            //input.CopyTo(output); // Using .NET 4
        }
        
        private static void AddKeyIfNotExistsInPlistArray( PlistElementArray plistArray, string id )
        {
            if( !PlistElementArrayContainsString( plistArray, id ) )
                plistArray.AddString( id );
        }
        private static bool PlistElementArrayContainsString(PlistElementArray plistArray, string id)
        {
            foreach( var elem in plistArray.values )
            {
                try
                {
                    var val = elem.AsString();
                    if( val == id )
                        return true;
                }
                catch (Exception e)
                {
                    // Do nothing
                }
            }

            return false;
        }
    }
}
#endif