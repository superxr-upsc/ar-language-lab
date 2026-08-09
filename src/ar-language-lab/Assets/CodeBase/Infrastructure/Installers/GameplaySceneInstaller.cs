using CodeBase.Infrastructure.Vuforia;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private ARCamera _arCamera;
                
        public override void InstallBindings()
        {
            BindARCamera();
            
        }

        private void BindARCamera()
        {
            Container.Bind<ARCamera>()
                .FromComponentInNewPrefab(_arCamera)
                .AsSingle()
                .NonLazy();
        }
    }
}