#import  <AMRSDK/AMRSDK.h>
#pragma mark - Helpers

typedef NS_ENUM(NSInteger, AMRBannerPosition) {
    AMRBannerPositionTop = 0,
    AMRBannerPositionBottom = 1,
    AMRBannerPositionCenter = 2
};

char* cStringCopy(const char* string) {
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

static NSString* CreateNSString(const char* string) {
    if (string != NULL)
        return [NSString stringWithUTF8String:string];
    else
        return [NSString stringWithUTF8String:""];
}

#pragma mark - Utilities

@interface AMRSDKPluginHelper : NSObject
+ (UIViewController *)topViewController;
@end

@implementation AMRSDKPluginHelper

+ (UIViewController *)topViewController {
    return [[[UIApplication sharedApplication] delegate] window].rootViewController;
}

+ (void)updateBannerView:(UIView *)bannerView forPosition:(AMRBannerPosition)position offset:(int)bannerBottomOffset {
    UIViewController *topVC = [AMRSDKPluginHelper topViewController];
    CGFloat bannerWidth = bannerView.frame.size.width;
    CGFloat bannerHeight = bannerView.frame.size.height;
    
    if (@available(iOS 11.0, *)) {
        [topVC.view addSubview:bannerView];
        bannerView.translatesAutoresizingMaskIntoConstraints = NO;
        
        NSMutableArray<NSLayoutConstraint*> *constraints = [NSMutableArray arrayWithArray:
                                                            @[
                                                              [bannerView.widthAnchor constraintEqualToConstant:bannerWidth],
                                                              [bannerView.heightAnchor constraintEqualToConstant:bannerHeight],
                                                              ]];
        
        if (position == AMRBannerPositionTop) {
            [constraints addObjectsFromArray:@[[bannerView.topAnchor constraintEqualToAnchor:bannerView.superview.safeAreaLayoutGuide.topAnchor],
                                               [bannerView.centerXAnchor constraintEqualToAnchor:bannerView.superview.safeAreaLayoutGuide.centerXAnchor]]];
            
        } else if (position == AMRBannerPositionCenter) {
            [constraints addObjectsFromArray:@[[bannerView.centerXAnchor constraintEqualToAnchor:bannerView.superview.safeAreaLayoutGuide.centerXAnchor],
                                               [bannerView.centerYAnchor constraintEqualToAnchor:bannerView.superview.safeAreaLayoutGuide.centerYAnchor]]];
            
        } else if (position == AMRBannerPositionBottom) {
            [constraints addObjectsFromArray:@[[bannerView.bottomAnchor constraintEqualToAnchor:(bannerView.superview.safeAreaLayoutGuide.bottomAnchor) constant:-bannerBottomOffset],
                                               [bannerView.centerXAnchor constraintEqualToAnchor:bannerView.superview.safeAreaLayoutGuide.centerXAnchor]]];
        }
        
        [NSLayoutConstraint activateConstraints:constraints];
    } else {
        CGRect bannerFrame = CGRectMake(0, 0, bannerWidth, bannerHeight);
        CGFloat screenWidth = [UIScreen mainScreen].bounds.size.width;
        CGFloat screenHeight = [UIScreen mainScreen].bounds.size.height;
        
        if (position == AMRBannerPositionTop) {
            bannerFrame.origin.x = .5 * (screenWidth - bannerFrame.size.width);
            bannerView.autoresizingMask = (UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleBottomMargin);
            
        } else if (position == AMRBannerPositionCenter) {
            bannerFrame.origin.x = .5 * (screenWidth - bannerFrame.size.width);
            bannerFrame.origin.y = .5 * (screenHeight - bannerFrame.size.height);
            bannerView.autoresizingMask = (UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleTopMargin | UIViewAutoresizingFlexibleBottomMargin);
            
        } else if (position == AMRBannerPositionBottom) {
            bannerFrame.origin.x = .5 * (screenWidth - bannerFrame.size.width);
            bannerFrame.origin.y = screenHeight - bannerFrame.size.height - bannerBottomOffset;
            bannerView.autoresizingMask = (UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleTopMargin);
        }
        
        bannerView.frame = bannerFrame;
        [topVC.view addSubview:bannerView];
    }
}

+ (void)updateBannerView:(UIView *)bannerView forPositionX:(double)positionX positionY:(double)positionY {
    UIViewController *topVC = [AMRSDKPluginHelper topViewController];
    CGFloat bannerWidth = bannerView.frame.size.width;
    CGFloat bannerHeight = bannerView.frame.size.height;
    CGFloat bannerX = positionX;
    CGFloat bannerY = [UIScreen mainScreen].bounds.size.height - positionY - bannerHeight;
    
    if (@available(iOS 11.0, *)) {
        [topVC.view addSubview:bannerView];
        bannerView.translatesAutoresizingMaskIntoConstraints = NO;
        
        NSMutableArray<NSLayoutConstraint*> *constraints = [NSMutableArray arrayWithArray:
                                                            @[
                                                              [bannerView.widthAnchor constraintEqualToConstant:bannerWidth],
                                                              [bannerView.heightAnchor constraintEqualToConstant:bannerHeight],
                                                              ]];
        
        [constraints addObjectsFromArray:@[
            [NSLayoutConstraint constraintWithItem:bannerView
                                         attribute:NSLayoutAttributeLeft
                                         relatedBy:NSLayoutRelationEqual
                                            toItem:topVC.view attribute:NSLayoutAttributeLeft
                                        multiplier:1.0
                                          constant:bannerX],
            [NSLayoutConstraint constraintWithItem:bannerView
                                         attribute:NSLayoutAttributeTop
                                         relatedBy:NSLayoutRelationEqual
                                            toItem:topVC.view
                                         attribute:NSLayoutAttributeTop
                                        multiplier:1.0f
                                          constant:bannerY]
        ]];
        
        [NSLayoutConstraint activateConstraints:constraints];
    } else {
        CGRect bannerFrame = CGRectMake(bannerX, bannerY, bannerWidth, bannerHeight);
        bannerView.autoresizingMask = (UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleTopMargin | UIViewAutoresizingFlexibleBottomMargin);
        bannerView.frame = bannerFrame;
        [topVC.view addSubview:bannerView];
    }
}

@end

@interface AMRSDKPlugin : NSObject
+ (void)startWithAppId:(NSString *)appId completion:(_Nullable AMRInitCompletionHandler)completion;
+ (void)startTestSuite:(NSString *)zoneIds;
+ (void)trackPurchase:(NSString *)identifier
         currencyCode:(NSString *)currencyCode
               amount:(double)amount;
+ (void)trackIAP:(NSString *)identifier
    currencyCode:(NSString *)currencyCode
          amount:(double)amount
            tags:(NSString *)tags;
+ (void)setUserId:(NSString *)userId;
+ (void)setAdjustUserId:(NSString *)adjustUserId;
+ (void)setClientCampaignId:(NSString *)campaignId;
+ (void)setUserConsent:(bool)consent;
+ (void)subjectToGDPR:(bool)subject;
+ (void)subjectToCCPA:(bool)subject;
+ (void)setUserChild:(bool)userChild;
+ (void)spendVirtualCurrency;
@end

@interface AMRInterstitialManager : NSObject <AMRInterstitialDelegate>
+ (void)loadInterstitialForZoneId:(NSString *)zoneId;
+ (void)showInterstitialForZoneId:(NSString *)zoneId tag:(NSString *)tag;
+ (BOOL)isReadyToShowForZoneId:(NSString *)zoneId;
@end

@interface AMRRewardedVideoManager : NSObject <AMRRewardedVideoDelegate>
+ (void)loadRewardedVideoForZoneId:(NSString *)zoneId;
+ (void)showRewardedVideoForZoneId:(NSString *)zoneId tag:(NSString *)tag;
+ (BOOL)isReadyToShowForZoneId:(NSString *)zoneId;
@end

@interface BannerDelegateWrapper : NSObject <AMRBannerDelegate> @end
@interface OfferWallDelegateWrapper : NSObject <AMROfferWallDelegate> @end
@interface VirtualCurrencyDelegateWrapper : NSObject <AMRVirtualCurrencyDelegate> @end
@interface TrackPurchaseResponseDelegateWrapper : NSObject <AMRTrackPurchaseResponseDelegate> @end

typedef void (* SDKDidInitializeCallback)(int didInitializeRefPtr, bool isInitialized, const char* errorMessage);

// banner
typedef void (* BannerSuccessCallback)(int bannerRefPtr, const char* networkName, double ecpm);
typedef void (* BannerFailCallback)(int bannerRefPtr, const char* errorMessage);
typedef void (* BannerShowCallback)(int bannerRefPtr, const char* zoneId, const char* networkName, double ecpm, const char* adSpaceId);
typedef void (* BannerClickCallback)(int bannerRefPtr, const char* networkName);

// offerWall
typedef void (* OfferWallSuccessCallback)(int offerWallRefPtr, const char* networkName, double ecpm);
typedef void (* OfferWallFailCallback)(int offerWallRefPtr, const char* errorMessage);
typedef void (* OfferWallDismissCallback)(int offerWallRefPtr);

// virtual currency
typedef void (* VirtualCurrencyDidSpendCallback)(int virtualCurrencyRefPtr, const char* networkName, const char* currency, double amount);

// track purchase
typedef void (* TrackPurchaseResponseCallback)(int trackPurchaseRefPtr, const char* uniqueID, int status);

// privacy
typedef void (* IsGDPRApplicableCallback)(int IsGDPRApplicableRefPtr, bool status);
typedef void (* PrivacyConsentRequiredCallback)(int PrivacyConsentRequiredRefPtr, int consentStatus);

static SDKDidInitializeCallback sdkDidInitializeCallback;
static int sdkDidInitializeHandle;

// banner
typedef const void *AMRBannerRef;
static int bannerFillCount;
static AMRBanner *banner;
static BannerDelegateWrapper *bannerDelegate;
static BannerSuccessCallback bannerSuccessCallback;
static BannerFailCallback bannerFailCallback;
static BannerShowCallback bannerShowCallback;
static BannerClickCallback bannerClickCallback;
static int bannerHandle;
static int bannerOffset;
static double bannerPosX;
static double bannerPosY;
static int useCoordinates;
static AMRBannerPosition position;

// offerWall
typedef const void *AMROfferWallRef;
static AMROfferWall *offerWall;
static OfferWallDelegateWrapper *offerWallDelegate;
static OfferWallSuccessCallback offerWallSuccessCallback;
static OfferWallFailCallback offerWallFailCallback;
static OfferWallDismissCallback offerWallDismissCallback;
static int offerWallHandle;

// virtualCurrency
static VirtualCurrencyDelegateWrapper *virtualCurrencyDelegate;
static VirtualCurrencyDidSpendCallback virtualCurrencyDidSpendCallback;
static int virtualCurrencyHandle;

// track purchase
static TrackPurchaseResponseDelegateWrapper *trackPurchaseResponseDelegate;
static TrackPurchaseResponseCallback trackPurchaseResponseCallback;
static int trackPurchaseHandle;

@implementation AMRSDKPlugin

+ (void)startWithAppId:(NSString *)appId completion:(_Nullable AMRInitCompletionHandler)completion {
    [AMRSDK setLogLevel:AMRLogLevelSilent];
    
    [AMRSDK startWithAppId:appId completion:^(AMRError * _Nullable error) {
        completion(error);
    }];
}

+ (void)startTestSuite:(NSString *)zoneIds {
    NSArray *zoneList = [zoneIds componentsSeparatedByString: @","];
    
    [AMRSDK startTestSuiteWithZones:zoneList];
}

+ (void)trackPurchase:(NSString *)identifier
         currencyCode:(NSString *)currencyCode
               amount:(double)amount {
    
    [AMRSDK trackPurchase:identifier
             currencyCode:currencyCode
                   amount:amount];
}

+ (void)trackIAP:(NSString *)identifier
    currencyCode:(NSString *)currencyCode
          amount:(double)amount
            tags:(NSString *)tags {
    NSArray *tagArray = [tags componentsSeparatedByString: @","];
    
    [AMRSDK trackIAP:identifier
        currencyCode:currencyCode
              amount:amount
                tags:tagArray];
}

+ (void)setUserId:(NSString *)userId {
    [AMRSDK setUserId:userId];
}

+ (void)setAdjustUserId:(NSString *)adjustUserId {}

+ (void)setClientCampaignId:(NSString *)campaignId {
    [AMRSDK setClientCampaignId:campaignId];
}

+ (void)setUserConsent:(bool)consent {
    [AMRSDK setUserConsent:consent];
}

+ (void)subjectToGDPR:(bool)subject {
    [AMRSDK subjectToGDPR:subject];
}

+ (void)subjectToCCPA:(bool)subject {
    [AMRSDK subjectToCCPA:subject];
}

+ (void)setUserChild:(bool)userChild {
    [AMRSDK setUserChild:userChild];
}

+ (void)spendVirtualCurrency {
    [AMRSDK spendVirtualCurrency];
}

@end

@implementation AMRInterstitialManager {
    NSMutableDictionary<NSString*, AMRInterstitial*> *_interstitials;
}

#pragma mark - NSObject

+ (instancetype)sharedInstance {
    static AMRInterstitialManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[AMRInterstitialManager alloc] init];
    });
    return sharedInstance;
}

