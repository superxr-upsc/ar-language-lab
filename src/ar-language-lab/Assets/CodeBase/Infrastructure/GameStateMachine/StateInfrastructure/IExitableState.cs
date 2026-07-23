using RSG;

namespace CodeBase.Infrastructure.GameStateMachine.StateInfrastructure
{
  public interface IExitableState
  {
    IPromise BeginExit();
    void EndExit();
  }
}