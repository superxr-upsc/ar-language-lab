using System;
using R3;
using RSG;

namespace CodeBase.Infrastructure.WindowsManagement.MVPBase
{
    public class PresenterBase : IDisposable
    {
        protected CompositeDisposable _compositeDisposable;
        private ViewBase _viewBase;
        private bool _isClosing;

        public PresenterBase(ViewBase viewBase)
        {
            _viewBase = viewBase;
            _compositeDisposable = new CompositeDisposable();
        }

        public virtual void Dispose()
        {
            // Keep IDisposable sync-friendly while still honoring async close animations.
            DisposeAsync().Catch(UnityEngine.Debug.LogException);
        }

        public IPromise DisposeAsync() =>
            Close();

        public IPromise Close()
        {
            if (_isClosing || _viewBase == null)
                return Promise.Resolved();

            _isClosing = true;
            _compositeDisposable.Dispose();

            var viewToClose = _viewBase;

            return viewToClose.Close()
                .Then(ClearInstance);
        }

        protected virtual void ClearInstance()
        {
            var viewToDestroy = _viewBase.gameObject;
            _viewBase = null;

            if (viewToDestroy != null)
                UnityEngine.Object.Destroy(viewToDestroy);
        }
    }
}