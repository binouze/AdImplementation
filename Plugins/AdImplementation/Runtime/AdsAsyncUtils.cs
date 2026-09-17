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
        /// <summary>
        /// Voir AdImplementation.ResetStatics. Le dispatcher est un MonoBehaviour: son GameObject meurt avec la
        /// session de jeu, alors que les statiques de la classe, elles, survivent quand le Domain Reload est
        /// desactive. Comme SetInstance() n'est appele que depuis AdImplementation.Initialize() - qui sort tout
        /// de suite sur son statique IsInIt -, la 2e session de play et les suivantes tournaient SANS
        /// dispatcher: la file se remplissait et plus AUCUN callback de pub n'etait livre au jeu (ni OnAdShown,
        /// ni OnAdClose, ni OnComplete) alors que la regie, elle, signalait bien tout.
        /// </summary>
        internal static void ResetStatics()
        {
            lock( ActionsToCallOnMainThread )
            {
                // callbacks de la session precedente: plus personne pour les traiter
                ActionsToCallOnMainThread.Clear();
            }

            AppPauseeDepuisDerniereDemande = false;
            _instance                      = null; // le GameObject de la session precedente est detruit
        }

        /// <summary>
        /// Recreer le dispatcher a chaque demarrage. Volontairement ici et pas dans ResetStatics
        /// (SubsystemRegistration): creer un GameObject DontDestroyOnLoad aussi tot n'est pas sur. Et
        /// CallOnMainThread ne peut pas s'en charger lui-meme, il est appele depuis des threads natifs ou
        /// creer un GameObject / appeler FindObjectOfType leve.
        /// </summary>
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        private static void CreerDispatcher()
        {
            SetInstance();
        }
        
        
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


        /// <summary>
        /// true si l'app est passee en arriere plan depuis la derniere demande d'affichage de pub.
        /// Sur Android une pub s'affiche dans une autre Activity: c'est la preuve que quelque chose s'est bien
        /// affiche par dessus le jeu, meme si la regie n'a rien signale. Utilise par le filet de securite
        /// AdImplementation.SetMaxTimeBeforeAdShown, remis a false a chaque nouvelle demande d'affichage.
        /// </summary>
        internal static bool AppPauseeDepuisDerniereDemande;

        private void OnApplicationPause( bool pause )
        {
            if( pause )
                AppPauseeDepuisDerniereDemande = true;
        }
        
        private static void _Enqueue( Action action )
        {
            lock( ActionsToCallOnMainThread )
            {
                ActionsToCallOnMainThread.Enqueue( action );
            }
        }

        public static void CallOnMainThread( Action action )
        {
            _Enqueue( action );
        }
        
        private static AdsAsyncUtils _instance;
        internal static void SetInstance()
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
        }
    }
}