- (instancetype)init {
    if (!(self = [super init]))
        return nil;
    
    _interstitials = [NSMutableDictionary new];
    
    return self;
}

#pragma mark - Public

+ (void)loadInterstitialForZoneId:(NSString *)zoneId {
    [[AMRInterstitialManager sharedInstance] loadInterstitialForZoneId:zoneId];
}

+ (void)showInterstitialForZoneId:(NSString *)zoneId tag:(NSString *)tag {
    [[AMRInterstitialManager sharedInstance] showInterstitialForZoneId:zoneId tag:tag];
}

+ (BOOL)isReadyToShowForZoneId:(NSString *)zoneId {
    return [[AMRInterstitialManager sharedInstance] isReadyToShowForZoneId:zoneId];
}

#pragma mark - Private

- (void)loadInterstitialForZoneId:(NSString *)zoneId {
    AMRInterstitial *interstitial = [self interstitialForZoneId:zoneId];
    if (interstitial) {
        [interstitial loadInterstitial];
    }
}

- (void)showInterstitialForZoneId:(NSString *)zoneId tag:(NSString *)tag {
    AMRInterstitial *interstitial = [self interstitialForZoneId:zoneId];
    if (interstitial) {
        [interstitial showFromViewController:[AMRSDKPluginHelper topViewController] withTag:tag];
    }
}

