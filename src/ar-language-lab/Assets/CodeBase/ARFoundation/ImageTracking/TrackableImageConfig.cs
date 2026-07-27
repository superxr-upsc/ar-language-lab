using System;
using CodeBase.Gameplay.ARObjects;
using Unity.XR.CoreUtils;

namespace CodeBase.ARFoundation.ImageTracking
{
    [Serializable]
    public struct TrackableImageConfig
    {
        [ReadOnly] public string Id;
        public ARObjectBase Prefab;

        public TrackableImageConfig(Guid guid, ARObjectBase prefab)
        {
            Id = guid.ToString();
            Prefab = prefab;
        }
    }
}