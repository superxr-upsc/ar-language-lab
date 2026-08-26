using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.Lessons;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.Vuforia;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterGameplayLoopState : SimpleState
    {
        private readonly IEventBrokerService _eventBrokerService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ILessonManagementService _lessonManagementService;

        public EnterGameplayLoopState(IEventBrokerService eventBrokerService,
            IVuforiaService vuforiaService,
            ILessonManagementService lessonManagementService)
        {
            _eventBrokerService = eventBrokerService;
            _vuforiaService = vuforiaService;
            _lessonManagementService = lessonManagementService;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _vuforiaService.SetupVuforiaBehaviour();
            _lessonManagementService.SetupLesson();
            
            _eventBrokerService.Rise<IGameLoopInitializable>(x => x.OnGameLoopInitialized());
            
            GameLogger.Log("ENTERED GAMEPLAY LOOP");
            
            _lessonManagementService.StartLesson();
        }

        protected override void Exit()
        {
            _lessonManagementService.CleanupLesson();
            
            _eventBrokerService.Rise<IGameLoopDisposable>(x => x.OnGameLoopDisposed());
            base.Exit();
        }
    }
}