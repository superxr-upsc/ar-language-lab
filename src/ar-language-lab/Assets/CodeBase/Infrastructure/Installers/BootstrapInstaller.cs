using System.Diagnostics;
using CodeBase.DebugExtensions;
using CodeBase.Infrastructure.CoroutineRunner;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.TimerService;
using CodeBase.Infrastructure.WindowsManagement;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private JahroExtensions _jahroExtensions;
        [SerializeField] private CoroutineRunnerComponent _coroutineRunner;
        [SerializeField] private WindowsManagementService _windowsManagementService;
        
        public override void InstallBindings()
        {
            //Only for debug builds
            BindDebugConsoleExtentions();

            //Bind services with prefab instances
            BindCoroutineRunner();
            BindWindowsManagementService();
            
            //Bind other services
            BindEventBrokerService();
            BindGameStateMachine();
            BindGameFactory();
            BindSceneLoadingService();
            BindProjectResourcesProvider();
            BindTimerService();
        }

        private void BindEventBrokerService()
        {
            Container.Bind<IEventBrokerService>()
                .To<EventBrokerService>()
                .AsSingle();
        }

        private void BindGameStateMachine()
        {
            Container.BindInterfacesTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameFactory()
        {
            Container.Bind<IGameFactory>()
                .To<GameFactory.GameFactory>()
                .AsSingle();
        }

        private void BindSceneLoadingService()
        {
            Container.Bind<ISceneLoader>()
                .To<SceneLoader>()
                .AsSingle();
        }

        private void BindProjectResourcesProvider()
        {
            Container.Bind<IProjectResourcesProvider>()
                .To<ProjectResourcesProvider.ProjectResourcesProvider>()
                .AsSingle();
        }

        private void BindTimerService()
        {
            Container.Bind<ITimerService>()
                .To<TimerService.TimerService>()
                .AsSingle();
        }

        [Conditional("DEBUG_LOGS_ENABLED")]
        private void BindDebugConsoleExtentions()
        {
            Container.Bind<JahroExtensions>()
                .FromComponentInNewPrefab(_jahroExtensions)
                .AsSingle()
                .NonLazy();
        }

        private void BindWindowsManagementService()
        {
            Container.Bind<IWindowsManagementService>()
                .To<WindowsManagementService>()
                .FromComponentInNewPrefab(_windowsManagementService)
                .AsSingle()
                .NonLazy();
        }

        private void BindCoroutineRunner()
        {
            Container.Bind<ICoroutineRunner>()
                .To<CoroutineRunnerComponent>()
                .FromComponentInNewPrefab(_coroutineRunner)
                .AsSingle()
                .NonLazy();
        }
    }
}