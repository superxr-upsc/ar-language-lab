using System;
using System.Threading;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterGameplayLoopState : SimpleState
    {
        private readonly IEventBrokerService _eventBrokerService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly ISceneLoader _sceneLoader;

        private CancellationTokenSource _stateCts;

        public EnterGameplayLoopState(IEventBrokerService eventBrokerService,
            IVuforiaService vuforiaService,
            ILessonManagementService lessonManagementService,
            ISceneLoader sceneLoader)
        {
            _eventBrokerService = eventBrokerService;
            _vuforiaService = vuforiaService;
            _lessonManagementService = lessonManagementService;
            _sceneLoader = sceneLoader;
        }
        
        public override void Enter()
        {
            base.Enter();

            CancelStateCts();
            _stateCts = new CancellationTokenSource();
            
            SetupLesson(_stateCts.Token)
                .Forget();
        }

        protected override void Exit()
        {
            CancelStateCts();
            _lessonManagementService.CleanupLesson();
            
            _eventBrokerService.Rise<IGameLoopDisposable>(x => x.OnGameLoopDisposed());
            base.Exit();
        }

        private async UniTaskVoid SetupLesson(CancellationToken cancellationToken)
        {
            try
            {
                _vuforiaService.SetupVuforiaBehaviour();
                _sceneLoader.UpdateProgress(0.9f, "Preparing lesson...");
                
                await _lessonManagementService.SetupLesson();

                if (cancellationToken.IsCancellationRequested)
                    return;

                _sceneLoader.UpdateProgress(1f, string.Empty);

                _eventBrokerService.Rise<IGameLoopInitializable>(x => x.OnGameLoopInitialized());
                _sceneLoader.CloseLoadingScreen();

                if (_lessonManagementService.GetPerspective() == Perspective.Audio)
                    _lessonManagementService.StartAudioLesson();
                else
                    _lessonManagementService.StartLesson();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelStateCts()
        {
            if (_stateCts == null)
                return;

            _stateCts.Cancel();
            _stateCts.Dispose();
            _stateCts = null;
        }
    }
}
