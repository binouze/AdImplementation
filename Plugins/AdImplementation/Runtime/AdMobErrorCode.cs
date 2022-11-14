namespace com.binouze
{
    internal enum AdMobErrorCode
    {
        INVALID_REQUEST,
        NO_FILL,
        MEDIATION_NO_FILL,
        NETWORK_ERROR,
        SERVER_ERROR,
        OS_NOT_SUPPORTED,
        TIMEOUT,
        MEDIATION_DATA_ERROR,
        MEDIATION_ADAPTER_ERROR,
        MEDIATION_INVALID_AD,
        INTERNAL_ERROR,
        INVALID_ARGUMENT,
        INVALID_RESPONSE,
        AD_ALREADY_USED,
        APP_ID_MISSING,
        
    }
    internal static class AdMobErrorCodeHelper
    {
        public static AdMobErrorCode GetErrorCodeFromInteger( int errorCode )
        {
            #if UNITY_IOS
            return errorCode switch
            {
                0  => AdMobErrorCode.INVALID_REQUEST,
                1  => AdMobErrorCode.NO_FILL,
                2 => AdMobErrorCode.NETWORK_ERROR,
                3  => AdMobErrorCode.SERVER_ERROR,
                4  => AdMobErrorCode.OS_NOT_SUPPORTED,
                5  => AdMobErrorCode.TIMEOUT,
                7  => AdMobErrorCode.MEDIATION_DATA_ERROR,
                8 => AdMobErrorCode.MEDIATION_ADAPTER_ERROR,
                9 => AdMobErrorCode.MEDIATION_NO_FILL,
                10 => AdMobErrorCode.MEDIATION_INVALID_AD,
                11 => AdMobErrorCode.INTERNAL_ERROR,
                12 => AdMobErrorCode.INVALID_ARGUMENT,
                13 => AdMobErrorCode.INVALID_RESPONSE,
                19 => AdMobErrorCode.AD_ALREADY_USED,
                20 => AdMobErrorCode.APP_ID_MISSING,
                _  => AdMobErrorCode.INTERNAL_ERROR
            };
            #elif UNITY_ANDROID
            return errorCode switch
            {
                8  => AdMobErrorCode.APP_ID_MISSING,
                0  => AdMobErrorCode.INTERNAL_ERROR,
                11 => AdMobErrorCode.INVALID_REQUEST,
                1  => AdMobErrorCode.INVALID_REQUEST,
                9  => AdMobErrorCode.MEDIATION_NO_FILL,
                2  => AdMobErrorCode.NETWORK_ERROR,
                3  => AdMobErrorCode.NO_FILL,
                10 => AdMobErrorCode.INVALID_REQUEST,
                _  => AdMobErrorCode.INTERNAL_ERROR
            };
            #else
            return AdMobErrorCode.OS_NOT_SUPPORTED;
            #endif
        }
    }
}