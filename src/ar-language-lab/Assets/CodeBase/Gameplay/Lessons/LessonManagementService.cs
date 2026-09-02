using System;
using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.GameStateMachineService.States;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.Vuforia;
using Vuforia;

namespace CodeBase.Gameplay.Lessons
{
    public class LessonManagementService : ILessonManagementService, IDisposable
    {
        private readonly IGameFactory _gameFactory;
        private readonly IProjectResourcesProvider _resourcesProvider;
        private readonly IARCameraProvider _arCameraProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ISaveService _saveService;
        private readonly IGameStateMachine _gameStateMachine;

        private LessonConfig _lessonConfig;
        private LessonsGameDataProvider _lessonGameDataProvider;

        private Dictionary<MultiTargetBehaviour, ARObjectBase> _lessonObjects = new();

        private LessonTasksService _lessonTasksService;

        public LessonManagementService(IGameFactory gameFactory,
            IProjectResourcesProvider resourcesProvider,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService,
            IVuforiaService vuforiaService,
            ISaveService saveService,
            IGameStateMachine gameStateMachine)
        {
            _gameFactory = gameFactory;
            _resourcesProvider = resourcesProvider;
            _arCameraProvider = arCameraProvider;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
            _saveService = saveService;
            _gameStateMachine = gameStateMachine;
        }

        public void SetupLesson()
        {
            _lessonGameDataProvider = new LessonsGameDataProvider(_saveService);
            _lessonConfig = GetSelectedLesson();

            SetupGameplayObjects();
            SetupQuests();
        }

        public void StartLesson()
        {
            _lessonTasksService.SelectAndRunNewTask();
            _lessonTasksService.OnLessonComplete += OnLessonComplete;
        }

        public ARObjectBase GetObject(ARObjectConfig config)
        {
            foreach (var arObject in _lessonObjects.Values)
            {
                if (arObject.IsEqualTo(config))
                    return arObject;
            }

            return null;
        }

        public void CleanupLesson()
        {
            if (_lessonTasksService != null)
            {
                _lessonTasksService.OnLessonComplete -= OnLessonComplete;
                _lessonTasksService.Dispose();
                _lessonTasksService = null;
            }
            
            foreach (var arObject in _lessonObjects) 
                arObject.Value.Cleanup();
            
            _lessonObjects.Clear();
        }

        public void Dispose()
        {
            CleanupLesson();
        }

        private LessonConfig GetSelectedLesson()
        {
            var gameLessons = _resourcesProvider.LoadResource<GameLessons>();
            return gameLessons.GetLesson(_saveService.SaveData.Lessons.SelectedLessonID);
        }

        private void OnLessonComplete()
        {
            _vuforiaService.SetVuforiaState(false);
            _gameStateMachine.Enter<EnterMainMenuState>();
        }

        private void SetupGameplayObjects()
        {
            for (var index = 0; index < _lessonConfig.ObjectsToUse.Length; index++)
            {
                var arObjectConfig = _lessonConfig.ObjectsToUse[index];
                CreateArObject(arObjectConfig, index);
            }
        }

        private void SetupQuests()
        {
            _lessonTasksService = _gameFactory.Create<LessonTasksService>(_lessonConfig, _lessonGameDataProvider);
        }

        private ARObjectBase CreateArObject(ARObjectConfig arObjectConfig, int index)
        {
            var markerDatabaseName = MarkerNames.GetNameAtIndex(index);
            if (string.IsNullOrEmpty(markerDatabaseName))
                return null;
            
            var target = _vuforiaService.CreateTarget(markerDatabaseName);
            var arObject = _gameFactory.CreateFromPrefab<ARObjectBase>(arObjectConfig.Prefab, target.transform);
            arObject.Initialize(arObjectConfig, _localizationService, _arCameraProvider.GetSpeaker(), target.GetComponent<ARObjectObserver>());
            
            _lessonObjects[target] = arObject;
            
            return arObject;
        }
    }
}