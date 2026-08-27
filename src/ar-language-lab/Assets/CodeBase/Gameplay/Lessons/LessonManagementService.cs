using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.Vuforia;
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

        private Dictionary<MultiTargetBehaviour, ARObjectBase> _lessonObjects = new();
        
        private LessonTasksService _lessonTasksService;

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

            SetupGameplayObjects();
            SetupQuests();
        }

        public void StartLesson()
        {
            _lessonTasksService.SelectAndRunNewTask();
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
            _lessonTasksService.Dispose();
            _lessonTasksService = null;
            
            foreach (var arObject in _lessonObjects) 
                arObject.Value.Cleanup();
            
            _lessonObjects.Clear();
        }

        private LessonConfig GetSelectedLesson()
        {
            // TODO : Should get selected lesson from main menu 
            return _resourcesProvider.LoadResource<LessonConfig>();
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
            _lessonTasksService = _gameFactory.Create<LessonTasksService>(_lessonConfig);
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