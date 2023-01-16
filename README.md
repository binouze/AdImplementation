# AdImplementation

Simple ad implementation using AdMost
 - Included Adapters:
   - AdMob
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
#endif
     
     
// OPTIONNAL 
//-----------

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
// Optionaly set an handler for ad clicked event  
AdImplementation.SetOnAdClicked( network, rewarded => 
{
    Debug.Log( $"AD CLICKED {network} {rewarded}" );
});
// Optionnaly set a UserID for the current User
// used for server side validation of rewarded ads
AdImplementation.SetUserID( USER_ID );

// GDPR
//-----

// Set a function to call and handle results of the GDPR form
AdImplementation.SetGDPRFormFunction( ShowPopupGDPR );
private void ShowPopupGDPR( Action<bool> OnComplete )
{
    // show a popup that call OnComplete function with the response
}

// MANDATORY
//-----------

// Set the Ids
AdImplementation.SetIds( appID, adRewardId, adInterstitialId );
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