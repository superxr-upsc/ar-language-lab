using System;

namespace CodeBase.Gameplay.ARFoundation.ImageTracking
{
    public interface IImageTrackingService : IDisposable
    {
        void Initialize();
    }
}