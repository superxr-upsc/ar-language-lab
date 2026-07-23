using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;

namespace CodeBase.Infrastructure.GameStateMachineService.StateMachine
{
  public interface IGameStateMachine 
  {
    void Enter<TState>() where TState : class, IState;
    void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
  }
}