- (BOOL)isReadyToShowForZoneId:(NSString *)zoneId {
    AMRInterstitial *interstitial = [self interstitialForZoneId:zoneId];
    if (interstitial && [interstitial respondsToSelector:@selector(isReadyToShow)]) {
        bool isReadyToShow;

        Class interstitialClass = [interstitial class];
        
        NSMethodSignature *methodSignature = [interstitialClass instanceMethodSignatureForSelector:NSSelectorFromString(@"isReadyToShow")];
        NSInvocation *invocation = [NSInvocation invocationWithMethodSignature:methodSignature];
        
        [invocation setTarget:interstitial];
        [invocation setSelector:NSSelectorFromString(@"isReadyToShow")];

        [invocation invoke];
        [invocation getReturnValue:&isReadyToShow];
        
        return isReadyToShow;
        
        return [[interstitial class] instancesRespondToSelector:@selector(isReadyToShow)];
    }
    
    return false;
}

#pragma mark - Util

- (AMRInterstitial *)interstitialForZoneId:(NSString *)zoneId {
    if (!zoneId || [zoneId isEqualToString:@""]) {
        NSLog(@"<AMRUnity> Invalid zoneid");
        return nil;
    }
    
    AMRInterstitial *interstitial = [_interstitials objectForKey:zoneId];
    
    if (!interstitial) {
        interstitial = [AMRInterstitial interstitialForZoneId:zoneId];
        interstitial.delegate = self;
        [_interstitials setObject:interstitial forKey:zoneId];
    }
    
    return interstitial;
}

