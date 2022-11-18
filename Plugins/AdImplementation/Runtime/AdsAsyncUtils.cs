using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_WEBGL
using UnityEngine;
#endif

namespace com.binouze
{
    internal class AdsAsyncUtils : MonoBehaviour
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
        
        public static async void DelayCall( Action a, int ms )
        {
            await Delay( ms );
            CallOnMainThread( a );
        }
        
        public static async void DelayCall( Action a, int ms, CancellationToken token )
        {
            var ok = await Delay( ms, token );
            if( !ok )
                return;
            
            CallOnMainThread( a );
        }
        
        
        // MAIN THREAD DISPATCHER
        
        private static readonly Queue<Action> ActionsToCallOnMainThread = new();
        
        private void Update() 
        {
            lock( ActionsToCallOnMainThread ) 
            {
                while( ActionsToCallOnMainThread.Count > 0 ) 
                {
                    ActionsToCallOnMainThread.Dequeue().Invoke();
                }
            }
        }
        
        private void _Enqueue( Action action )
        {
            lock( ActionsToCallOnMainThread )
            {
                ActionsToCallOnMainThread.Enqueue( action );
            }
        }

        public static void CallOnMainThread( Action action )
        {
            GetInstance()?._Enqueue( action );
        }
        
        private static AdsAsyncUtils _instance;
        private static AdsAsyncUtils GetInstance()
        {
            if( _instance == null ) 
            {
                _instance = (AdsAsyncUtils)FindObjectOfType( typeof(AdsAsyncUtils) );
                if( _instance == null ) 
                {
                    const string goName = "[AdsAsyncUtils]";          

                    var go = GameObject.Find( goName );
                    if( go == null ) 
                    {
                        go = new GameObject {name = goName};
                        DontDestroyOnLoad( go );
                    }
                    _instance = go.AddComponent<AdsAsyncUtils>();                   
                }
            }
            return _instance;
        }
    }
}