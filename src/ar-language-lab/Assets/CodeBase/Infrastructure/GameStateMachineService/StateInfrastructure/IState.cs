namespace CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure
{
  public interface IState: IExitableState
  {
    void Enter();
  }
}