- (void)sendUnityEvent:(NSString *)event params:(NSArray *)params {
    NSString *paramsString = [params componentsJoinedByString:@"<>"];
    UnitySendMessage("AMRInterstitialManager", event.UTF8String, paramsString.UTF8String);
}

#pragma mark - <AMRInterstitialDelegate>

- (void)didReceiveInterstitial:(AMRInterstitial *)interstitial {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidReceiveInterstitial" params:@[interstitial.zoneId, interstitial.networkName, interstitial.ecpm]];
    });
}

- (void)didFailToReceiveInterstitial:(AMRInterstitial *)interstitial error:(AMRError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidFailToReceiveInterstitial" params:@[interstitial.zoneId, error.errorDescription]];
    });
}

- (void)didShowInterstitial:(AMRInterstitial *)interstitial {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidShowInterstitial" params:@[interstitial.zoneId, interstitial.networkName, interstitial.ecpm, interstitial.adSpaceId]];
    });
}

- (void)didFailToShowInterstitial:(AMRInterstitial *)interstitial error:(AMRError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidFailToShowInterstitial" params:@[interstitial.zoneId, [@(error.errorCode) stringValue]]];
    });
}

- (void)didClickInterstitial:(AMRInterstitial *)interstitial {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidClickInterstitial" params:@[interstitial.zoneId, interstitial.networkName]];
    });
}

- (void)didDismissInterstitial:(AMRInterstitial *)interstitial {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidDismissInterstitial" params:@[interstitial.zoneId]];
    });
}

- (void)didInterstitialStateChanged:(AMRInterstitial *)interstitial state:(AMRAdState)state {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidInterstitialStatusChanged" params:@[interstitial.zoneId, @(state)]];
    });
}

@end

@implementation AMRRewardedVideoManager {
    NSMutableDictionary<NSString*, AMRRewardedVideo*> *_rewardedVideos;
}

#pragma mark - NSObject

+ (instancetype)sharedInstance {
    static AMRRewardedVideoManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[AMRRewardedVideoManager alloc] init];
    });
    return sharedInstance;
}

