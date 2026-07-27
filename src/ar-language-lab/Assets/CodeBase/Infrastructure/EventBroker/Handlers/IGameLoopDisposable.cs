namespace CodeBase.Infrastructure.EventBroker.Handlers
{
    public interface IGameLoopDisposable : ISubscriber
    {
        void OnGameLoopDisposed();
    }
}