using System.Diagnostics;
using CodeBase.ARFoundation;
using CodeBase.ARFoundation.ImageTracking;
using CodeBase.DebugExtensions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private ARSession _sessionPrefab;
        [SerializeField] private AROriginContainer _originContainerPrefab;
        
        // For AR Foundation debug purposes
        [SerializeField] private ARDebugMenu _arDebugMenuPrefab;
        
        public override void InstallBindings()
        {
            BindARSession();
            BindArOriginContainer();
            
            BindImageTrackingService();
            
            // Only for debug builds
            BindArDebugMenu();
        }

        private void BindARSession()
        {
            Container.Bind<ARSession>()
                .FromComponentInNewPrefab(_sessionPrefab)
                .AsSingle()
                .NonLazy();
        }

        private void BindArOriginContainer()
        {
            Container.Bind<AROriginContainer>()
                .FromComponentInNewPrefab(_originContainerPrefab)
                .AsSingle()
                .NonLazy();
        }

        private void BindImageTrackingService()
        {
            Container.Bind<IImageTrackingService>()
                .To<ImageTrackingService>()
                .AsSingle()
                .NonLazy();
        }

        [Conditional("DEBUG_LOGS_ENABLED")]
        private void BindArDebugMenu()
        {
            Container.Bind<ARDebugMenu>()
                .FromComponentInNewPrefab(_arDebugMenuPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}