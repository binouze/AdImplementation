using System;
using System.Threading;
using System.Threading.Tasks;

#if UNITY_WEBGL
using UnityEngine;
#endif

namespace com.binouze
{
    public static class AsyncUtils
    {
        public static async Task Delay(int milisecondsDelay)
        {
            #if UNITY_WEBGL
            var seconds   = milisecondsDelay / 1000f;
            var startTime = Time.time;
            var end       = startTime + seconds;
            while (Time.time < end) await Task.Yield();
            #else
            await Task.Delay( milisecondsDelay );
            #endif
        }
        
        public static async Task<bool> Delay(int milisecondsDelay, CancellationToken token)
        {
            try
            {
                #if UNITY_WEBGL
                var seconds = milisecondsDelay / 1000f;
                var startTime = Time.time;
                var end = startTime + seconds;
                while (Time.time < end) 
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                #else
                await Task.Delay( milisecondsDelay, token );
                #endif
            }
            catch( Exception )
            {
                // ignored
                return false;
            }

            return true;
        }
    }
}