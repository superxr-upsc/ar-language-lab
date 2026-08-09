using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.ProjectResourcesProvider;

namespace CodeBase.Gameplay.Lessons
{
    public class LessonManagementService : ILessonManagementService
    {
        private readonly IGameFactory _gameFactory;
        private readonly IProjectResourcesProvider _resourcesProvider;

        private LessonConfig _lessonConfig;
        
        public LessonManagementService(IGameFactory gameFactory,
            IProjectResourcesProvider resourcesProvider)
        {
            _gameFactory = gameFactory;
            _resourcesProvider = resourcesProvider;
        }

        public void SetupLesson()
        {
            _lessonConfig = GetSelectedLesson();

            SetupGameplayObjects();
        }

        private LessonConfig GetSelectedLesson()
        {
            // TODO : Should get selected lesson from main menu 
            return _resourcesProvider.LoadResource<LessonConfig>();
        }

        private void SetupGameplayObjects()
        {
            foreach (var arObjectConfig in _lessonConfig.ObjectsToUse) 
                _gameFactory.CreateFromPrefab<ARObjectBase>(arObjectConfig.Prefab);
        }
    }
}