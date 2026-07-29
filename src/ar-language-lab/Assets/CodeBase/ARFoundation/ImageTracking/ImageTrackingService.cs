using System;
using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace CodeBase.ARFoundation.ImageTracking
{
    public class ImageTrackingService : IImageTrackingService, IGameLoopInitializable, IGameLoopDisposable
    {
        private readonly AROriginContainer _arContainer;
        private readonly IProjectResourcesProvider _resourceProvider;
        private readonly IGameFactory _factory;
        private readonly IEventBrokerService _eventBrokerService;
        private readonly Dictionary<Guid, ARObjectBase> _instantiatedObjects;

        private TrackableImagesConfig _config;
        
        public ImageTrackingService(AROriginContainer arContainer, IProjectResourcesProvider resourceProvider, IGameFactory factory, IEventBrokerService eventBrokerService)
        {
            _arContainer = arContainer;
            _resourceProvider = resourceProvider;
            _factory = factory;
            _eventBrokerService = eventBrokerService;

            _eventBrokerService.Subscribe(this);
            _instantiatedObjects = new Dictionary<Guid, ARObjectBase>();
        }

        public void OnGameLoopInitialized() => 
            Initialize();

        public void OnGameLoopDisposed() => 
            Dispose();

        public void Initialize()
        {
            _config = _resourceProvider.LoadResource<TrackableImagesConfig>();
            _arContainer.TrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }

        public void Dispose()
        {
            _arContainer.TrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            _resourceProvider.ReleaseResource(_config);
            
            _eventBrokerService.Unsubscribe(this);
        }

        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (var trackedImage in eventArgs.added) 
                AssignPrefab(trackedImage);
            
            foreach (var trackedImage in eventArgs.updated) 
                UpdatePrefab(trackedImage);

            foreach (var trackedImage in eventArgs.removed) 
                CleanupPrefab(trackedImage);
        }

        private void AssignPrefab(ARTrackedImage trackedImage)
        {
            var prefab = _config.GetPrefabForReferenceImage(trackedImage.referenceImage);
            if (prefab == null) 
                return;
            
            var instantiatedReferenceObject = _factory.CreateFromPrefab<ARObjectBase>(prefab, trackedImage.transform);
            instantiatedReferenceObject.Initialize(trackedImage);

            _instantiatedObjects[trackedImage.referenceImage.guid] = instantiatedReferenceObject;
        }

        private void CleanupPrefab(KeyValuePair<TrackableId, ARTrackedImage> trackedImageInfo)
        {
            var prefab = _instantiatedObjects.GetValueOrDefault(trackedImageInfo.Value.referenceImage.guid);
            if (prefab == null) 
                return;
            
            prefab.Cleanup();
            _instantiatedObjects.Remove(trackedImageInfo.Value.referenceImage.guid);
        }

        private void UpdatePrefab(ARTrackedImage trackedImage)
        {
            var prefab = _instantiatedObjects.GetValueOrDefault(trackedImage.referenceImage.guid);
            if (prefab != null) 
                prefab.Refresh(trackedImage);
        }
    }
}