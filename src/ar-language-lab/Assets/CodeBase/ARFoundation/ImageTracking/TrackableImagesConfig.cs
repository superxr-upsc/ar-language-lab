using System;
using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace CodeBase.ARFoundation.ImageTracking
{
    [CreateAssetMenu(menuName = "ARFoundation/ImageTrackingConfig", fileName = "ImageTrackingConfig")]
    public class TrackableImagesConfig : ScriptableObject, ISerializationCallbackReceiver, IResource
    {
        public XRReferenceImageLibrary ImageLibrary;
        public List<TrackableImageConfig> ImagesConfig = new();

        private Dictionary<Guid, ARObjectBase> TrackableImageCache = new();

        public void OnBeforeSerialize()
        {
            ImagesConfig.Clear();
            foreach (var kvp in TrackableImageCache) 
                ImagesConfig.Add(new TrackableImageConfig(kvp.Key, kvp.Value));
        }

        public void OnAfterDeserialize()
        {
            TrackableImageCache.Clear();
            foreach (var entry in ImagesConfig) 
                TrackableImageCache.Add(Guid.Parse(entry.Id), entry.Prefab);
        }

        public ARObjectBase GetPrefabForReferenceImage(XRReferenceImage referenceImage) => 
            TrackableImageCache.GetValueOrDefault(referenceImage.guid);

        public void SetTrackableImagesCache(Dictionary<Guid, ARObjectBase> trackableImageCache) => 
            TrackableImageCache = trackableImageCache;

        public void ClearTrackableImagesCache() => 
            TrackableImageCache.Clear();
    }
}