- (instancetype)init {
    if (!(self = [super init]))
        return nil;
    
    _rewardedVideos = [NSMutableDictionary new];
    
    return self;
}

#pragma mark - Public

+ (void)loadRewardedVideoForZoneId:(NSString *)zoneId {
    [[AMRRewardedVideoManager sharedInstance] loadRewardedVideoForZoneId:zoneId];
}

+ (void)showRewardedVideoForZoneId:(NSString *)zoneId tag:(NSString *)tag {
    [[AMRRewardedVideoManager sharedInstance] showRewardedVideoForZoneId:zoneId tag:tag];
}

+ (BOOL)isReadyToShowForZoneId:(NSString *)zoneId {
    return [[AMRRewardedVideoManager sharedInstance] isReadyToShowForZoneId:zoneId];
}

#pragma mark - Private

- (void)loadRewardedVideoForZoneId:(NSString *)zoneId {
    AMRRewardedVideo *rw = [self rewardedVideoForZoneId:zoneId];
    if (rw) {
        [rw loadRewardedVideo];
    }
}

- (void)showRewardedVideoForZoneId:(NSString *)zoneId tag:(NSString *)tag {
    AMRRewardedVideo *rw = [self rewardedVideoForZoneId:zoneId];
    if (rw) {
        [rw showFromViewController:[AMRSDKPluginHelper topViewController] withTag:tag];
    }
}

- (BOOL)isReadyToShowForZoneId:(NSString *)zoneId {
    AMRRewardedVideo *rw = [self rewardedVideoForZoneId:zoneId];
    if (rw && [rw respondsToSelector:@selector(isReadyToShow)]) {
        bool isReadyToShow;

        Class rewardedClass = [rw class];
        
        NSMethodSignature *methodSignature = [rewardedClass instanceMethodSignatureForSelector:NSSelectorFromString(@"isReadyToShow")];
        NSInvocation *invocation = [NSInvocation invocationWithMethodSignature:methodSignature];
        
        [invocation setTarget:rw];
        [invocation setSelector:NSSelectorFromString(@"isReadyToShow")];

        [invocation invoke];
        [invocation getReturnValue:&isReadyToShow];
        
        return isReadyToShow;
        
        return [[rw class] instancesRespondToSelector:@selector(isReadyToShow)];
    }
    
    return false;
}

#pragma mark - Util

- (AMRRewardedVideo *)rewardedVideoForZoneId:(NSString *)zoneId {
    if (!zoneId || [zoneId isEqualToString:@""]) {
        NSLog(@"<AMRUnity> Invalid zoneid");
        return nil;
    }
    
    AMRRewardedVideo *rw = [_rewardedVideos objectForKey:zoneId];
    
    if (!rw) {
        rw = [AMRRewardedVideo rewardedVideoForZoneId:zoneId];
        rw.delegate = self;
        [_rewardedVideos setObject:rw forKey:zoneId];
    }
    
    return rw;
}

- (void)sendUnityEvent:(NSString *)event params:(NSArray *)params {
    NSString *paramsString = [params componentsJoinedByString:@"<>"];
    UnitySendMessage("AMRRewardedVideoManager", event.UTF8String, paramsString.UTF8String);
}

#pragma mark - <AMRRewardedVideoDelegate>

- (void)didReceiveRewardedVideo:(AMRRewardedVideo *)rewardedVideo {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidReceiveRewardedVideo" params:@[rewardedVideo.zoneId, rewardedVideo.networkName, rewardedVideo.ecpm]];
    });
}

- (void)didFailToReceiveRewardedVideo:(AMRRewardedVideo *)rewardedVideo error:(AMRError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidFailToReceiveRewardedVideo" params:@[rewardedVideo.zoneId, error.errorDescription]];
    });
}

- (void)didShowRewardedVideo:(AMRRewardedVideo *)rewardedVideo {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidShowRewardedVideo" params:@[rewardedVideo.zoneId, rewardedVideo.networkName, rewardedVideo.ecpm, rewardedVideo.adSpaceId]];
    });
}

- (void)didFailToShowRewardedVideo:(AMRRewardedVideo *)rewardedVideo error:(AMRError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidFailToShowRewardedVideo" params:@[rewardedVideo.zoneId, [@(error.errorCode) stringValue]]];
    });
}

- (void)didClickRewardedVideo:(AMRRewardedVideo *)rewardedVideo {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidClickRewardedVideo" params:@[rewardedVideo.zoneId, rewardedVideo.networkName]];
    });
}

- (void)didCompleteRewardedVideo:(AMRRewardedVideo *)rewardedVideo {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidCompleteRewardedVideo" params:@[rewardedVideo.zoneId]];
    });
}

