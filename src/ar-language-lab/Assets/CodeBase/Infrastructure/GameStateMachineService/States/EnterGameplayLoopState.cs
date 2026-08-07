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

        public EnterGameplayLoopState(IEventBrokerService eventBrokerService,
            IVuforiaService vuforiaService)
        {
            _eventBrokerService = eventBrokerService;
            _vuforiaService = vuforiaService;
        }
        
        public override void Enter()
        {
            base.Enter();
            _vuforiaService.SetupVuforiaBehaviour();
            _eventBrokerService.Rise<IGameLoopInitializable>(x => x.OnGameLoopInitialized());
        }

        protected override void Exit()
        {
            _eventBrokerService.Rise<IGameLoopDisposable>(x => x.OnGameLoopDisposed());
            base.Exit();
        }
    }
}