using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.EventBroker
{
    public interface ISubscribersCollection<TSubscriber> : IDisposable where TSubscriber : class
    {
        bool IsExecuting { get; set; }
        void Add(TSubscriber subscriber);
        void Remove(TSubscriber subscriber);
        void CleanCollection();
        IEnumerable<TSubscriber> GetCollection();
    }
}