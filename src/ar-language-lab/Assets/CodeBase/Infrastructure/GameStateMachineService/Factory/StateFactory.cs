using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using Zenject;

namespace CodeBase.Infrastructure.GameStateMachineService.Factory
{
  public class StateFactory : IStateFactory
  {
    private readonly DiContainer _container;

    public StateFactory(DiContainer container)
    {
      _container = container;
    }

    public T GetState<T>() where T : class, IExitableState
    {
      return _container.Resolve<T>();
    }
  }
}