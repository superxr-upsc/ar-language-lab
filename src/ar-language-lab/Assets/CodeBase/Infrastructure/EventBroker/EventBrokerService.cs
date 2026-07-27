using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeBase.Infrastructure.EventBroker
{
    public class EventBrokerService : IEventBrokerService
    {
        private readonly Dictionary<Type, ISubscribersCollection<ISubscriber>> _subscribers;
        private readonly Dictionary<Type, List<Type>> _cachedTypes;

        public EventBrokerService()
        {
            _subscribers = new Dictionary<Type, ISubscribersCollection<ISubscriber>>();
            _cachedTypes = new Dictionary<Type, List<Type>>();
        }

        public BrokerSubscription Subscribe(ISubscriber subscriber)
        {
            foreach (var type in GetSubscriberTypes(subscriber))
            {
                if (_subscribers.ContainsKey(type) == false)
                {
                    _subscribers[type] = new SubscribersCollection<ISubscriber>();
                }
             
                _subscribers[type].Add(subscriber);
            }

            return new BrokerSubscription(this, subscriber);
        }

        public void Unsubscribe(ISubscriber subscriber)
        {
            foreach (var type in GetSubscriberTypes(subscriber))
            {
                if (_subscribers.ContainsKey(type))
                {
                    _subscribers[type].Remove(subscriber);
                }
            }
        }

        public void Rise<TSubscriber>(Action<TSubscriber> action) where TSubscriber : class, ISubscriber
        {
            var subscriberType = typeof(TSubscriber);
            if (_subscribers.ContainsKey(subscriberType) == false) return;

            var subscribers = _subscribers[subscriberType];
            subscribers.IsExecuting = true;

            foreach (var subscriber in subscribers.GetCollection().ToArray())
            {
                try
                {
                    action.Invoke(subscriber as TSubscriber);
                }
                catch (Exception)
                {
                    subscribers.IsExecuting = false;
                    throw;
                }
            }

            subscribers.IsExecuting = false;
            subscribers.CleanCollection();
        }

        public List<Type> GetSubscriberTypes(ISubscriber subscriber)
        {
            var type = subscriber.GetType();
            if (_cachedTypes.ContainsKey(type))
            {
                return _cachedTypes[type];
            }

            var types = type.GetInterfaces()
                .Where(x => typeof(ISubscriber).IsAssignableFrom(x) && x != typeof(ISubscriber))
                .ToList();
            
            _cachedTypes.Add(type, types);
            return types;
        }

        public void Dispose()
        {
            _subscribers.Clear();
            _cachedTypes.Clear();
        }
    }
}