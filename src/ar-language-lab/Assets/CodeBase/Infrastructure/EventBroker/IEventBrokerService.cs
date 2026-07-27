using System;

namespace CodeBase.Infrastructure.EventBroker
{
    public interface IEventBrokerService : IDisposable
    {
        void Unsubscribe(ISubscriber subscriber);
        BrokerSubscription Subscribe(ISubscriber subscriber);
        void Rise<TSubscriber>(Action<TSubscriber> action) where TSubscriber : class, ISubscriber;
    }
}