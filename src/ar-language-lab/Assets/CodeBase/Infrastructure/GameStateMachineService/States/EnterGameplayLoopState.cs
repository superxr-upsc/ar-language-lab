using CodeBase.Gameplay.Lessons;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.Vuforia;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterGameplayLoopState : SimpleState
    {
        private readonly IEventBrokerService _eventBrokerService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly ISceneLoader _sceneLoader;

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
            
            _sceneLoader.UpdateProgress(1f, "Setup vuforia behaviour...");
            
            _vuforiaService.SetupVuforiaBehaviour();
            _lessonManagementService.SetupLesson();
            
            _eventBrokerService.Rise<IGameLoopInitializable>(x => x.OnGameLoopInitialized());
            _lessonManagementService.StartLesson();
            
            _sceneLoader.CloseLoadingScreen();
        }

        protected override void Exit()
        {
            _lessonManagementService.CleanupLesson();
            
            _eventBrokerService.Rise<IGameLoopDisposable>(x => x.OnGameLoopDisposed());
            base.Exit();
        }
    }
}