using System;

namespace CodeBase.Infrastructure.EventBroker
{
    public class BrokerSubscription : IDisposable
    {
        private readonly IEventBrokerService _eventBrokerService;
        private readonly ISubscriber _subscriber;

        public BrokerSubscription(IEventBrokerService eventBrokerService, ISubscriber subscriber)
        {
            _eventBrokerService = eventBrokerService;
            _subscriber = subscriber;
        }

        public void Dispose()
        {
            _eventBrokerService.Unsubscribe(_subscriber);
        }
    }
}