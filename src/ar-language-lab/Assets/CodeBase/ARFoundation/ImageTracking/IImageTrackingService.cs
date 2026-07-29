using System;

namespace CodeBase.ARFoundation.ImageTracking
{
    public interface IImageTrackingService : IDisposable
    {
        void Initialize();
        void Dispose();
    }
}