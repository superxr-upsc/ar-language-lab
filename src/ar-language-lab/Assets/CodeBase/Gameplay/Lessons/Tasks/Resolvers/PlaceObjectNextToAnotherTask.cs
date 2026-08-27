using System;
using CodeBase.Common.Extensions;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class PlaceObjectNextToAnotherTask : TaskResolverBase
    {
        private const float NearDistanceMeters = 0.25f;
        private const int RequiredStableFrames = 10;

        private readonly ILocalizationService _localization;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly Speaker _speaker;

        private ARObjectConfig _subjectObjectConfig;
        private ARObjectConfig _referenceObjectConfig;

        private ARObjectBase _subjectObject;
        private ARObjectBase _referenceObject;

        private IDisposable _updateSubscription;
        private bool _completed;
        private int _stableNearFrames;

        public PlaceObjectNextToAnotherTask(TaskData taskData, ILocalizationService localization, IARCameraProvider cameraProvider, ILessonManagementService lessonManagementService) : base(taskData)
        {
            _localization = localization;
            _lessonManagementService = lessonManagementService;
            _speaker = cameraProvider.GetSpeaker();
        }

        public override void Run()
        {
            TryResolveObjects();
            if (_subjectObject == null || _referenceObject == null)
            {
                CompleteTask();
                return;
            }

            PlayQuestDescription().Forget();

            _updateSubscription = Observable.EveryUpdate()
                .Subscribe(_ => EvaluateProximity());
        }

        public override void Dispose()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;

            _subjectObject = null;
            _referenceObject = null;
            _subjectObjectConfig = null;
            _referenceObjectConfig = null;
        }

        protected override bool IsTaskComplete() =>
            _completed;

        protected override void CompleteTask()
        {
            if (_completed)
                return;

            _completed = true;
            _updateSubscription?.Dispose();
            _updateSubscription = null;
            base.CompleteTask();
        }

        private void TryResolveObjects()
        {
            if (_taskData.TargetObjects.IsNullOrEmpty())
                return;

            _subjectObjectConfig = _taskData.TargetObjects.PickRandom();
            if (_subjectObjectConfig == null)
                return;

            _subjectObject = _lessonManagementService.GetObject(_subjectObjectConfig);
            if (_subjectObject == null)
                return;

            _referenceObjectConfig = PickReferenceObjectConfig();
            if (_referenceObjectConfig == null)
                return;

            _referenceObject = _lessonManagementService.GetObject(_referenceObjectConfig);
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

        private async UniTaskVoid PlayQuestDescription()
        {
            var subjectName = await _localization.GetStringAsync(_subjectObjectConfig.LocalisationKey);
            var referenceName = await _localization.GetStringAsync(_referenceObjectConfig.LocalisationKey);

            var questDescription = await _localization.GetStringAsync(
                _taskData.DescriptionLocalizationKey,
                LocalizationConsts.DefaultStringTableName,
                new { SubjectName = subjectName, ReferenceName = referenceName });

            await _speaker.SpeakAsync(questDescription);
        }

        private void EvaluateProximity()
        {
            if (_completed || _subjectObject == null || _referenceObject == null)
                return;

            var distance = Vector3.Distance(_subjectObject.transform.position, _referenceObject.transform.position);
            var isNear = distance <= NearDistanceMeters;

            if (isNear)
            {
                GameLogger.Log("IS NEAR! ===== " + distance);
            }
            
            _stableNearFrames = isNear ? _stableNearFrames + 1 : 0;

            if (_stableNearFrames >= RequiredStableFrames)
            {
                GameLogger.Log("IS COMPLETED!");
                CompleteTask();
            }
        }
    }
}