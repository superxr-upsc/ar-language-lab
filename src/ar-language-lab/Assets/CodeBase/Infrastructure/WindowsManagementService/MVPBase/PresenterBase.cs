using System;

namespace CodeBase.Infrastructure.WindowsManagementService.MVPBase
{
    public class PresenterBase : IDisposable
    {
        protected ModelBase Model;
        protected ViewBase View;

        public PresenterBase(ModelBase model, ViewBase view)
        {
            Model = model;
            View = view;
        }

        public virtual void Dispose()
        {
            View.Close()
                .Then(ClearInstance)
                .Catch(exception => throw exception);
        }

        protected virtual void ClearInstance()
        {
            Model = null;
            View = null;
        }
    }
}