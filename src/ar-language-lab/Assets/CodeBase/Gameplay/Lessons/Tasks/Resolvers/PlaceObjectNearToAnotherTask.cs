using System;
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
    public class PlaceObjectNearToAnotherTask : TaskResolverBase
    {
        private readonly ILocalizationService _localization;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly IProjectResourcesProvider _projectResourcesProvider;
        private readonly TaskResolversSettings _settings;
        
        private ARObjectConfig _subjectObjectConfig;
        private ARObjectConfig _referenceObjectConfig;

        private ARObjectBase _subjectObject;
        private ARObjectBase _referenceObject;
        
        private IDisposable _updateSubscription;
        private int _stableNearAndSideFrames;

        public PlaceObjectNearToAnotherTask(
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
        }

        public override void Run(ActiveTaskData viewData)
        {
            base.Run(viewData);

            _updateSubscription = Observable.EveryUpdate()
                .Subscribe(_ => EvaluatePlacement());
        }

        public override void Dispose()
        {
            DisposeUpdateSubscription();

            _subjectObject = null;
            _referenceObject = null;
            _subjectObjectConfig = null;
            _referenceObjectConfig = null;
            
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

            _subjectObjectConfig = _taskData.TargetObjects.PickRandom();
            if (_subjectObjectConfig == null)
                return false;

            _subjectObject = _lessonManagementService.GetObject(_subjectObjectConfig);
            if (_subjectObject == null)
                return false;

            _referenceObjectConfig = PickReferenceObjectConfig();
            if (_referenceObjectConfig == null)
                return false;

            _referenceObject = _lessonManagementService.GetObject(_referenceObjectConfig);
            return _referenceObject != null;
        }

        protected override async UniTask<string> GetQuestDescription()
        {
            var subjectName = await _localization.GetStringAsync(_subjectObjectConfig.LocalisationKey);
            var referenceName = await _localization.GetStringAsync(_referenceObjectConfig.LocalisationKey);

            var questDescription = await _localization.GetStringAsync(
                _taskData.DescriptionLocalizationKey,
                LocalizationConsts.DefaultStringTableName,
                new { SubjectName = subjectName, ReferenceName = referenceName });
            
            return questDescription;
        }

        private ARObjectConfig PickReferenceObjectConfig()
        {
            if (!_taskData.SecondaryTargetObjects.IsNullOrEmpty())
            {
                foreach (var config in _taskData.SecondaryTargetObjects)
                {
                    if (config == null || config == _subjectObjectConfig)
                        continue;

                    return config;
                }
            }

            foreach (var config in _taskData.TargetObjects)
            {
                if (config == null || config == _subjectObjectConfig)
                    continue;

                return config;
            }

            return null;
        }

        private void EvaluatePlacement()
        {
            if (_subjectObject == null || _referenceObject == null)
                return;

            var subjectPosition = _subjectObject.transform.position;
            var referencePosition = _referenceObject.transform.position;
            var distance = Vector3.Distance(subjectPosition, referencePosition);
            var isNear = distance <= _settings.NearDistanceMeters;
            var isCorrectSide = IsOnRequiredSide(subjectPosition, referencePosition);

            var isValidPlacement = isNear && isCorrectSide;
            _stableNearAndSideFrames = isValidPlacement ? _stableNearAndSideFrames + 1 : 0;

            if (_stableNearAndSideFrames >= _settings.RequiredStableFrames)
                CompleteTask();
        }

        private bool IsOnRequiredSide(Vector3 subjectPosition, Vector3 referencePosition)
        {
            var worldDelta = subjectPosition - referencePosition;
            var localDelta = _referenceObject.transform.InverseTransformDirection(worldDelta);

            var requiredSide = _taskData.RequiredSide;
            return requiredSide switch
            {
                PlacementSide.Any => true,
                PlacementSide.OnTop => IsOnTop(localDelta),
                PlacementSide.Above => IsPositiveDominant(localDelta.y, localDelta.x, localDelta.z),
                PlacementSide.Below => IsNegativeDominant(localDelta.y, localDelta.x, localDelta.z),
                PlacementSide.Left => IsNegativeDominant(localDelta.x, localDelta.y, localDelta.z),
                PlacementSide.Right => IsPositiveDominant(localDelta.x, localDelta.y, localDelta.z),
                PlacementSide.Front => IsPositiveDominant(localDelta.z, localDelta.x, localDelta.y),
                PlacementSide.Back => IsNegativeDominant(localDelta.z, localDelta.x, localDelta.y),
                _ => true
            };
        }

        private bool IsOnTop(Vector3 localDelta)
        {
            if (localDelta.y < _settings.SideOffsetMeters)
                return false;

            var horizontalDistance = Mathf.Sqrt(localDelta.x * localDelta.x + localDelta.z * localDelta.z);
            return horizontalDistance <= _settings.TopHorizontalToleranceMeters;
        }

        private bool IsPositiveDominant(float axisValue, float secondaryAxis, float tertiaryAxis)
        {
            return axisValue >= _settings.SideOffsetMeters
                   && axisValue >= Mathf.Abs(secondaryAxis) + _settings.AxisAdvantageMeters
                   && axisValue >= Mathf.Abs(tertiaryAxis) + _settings.AxisAdvantageMeters;
        }

        private bool IsNegativeDominant(float axisValue, float secondaryAxis, float tertiaryAxis)
        {
            var magnitude = -axisValue;
            return magnitude >= _settings.SideOffsetMeters
                   && magnitude >= Mathf.Abs(secondaryAxis) + _settings.AxisAdvantageMeters
                   && magnitude >= Mathf.Abs(tertiaryAxis) + _settings.AxisAdvantageMeters;
        }

        private void DisposeUpdateSubscription()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
        }
    }
}