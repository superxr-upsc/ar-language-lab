namespace CodeBase.Infrastructure.GameStateMachine.StateInfrastructure
{
  public interface IState: IExitableState
  {
    void Enter();
  }
}