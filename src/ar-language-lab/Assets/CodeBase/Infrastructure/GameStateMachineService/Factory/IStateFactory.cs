using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;

namespace CodeBase.Infrastructure.GameStateMachineService.Factory
{
  public interface IStateFactory
  {
    T GetState<T>() where T : class, IExitableState;
  }
}