- (void)didDismissRewardedVideo:(AMRRewardedVideo *)rewardedVideo {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidDismissRewardedVideo" params:@[rewardedVideo.zoneId]];
    });
}

- (void)didRewardedVideoStateChanged:(AMRRewardedVideo *)rewardedVideo state:(AMRAdState)state {
    dispatch_async(dispatch_get_main_queue(), ^{
        [self sendUnityEvent:@"DidRewardedVideoStatusChanged" params:@[rewardedVideo.zoneId, @(state)]];
    });
}

@end

@implementation BannerDelegateWrapper

- (void)didReceiveBanner:(AMRBanner *)banner {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (bannerSuccessCallback) {
            bannerSuccessCallback(bannerHandle, [banner.networkName UTF8String], [banner.ecpm doubleValue]);
        }
        
//        if (bannerFillCount > 0) {
//            if (useCoordinates == 1) {
//                [AMRSDKPluginHelper updateBannerView:banner.bannerView forPositionX:bannerPosX positionY:bannerPosY];
//            } else {
//                [AMRSDKPluginHelper updateBannerView:banner.bannerView forPosition:position offset:bannerOffset];
//            }
//        }
        
        bannerFillCount += 1;
    });
}

- (void)didFailToReceiveBanner:(AMRBanner *)banner error:(AMRError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (bannerFailCallback) {
            bannerFailCallback(bannerHandle, [error.errorDescription UTF8String]);
        }
    });
}

- (void)didShowBanner:(AMRBanner *)banner {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (bannerShowCallback) {
            bannerShowCallback(bannerHandle, [banner.zoneId UTF8String], [banner.networkName UTF8String], [banner.ecpm doubleValue], [banner.adSpaceId UTF8String]);
        }
    });
}

- (void)didClickBanner:(AMRBanner *)banner {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (bannerClickCallback) {
            bannerClickCallback(bannerHandle, [banner.networkName UTF8String]);
        }
    });
}

@end

@implementation OfferWallDelegateWrapper

- (void)didReceiveOfferWall:(AMROfferWall *)offerWall {
    if (offerWallSuccessCallback) {
        offerWallSuccessCallback(offerWallHandle, [offerWall.networkName UTF8String], [offerWall.ecpm doubleValue]);
    }
}

- (void)didFailToReceiveOfferWall:(AMROfferWall *)offerWall error:(AMRError *)error {
    if (offerWallFailCallback) {
        offerWallFailCallback(offerWallHandle, [error.errorDescription UTF8String]);
    }
}

- (void)didDismissOfferWall:(AMROfferWall *)offerWall {
    if (offerWallDismissCallback) {
        offerWallDismissCallback(offerWallHandle);
    }
}

@end

@implementation VirtualCurrencyDelegateWrapper

- (void)didSpendVirtualCurrency:(NSString *)currency
                         amount:(NSNumber *)amount
                    networkName:(NSString *)networkName {
    if (virtualCurrencyDidSpendCallback) {
        virtualCurrencyDidSpendCallback(virtualCurrencyHandle, [networkName UTF8String], [currency UTF8String], [amount doubleValue]);
    }
}

@end

@implementation TrackPurchaseResponseDelegateWrapper

- (void)trackPurchaseResponse:(NSString *)identifier status:(AMRTrackPurchaseResponseStatus)status {
    if (trackPurchaseResponseCallback) {
        trackPurchaseResponseCallback(trackPurchaseHandle, [identifier UTF8String], (int)status);
    }
}

@end

