using RSG;

namespace CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure
{
  public interface IExitableState
  {
    IPromise BeginExit();
    void EndExit();
  }
}