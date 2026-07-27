using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterGameplayLoopState : SimpleState
    {
        private readonly IEventBrokerService _eventBrokerService;
        
        public EnterGameplayLoopState(IEventBrokerService eventBrokerService)
        {
            _eventBrokerService = eventBrokerService;
        }
        
        public override void Enter()
        {
            base.Enter();

            _eventBrokerService.Rise<IGameLoopInitializable>(x => x.OnGameLoopInitialized());
        }

        protected override void Exit()
        {
            _eventBrokerService.Rise<IGameLoopDisposable>(x => x.OnGameLoopDisposed());
            
            base.Exit();
        }
    }
}