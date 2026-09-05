using System;
using System.Collections.Generic;
using CodeBase.Common.Extensions;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tasks.Resolvers
{
    public class PlaceObjectsInHierarchyTask : TaskResolverBase
    {
        private readonly ILocalizationService _localization;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly IProjectResourcesProvider _projectResourcesProvider;
        private readonly TaskResolversSettings _settings;
        private readonly Camera _camera;

        private readonly List<ARObjectBase> _objects = new();

        private IDisposable _updateSubscription;
        private Vector3 _sortingDirection;

        private int _stableCorrectFrames;

        public PlaceObjectsInHierarchyTask(
            TaskData taskData,
            IARCameraProvider cameraProvider,
            ILocalizationService localization,
            ILessonManagementService lessonManagementService,
            IProjectResourcesProvider projectResourcesProvider)
            : base(taskData, cameraProvider)
        {
            _localization = localization;
            _lessonManagementService = lessonManagementService;
            _projectResourcesProvider = projectResourcesProvider;
            _settings = projectResourcesProvider.LoadResource<TaskResolversSettings>();
            _camera = cameraProvider.GetActiveCamera();
            
            _sortingDirection = GetCameraRightDirection();
        }

        public override void Run(ActiveTaskData viewData)
        {
            base.Run(viewData);
            _updateSubscription = Observable.EveryUpdate()
                .Subscribe(_ => EvaluateOrder());
        }

        public override void Dispose()
        {
            DisposeUpdateSubscription();

            _objects.Clear();

            _sortingDirection = Vector3.zero;
            _stableCorrectFrames = 0;
            _projectResourcesProvider.ReleaseResource(_settings);
        }

        protected override void CompleteTask()
        {
            DisposeUpdateSubscription();
            base.CompleteTask();
        }

        protected override bool TryResolveTargets()
        {
            if (_taskData.TargetObjects.IsNullOrEmpty())
                return false;

            foreach (var config in _taskData.TargetObjects)
            {
                if (config == null)
                    return false;

                var arObject = _lessonManagementService.GetObject(config);

                if (arObject == null)
                    return false;

                _objects.Add(arObject);
            }

            return _objects.Count > 1;
        }

        protected override async UniTask<string> GetQuestDescription()
        {
            var objectNames = new List<string>(_taskData.TargetObjects.Length);

            foreach (var config in _taskData.TargetObjects)
            {
                var name = await _localization.GetStringAsync(
                    config.LocalisationKey);

                objectNames.Add(name);
            }

            var questDescription = await _localization.GetStringAsync(
                _taskData.DescriptionLocalizationKey,
                LocalizationConsts.DefaultStringTableName,
                new
                {
                    Objects = string.Join(", ", objectNames)
                });

            return questDescription;
        }

        private Vector3 GetCameraRightDirection()
        {
            return _camera != null
                ? _camera.transform.right
                : Vector3.right;
        }

        private void EvaluateOrder()
        {
            var isCorrectOrder = IsObjectsInCorrectOrder();

            _stableCorrectFrames = isCorrectOrder
                ? _stableCorrectFrames + 1
                : 0;

            if (_stableCorrectFrames >= _settings.RequiredStableFrames)
                CompleteTask();
        }

        private bool IsObjectsInCorrectOrder()
        {
            for (var i = 0; i < _objects.Count - 1; i++)
            {
                var currentObject = _objects[i];
                var nextObject = _objects[i + 1];

                if (!IsObjectCorrectlyPlaced(currentObject, nextObject))
                    return false;
            }

            return true;
        }

        private bool IsObjectCorrectlyPlaced(ARObjectBase currentObject, ARObjectBase nextObject)
        {
            var currentPosition = currentObject.transform.position;
            var nextPosition = nextObject.transform.position;

            var delta = nextPosition - currentPosition;

            //if positive - nextObject is on right from currentObject
            var distanceAlongAxis = Vector3.Dot(
                delta,
                _sortingDirection);
            
            return distanceAlongAxis >= _settings.SideOffsetMeters;
        }

        private void DisposeUpdateSubscription()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
        }
    }
}