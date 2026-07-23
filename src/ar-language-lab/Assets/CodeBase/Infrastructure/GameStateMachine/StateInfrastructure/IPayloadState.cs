namespace CodeBase.Infrastructure.GameStateMachine.StateInfrastructure
{
  public interface IPayloadState<TPayload> : IExitableState
  {
    void Enter(TPayload payload);
  }
}