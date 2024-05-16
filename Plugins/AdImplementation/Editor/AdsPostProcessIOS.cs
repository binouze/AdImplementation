#if UNITY_IOS || true
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace com.binouze
{
    public class AdsPostProcessIOS : IPostprocessBuildWithReport
    {
        private const string SKADNETWORKS_RELATIVE_PATH = "LagoonPlugins/AdImplementation/Editor/SKAdNetworkItems.txt";
        
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

            // Add default text for NSCalendarsUsageDescription
            if( !plist.root.values.ContainsKey( "NSCalendarsUsageDescription" ) ) 
                plist.root.SetString("NSCalendarsUsageDescription","Some ad content may access calendar.");

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

        
        // CAS SPECIAL POUR APPLOVIN QUI A MIS SON SDK EN DYNAMIC LINKING ET QUI CASSE TOUT EN STATIC LINKING

        private const string TargetUnityIphonePodfileLine    = "target 'Unity-iPhone' do";
        private const string UseFrameworksPodfileLine        = "use_frameworks!";
        private const string UseFrameworksDynamicPodfileLine = "use_frameworks! :linkage => :dynamic";
        private const string UseFrameworksStaticPodfileLine  = "use_frameworks! :linkage => :static";
        private const string AppLovinSDKFramework            = "AppLovinSDK.xcframework";
        
        [PostProcessBuild(int.MaxValue)] // on fait ca a la toute fin de la build
        private static void EndOfBuild(BuildTarget buildTarget, string buildPath)
        {
            // Check that the Pods directory exists (it might not if a publisher is building with Generate Podfile setting disabled in EDM).
            var podsDirectory = Path.Combine(buildPath, "Pods");
            if( !Directory.Exists(podsDirectory) || !ShouldEmbedDynamicLibraries( buildPath ) ) 
                return;
            
            Debug.Log( $"[AdImplementation] Embedding {AppLovinSDKFramework} to UnityMainTarget" );
            
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project     = new PBXProject();
            project.ReadFromFile(projectPath);
            
            var unityMainTargetGuid = project.GetUnityMainTargetGuid();
            
            // find the AppLovinSDK framework into Pods directory
            
            // both .framework and .xcframework are directories, not files
            var directories = Directory.GetDirectories(podsDirectory, AppLovinSDKFramework, SearchOption.AllDirectories);
            if( directories.Length <= 0 )
                return;

            var dynamicLibraryAbsolutePath       = directories[0];
            var index                            = dynamicLibraryAbsolutePath.LastIndexOf("Pods", StringComparison.Ordinal );
            var AppLovinSDKFrameworkRelativePath = dynamicLibraryAbsolutePath[index..];
            
            var fileGuid = project.AddFile(AppLovinSDKFrameworkRelativePath, AppLovinSDKFrameworkRelativePath);
            project.AddFileToEmbedFrameworks(unityMainTargetGuid, fileGuid);
        }

        /// <summary>
        /// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
        /// |         embed             |  use_frameworks! (:linkage => :dynamic)  |  use_frameworks! :linkage => :static  |  `use_frameworks!` line not present  |
        /// |---------------------------|------------------------------------------|---------------------------------------|--------------------------------------|
        /// | Unity-iPhone present      | Do not embed dynamic libraries           | Embed dynamic libraries               | Do not embed dynamic libraries       |
        /// | Unity-iPhone not present  | Embed dynamic libraries                  | Embed dynamic libraries               | Embed dynamic libraries              |
        /// |-----------------------------------------------------------------------------------------------------------------------------------------------------|
        /// </summary>
        /// <param name="buildPath">An iOS build path</param>
        /// <returns>Whether or not the dynamic libraries should be embedded.</returns>
        private static bool ShouldEmbedDynamicLibraries( string buildPath )
        {
            var podfilePath = Path.Combine( buildPath, "Podfile" );
            if( !File.Exists( podfilePath ) )
                return false;

            // If the Podfile doesn't have a `Unity-iPhone` target, we should embed the dynamic libraries.
            var lines                     = File.ReadAllLines( podfilePath );
            var containsUnityIphoneTarget = lines.Any( line => line.Contains( TargetUnityIphonePodfileLine ) );
            if( !containsUnityIphoneTarget )
                return true;

            // If the Podfile does not have a `use_frameworks! :linkage => static` line, we should not embed the dynamic libraries.
            var useFrameworksStaticLineIndex =
                Array.FindIndex( lines, line => line.Contains( UseFrameworksStaticPodfileLine ) );
            if( useFrameworksStaticLineIndex == -1 ) 
                return false;

            // If more than one of the `use_frameworks!` lines are present, CocoaPods will use the last one.
            var useFrameworksLineIndex =
                Array.FindIndex( lines, line => line.Trim() == UseFrameworksPodfileLine ); // Check for exact line to avoid matching `use_frameworks! :linkage => static/dynamic`
            var useFrameworksDynamicLineIndex =
                Array.FindIndex( lines, line => line.Contains( UseFrameworksDynamicPodfileLine ) );

            // Check if `use_frameworks! :linkage => :static` is the last line of the three. If it is, we should embed the dynamic libraries.
            return useFrameworksLineIndex        < useFrameworksStaticLineIndex &&
                   useFrameworksDynamicLineIndex < useFrameworksStaticLineIndex;
        }
    }
}
#endif