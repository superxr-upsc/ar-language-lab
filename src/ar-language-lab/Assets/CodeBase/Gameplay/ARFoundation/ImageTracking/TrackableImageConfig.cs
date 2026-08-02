using System;
using CodeBase.Gameplay.ARObjects;

namespace CodeBase.Gameplay.ARFoundation.ImageTracking
{
    [Serializable]
    public struct TrackableImageConfig
    {
        public string Id;
        public ARObjectBase Prefab;

        public TrackableImageConfig(Guid guid, ARObjectBase prefab)
        {
            Id = guid.ToString();
            Prefab = prefab;
        }
    }
}