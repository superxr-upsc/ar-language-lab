using CodeBase.ARFoundation;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private ARSession _sessionPrefab;
        [SerializeField] private AROriginContainer _originContainerPrefab;
        
        public override void InstallBindings()
        {
            BindARSession();
            BindArOriginContainer();
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
    }
}