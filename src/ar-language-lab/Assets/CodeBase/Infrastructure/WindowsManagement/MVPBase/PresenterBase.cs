using System;
using R3;

namespace CodeBase.Infrastructure.WindowsManagement.MVPBase
{
    public class PresenterBase : IDisposable
    {
        protected CompositeDisposable _compositeDisposable;
        private ViewBase _viewBase;

        public PresenterBase(ViewBase viewBase)
        {
            _viewBase = viewBase;
            _compositeDisposable = new CompositeDisposable();
        }

        public virtual void Dispose()
        {
            _compositeDisposable.Dispose();
            
            _viewBase.Close()
                .Then(ClearInstance)
                .Catch(exception => throw exception);
        }

        protected virtual void ClearInstance()
        {
            _viewBase = null;
        }
    }
}