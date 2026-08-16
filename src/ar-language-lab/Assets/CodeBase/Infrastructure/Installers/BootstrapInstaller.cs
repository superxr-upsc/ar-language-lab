using System.Diagnostics;
using CodeBase.DebugExtensions;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.CoroutineRunner;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.TimerService;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.Services.InputService;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private JahroExtensions _jahroExtensions;
        [SerializeField] private CoroutineRunnerComponent _coroutineRunner;
        [SerializeField] private WindowsManagementService _windowsManagementService;
        [SerializeField] private TTSService _ttsService;
        
        public override void InstallBindings()
        {
            //Only for debug builds
            BindDebugConsoleExtentions();

            //Bind services with prefab instances
            BindCoroutineRunner();
            BindWindowsManagementService();
            BindTTSService();

            //Bind other services
            BindEventBrokerService();
            BindGameStateMachine();
            BindGameFactory();
            BindInputService();
            BindSceneLoadingService();
            BindProjectResourcesProvider();
            BindTimerService();
            BindLocalizationService();
            BindVuforiaService();
            BindARCameraProvider();
            BindLessonManagementService();
        }

        private void BindLessonManagementService()
        {
            Container.Bind<ILessonManagementService>()
                .To<LessonManagementService>()
                .AsSingle();
        }

        private void BindVuforiaService()
        {
            Container.BindInterfacesTo<VuforiaService>()
                .AsSingle();
        }

        private void BindARCameraProvider()
        {
            Container.Bind<IARCameraProvider>()
                .To<ARCameraProvider>()
                .AsSingle();
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

        private void BindInputService()
        {
            Container.Bind<IInputService>()
                .To<PlayerInputService>()
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

        private void BindLocalizationService()
        {
            Container.Bind<ILocalizationService>()
                .To<LocalizationService>()
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

        private void BindTTSService()
        {
            Container.BindInterfacesTo<TTSService>()
                .FromComponentInNewPrefab(_ttsService)
                .AsSingle()
                .NonLazy();
        }
    }
}