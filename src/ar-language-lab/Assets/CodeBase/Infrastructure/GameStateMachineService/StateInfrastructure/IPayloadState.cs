namespace CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure
{
  public interface IPayloadState<TPayload> : IExitableState
  {
    void Enter(TPayload payload);
  }
}