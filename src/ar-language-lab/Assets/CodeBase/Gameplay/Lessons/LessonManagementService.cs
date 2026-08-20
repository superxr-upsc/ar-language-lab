using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.Vuforia;
using UnityEngine;
using Vuforia;

namespace CodeBase.Gameplay.Lessons
{
    public class LessonManagementService : ILessonManagementService
    {
        private readonly IGameFactory _gameFactory;
        private readonly IProjectResourcesProvider _resourcesProvider;
        private readonly IARCameraProvider _arCameraProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IVuforiaService _vuforiaService;

        private LessonConfig _lessonConfig;

        private Transform _gameplayObjectsParent;
        private Dictionary<MultiTargetBehaviour, ARObjectBase> _lessonObjects = new();

        public LessonManagementService(IGameFactory gameFactory,
            IProjectResourcesProvider resourcesProvider,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService,
            IVuforiaService vuforiaService)
        {
            _gameFactory = gameFactory;
            _resourcesProvider = resourcesProvider;
            _arCameraProvider = arCameraProvider;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
        }

        public void SetupLesson()
        {
            _lessonConfig = GetSelectedLesson();

            SetupGameplayObjectsParent();
            SetupGameplayObjects();
        }

        public void CleanupLesson()
        {
            foreach (var arObject in _lessonObjects) 
                arObject.Value.Cleanup(arObject.Key);
            
            _lessonObjects.Clear();
        }

        private LessonConfig GetSelectedLesson()
        {
            // TODO : Should get selected lesson from main menu 
            return _resourcesProvider.LoadResource<LessonConfig>();
        }

        private void SetupGameplayObjectsParent() => 
            _gameplayObjectsParent = new GameObject("[GameplayObjectsParent]").transform;

        private void SetupGameplayObjects()
        {
            foreach (var arObjectConfig in _lessonConfig.ObjectsToUse)
            {
                CreateArObject(arObjectConfig);
            }
        }

        private ARObjectBase CreateArObject(ARObjectConfig arObjectConfig)
        {
            var target = _vuforiaService.CreateTarget(arObjectConfig.VuforiaKey);
            
            var arObject = _gameFactory.CreateFromPrefab<ARObjectBase>(arObjectConfig.Prefab, target.transform);
            arObject.Initialize(arObjectConfig, _localizationService, _arCameraProvider.GetSpeaker(), target.GetComponent<ARObjectObserver>());
            
            _lessonObjects[target] = arObject;
            
            return arObject;
        }
    }
}