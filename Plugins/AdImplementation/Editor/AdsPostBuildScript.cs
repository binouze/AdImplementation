using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace com.binouze.Editor
{
    public class AdsPostBuildScript : MonoBehaviour, IPostprocessBuildWithReport
    {
        /// <summary>
        ///   <para>Returns the relative callback order for callbacks.  Callbacks with lower values are called before ones with higher values.</para>
        /// </summary>
        public int callbackOrder { get; } = 1;

        /// <summary>
        ///   <para>Implement this function to receive a callback after the build is complete.</para>
        /// </summary>
        /// <param name="report">A BuildReport containing information about the build, such as the target platform and output path.</param>
        public void OnPostprocessBuild( BuildReport report )
        {
            #if UNITY_IOS
            
            var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            var plist     = new PlistDocument();
            plist.ReadFromFile(plistPath);


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

            File.WriteAllText(plistPath, plist.WriteToString());
            #endif
        }

        #if UNITY_IOS
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
        #endif
    }
}