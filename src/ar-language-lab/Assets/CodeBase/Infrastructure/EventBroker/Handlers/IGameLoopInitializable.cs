namespace CodeBase.Infrastructure.EventBroker.Handlers
{
    public interface IGameLoopInitializable : ISubscriber
    {
        void OnGameLoopInitialized();
    }
}