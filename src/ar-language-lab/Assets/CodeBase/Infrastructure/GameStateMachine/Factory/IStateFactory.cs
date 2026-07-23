using CodeBase.Infrastructure.GameStateMachine.StateInfrastructure;

namespace CodeBase.Infrastructure.GameStateMachine.Factory
{
  public interface IStateFactory
  {
    T GetState<T>() where T : class, IExitableState;
  }
}