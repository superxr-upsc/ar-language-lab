using System.Collections.Generic;

namespace CodeBase.Infrastructure.EventBroker
{
    public class SubscribersCollection<TSubscriber> : ISubscribersCollection<TSubscriber> where TSubscriber : class
    {
        public bool IsExecuting { get; set; }
        
        private readonly List<TSubscriber> _subscribers;

        public SubscribersCollection()
        {
            _subscribers = new List<TSubscriber>();
            IsExecuting = false;
        }

        public void Add(TSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Remove(TSubscriber subscriber)
        {
            if (IsExecuting)
            {
                int subscriberIndex = _subscribers.IndexOf(subscriber);
                if (subscriberIndex >= 0)
                {
                    _subscribers[subscriberIndex] = null;
                }
            }
            else
            {
                _subscribers.Remove(subscriber);
            }
        }

        public void CleanCollection()
        {
            _subscribers.RemoveAll(x => x is null);
        }

        public IEnumerable<TSubscriber> GetCollection()
        {
            return _subscribers;
        }

        public void Dispose()
        {
            _subscribers.Clear();
        }
    }
}