extern "C"
{
#pragma mark - SDK
    void _startWithAppId(const char* appId) {
        [AMRSDKPlugin startWithAppId:CreateNSString(appId) completion:^(AMRError * _Nullable error) {
            if (sdkDidInitializeCallback) {
                sdkDidInitializeCallback(sdkDidInitializeHandle, error == nil, "");
            }
        }];
    }
    
    void _startTestSuite(const char* zoneIds) {
        [AMRSDKPlugin startTestSuite:CreateNSString(zoneIds)];
    }
    
    void _trackPurchase(const char* identifier,
                        const char* currencyCode,
                        double amount) {
        [AMRSDKPlugin trackPurchase:CreateNSString(identifier)
                       currencyCode:CreateNSString(currencyCode)
                             amount:amount];
    }

    void _trackIAP(const char* identifier,
                   const char* currencyCode,
                   double amount,
                   const char* tags) {
        [AMRSDKPlugin trackIAP:CreateNSString(identifier)
                  currencyCode:CreateNSString(currencyCode)
                        amount:amount
                          tags:CreateNSString(tags)];
    }
    
    void _setUserId(const char* userId) {
        [AMRSDKPlugin setUserId:CreateNSString(userId)];
    }
    
    void _setAdjustUserId(const char* adjustUserId) {
        [AMRSDKPlugin setAdjustUserId:CreateNSString(adjustUserId)];
    }
    
    void _setClientCampaignId(const char* campaignId) {
        [AMRSDKPlugin setClientCampaignId:CreateNSString(campaignId)];
    }
    
    void _setUserConsent(bool consent) {
        [AMRSDKPlugin setUserConsent:consent];
    }
    
    void _subjectToGDPR(bool subject) {
        [AMRSDKPlugin subjectToGDPR:subject];
    }

    void _subjectToCCPA(bool subject) {
        [AMRSDKPlugin subjectToCCPA:subject];
    }

    void _setUserChild(bool userChild) {
        [AMRSDKPlugin setUserChild:userChild];
    }
    
    void _spendVirtualCurrency() {
        [AMRSDKPlugin spendVirtualCurrency];
    }
    
#pragma mark - BANNER
    
    void _setBannerSuccessCallback(BannerSuccessCallback cb) {
        bannerSuccessCallback = cb;
    }
    
    void _setBannerFailCallback(BannerFailCallback cb) {
        bannerFailCallback = cb;
    }

    void _setBannerShowCallback(BannerShowCallback cb) {
        bannerShowCallback = cb;
    }
    
    void _setBannerClickCallback(BannerClickCallback cb) {
        bannerClickCallback = cb;
    }
    
    AMRBannerRef _loadBannerForZoneId(const char* zoneId,
                                      AMRBannerPosition pos,
                                      int offset,
                                      int x) {
        bannerHandle = x;
        useCoordinates = 0;
        position = pos;
        bannerOffset = offset;
        banner = [AMRBanner bannerForZoneId:CreateNSString(zoneId)];
        bannerDelegate = [BannerDelegateWrapper new];
        banner.delegate = bannerDelegate;
        [banner loadBanner];
        bannerFillCount = 0;
        return (__bridge AMRBannerRef)banner;
    }

    AMRBannerRef _loadBannerForZoneIdWithPosition(const char* zoneId,
                                                 double positionX,
                                                 double positionY,
                                                 int x) {
        bannerHandle = x;
        bannerPosX = positionX;
        bannerPosY = positionY;
        useCoordinates = 1;
        banner = [AMRBanner bannerForZoneId:CreateNSString(zoneId)];
        bannerDelegate = [BannerDelegateWrapper new];
        banner.delegate = bannerDelegate;
        [banner loadBanner];
        bannerFillCount = 0;
        return (__bridge AMRBannerRef)banner;
    }
    
    void _showBanner(AMRBannerRef bannerRef) {
        AMRBanner *internalBanner = (__bridge AMRBanner*)bannerRef;
        if (useCoordinates == 1) {
            [AMRSDKPluginHelper updateBannerView:internalBanner.bannerView forPositionX:bannerPosX positionY:bannerPosY];
        } else {
            [AMRSDKPluginHelper updateBannerView:internalBanner.bannerView forPosition:position offset:bannerOffset];
        }
    }
    
    void _hideBanner(AMRBannerRef bannerRef) {
        AMRBanner *internalBanner = (__bridge AMRBanner*)bannerRef;
        [internalBanner.bannerView removeFromSuperview];
    }

    void _destroyBanner(AMRBannerRef bannerRef) {
        AMRBanner *internalBanner = (__bridge AMRBanner*)bannerRef;
        [internalBanner.bannerView removeFromSuperview];
        internalBanner.delegate = nil;
        internalBanner = nil;
    }
    
#pragma mark - INTERSTITIAL
    
    void _loadInterstitialForZoneId(const char* zoneId) {
        [AMRInterstitialManager loadInterstitialForZoneId:CreateNSString(zoneId)];
    }

    void _showInterstitial(const char* zoneId, const char* tag) {
        [AMRInterstitialManager showInterstitialForZoneId:CreateNSString(zoneId) tag:CreateNSString(tag)];
    }

    bool _isInterstitialReadyToShow(const char* zoneId) {
        return [AMRInterstitialManager isReadyToShowForZoneId:CreateNSString(zoneId)];
    }
    
#pragma mark - REWARDED VIDEO
    
    void _loadRewardedVideoForZoneId(const char* zoneId) {
        [AMRRewardedVideoManager loadRewardedVideoForZoneId:CreateNSString(zoneId)];
    }
    
    void _showRewardedVideo(const char* zoneId, const char* tag) {
        [AMRRewardedVideoManager showRewardedVideoForZoneId:CreateNSString(zoneId) tag:CreateNSString(tag)];
    }

    bool _isRewardedVideoReadyToShow(const char* zoneId) {
        return [AMRRewardedVideoManager isReadyToShowForZoneId:CreateNSString(zoneId)];
    }
    
#pragma mark - OFFERWALL
    
    void _setOfferWallSuccessCallback(OfferWallSuccessCallback cb) {
        offerWallSuccessCallback = cb;
    }
    
    void _setOfferWallFailCallback(OfferWallFailCallback cb) {
        offerWallFailCallback = cb;
    }
    
    void _setOfferWallDismissCallback(OfferWallDismissCallback cb) {
        offerWallDismissCallback = cb;
    }
    
    AMROfferWallRef _loadOfferWallForZoneId(const char* zoneId, int x) {
        offerWallHandle = x;
        offerWall = [AMROfferWall offerWallForZoneId:CreateNSString(zoneId)];
        offerWallDelegate = [OfferWallDelegateWrapper new];
        offerWall.delegate = offerWallDelegate;
        [offerWall loadOfferWall];
        return (__bridge AMROfferWallRef)offerWall;
    }
    
    void _showOfferWall(AMROfferWallRef offerWallRef) {
        AMROfferWall *internalOfferWall = (__bridge AMROfferWall*)offerWallRef;
        [internalOfferWall showFromViewController:[AMRSDKPluginHelper topViewController]];
    }
    
    void _showOfferWallWithTag(const char* tag, AMROfferWallRef offerWallRef) {
        AMROfferWall *internalOfferWall = (__bridge AMROfferWall*)offerWallRef;
        [internalOfferWall showFromViewController:[AMRSDKPluginHelper topViewController] withTag:CreateNSString(tag)];
    }
    
#pragma mark - Virtual Currency
    
    void _setVirtualCurrencyDidSpendCallback(VirtualCurrencyDidSpendCallback cb, int x) {
        virtualCurrencyDidSpendCallback = cb;
        
        virtualCurrencyHandle = x;
        virtualCurrencyDelegate = [VirtualCurrencyDelegateWrapper new];
        [AMRSDK setVirtualCurrencyDelegate:virtualCurrencyDelegate];
    }

    void _setSDKInitializeCallback(SDKDidInitializeCallback cb, int x) {
        sdkDidInitializeCallback = cb;
        sdkDidInitializeHandle = x;
    }
    
#pragma mark - Track Purchase
    
    void _setTrackPurchaseResponseCallback(TrackPurchaseResponseCallback cb, int x) {
        trackPurchaseResponseCallback = cb;
        
        trackPurchaseHandle = x;
        trackPurchaseResponseDelegate = [TrackPurchaseResponseDelegateWrapper new];
        [AMRSDK setTrackPurchaseResponseDelegate:trackPurchaseResponseDelegate];
    }
    
#pragma mark - Privacy
    
    void _setIsGDPRApplicableCallback(IsGDPRApplicableCallback cb, int x) {
        [AMRSDK isGDPRApplicable:^(BOOL isGDPRApplicable) {
            cb(x, (int)isGDPRApplicable);
        }];
    }

    void _setPrivacyConsentRequiredCallback(PrivacyConsentRequiredCallback cb, int x) {
        [AMRSDK isPrivacyConsentRequired:^(AMRPrivacyConsentStatus consentStatus) {
            cb(x, (int)consentStatus);
        }];
    }

#pragma mark - Remote Config
    char * _getRemoteConfigString(const char* key, const char* defaultValue) {
        AMRRemoteConfigValue *value = [AMRSDK getConfigForKey:CreateNSString(key)];
        
        if (value != nil && value.stringValue != nil) {
            const char *cString = [value.stringValue UTF8String];
            return cStringCopy(cString);
        }
        
        return cStringCopy(defaultValue);
    }

    double _getRemoteConfigDouble(const char* key, double defaultValue) {
        AMRRemoteConfigValue *value = [AMRSDK getConfigForKey:CreateNSString(key)];
        
        if (value == nil) {
            return defaultValue;
        }
        
        return value.numberValue.doubleValue;
    }

    long _getRemoteConfigLong(const char* key, long defaultValue) {
        AMRRemoteConfigValue *value = [AMRSDK getConfigForKey:CreateNSString(key)];
        
        if (value == nil) {
            return defaultValue;
        }
        
        return value.numberValue.longValue;
    }

    bool _getRemoteConfigBoolean(const char* key, bool defaultValue) {
        AMRRemoteConfigValue *value = [AMRSDK getConfigForKey:CreateNSString(key)];
        
        if (value == nil) {
            return defaultValue;
        }
        
        return value.numberValue.boolValue;
    }
}
