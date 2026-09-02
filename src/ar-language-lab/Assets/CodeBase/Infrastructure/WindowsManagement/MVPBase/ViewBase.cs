using System;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using RSG;
using UnityEngine;

namespace CodeBase.Infrastructure.WindowsManagement.MVPBase
{
    public class ViewBase : MonoBehaviour, IResource
    {
        public Promise<ViewBase> Open() => new(OpenWindowAnimation);
        public Promise Close() => new(CloseWindowAnimation);
        
        protected virtual void OpenWindowAnimation(Action<ViewBase> resolve, Action<Exception> reject)
        {
            resolve(this);
        }
        
        protected virtual void CloseWindowAnimation(Action resolve, Action<Exception> reject)
        {
            resolve();
        }
    }
}