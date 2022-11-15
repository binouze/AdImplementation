# AdImplementation

Simple ad implementation using Admob mediation
 - Included Adapters:
   - UnityAds
   - AdColony
   - AppLovin
   - Liftoff (Vungle)

### USAGE:

```csharp

// -----------------------------------------------------------------------------
//                               Initialization
// -----------------------------------------------------------------------------


// DEBUGGING
//-----------
        
#if IS_DEBUG
    // set debug mode TRUE on debug builds (FALSE by default)
    AdImplementation.SetIsDebug( true );
    // enable logging on debug builds (FALSE BY DEFAULT)
    AdImplementation.SetLogEnabled( true );

    #if UNITY_IOS
        // set the test device ID for Google User Messaging Platform 
        AdImplementation.SetTestDeviceUMP( "xxx" );
        // set the test device ID for Google AdMob
        AdImplementation.SetTestDeviceAdMob( "xxx" );
    #elif UNITY_ANDROID
        // set the test device ID for Google User Messaging Platform 
        AdImplementation.SetTestDeviceUMP( "xxx" );
        // set the test device ID for Google AdMob
        AdImplementation.SetTestDeviceAdMob("xxx");
    #endif
#endif
     
     
// OPTIONNAL 
//-----------

// Optionaly Set if app is targeted for children (default FALSE)
AdImplementation.SetTargetChildrenType( TargetChildren.FALSE );
// Optionaly set a global handler for adOpen
AdImplementation.SetOnAdOpen( () => 
{
    Debug.Log( "AN AD HAS BEEN OPENED" );
});
// Optionaly set an handler for adClose
AdImplementation.SetOnAdClose( () => 
{
    Debug.Log( "AN AD HAS BEEN CLOSED" );
} );
// Optionaly set an handler for ImpressionData  
AdImplementation.SetImpressionDataHandler( data => 
{
    Debug.Log( $"IMPRESSION DATA RECEIVED {data}" );
});
// Optionnaly set a UserID for the current User
// used for server side validation of rewarded ads
AdImplementation.SetUserID( USER_ID );


// MANDATORY
//-----------

// Set the placement Ids
AdImplementation.SetUnitIds( adRewardId, adInterstitialId );
// Start the initialisation
AdImplementation.Initialize(); 


// -----------------------------------------------------------------------------
//                                    Usage
// -----------------------------------------------------------------------------

// Show a rewarded ad if available
if( AdImplementation.HasRewardedAvailable )
{
    AdImplementation.ShowRewarded( ok =>
    {
        // if ok == true the reward can be delivered
        var okstr = ok ? "OK" : "NOT OK";
        Debug.Log( $"REWARDED AD complete {okstr}" );
    } );
}

// Show an interstitial ad if available
if( AdImplementation.HasInterstitialAvailable )
{
    AdImplementation.ShowInterstitial( ok => 
    {
        // if ok == true the ad has been seen completely
        var okstr = ok ? "OK" : "NOT OK";
        Debug.Log( $"INTERSITIOAL AD complete {okstr}" );
    } );
}

```

### UPDATE NOTES FOR MYSELF:

```csharp

// Apres mise a jour du package GoogleAdMob:
//  - Verifier GoogleMobileAds/Editor/ManifestProcessor.cs et fixer les url du fichier manifest

private const string MANIFEST_RELATIVE_PATH = "Plugins/Android/AndroidManifest.xml";

```

### TODO:

 - handle multiple rewarded and interstitial placements 