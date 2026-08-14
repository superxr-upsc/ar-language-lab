using System;
using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons
{
    public class LessonManagementService : ILessonManagementService
    {
        private readonly IGameFactory _gameFactory;
        private readonly IProjectResourcesProvider _resourcesProvider;

        private LessonConfig _lessonConfig;
        
        private Transform _gameplayObjectsParent;
        private List<ARObjectBase> _lessonObjects = new();

        public LessonManagementService(IGameFactory gameFactory,
            IProjectResourcesProvider resourcesProvider)
        {
            _gameFactory = gameFactory;
            _resourcesProvider = resourcesProvider;
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
                arObject.Cleanup();
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
                var arObject = _gameFactory.CreateFromPrefab<ARObjectBase>(arObjectConfig.Prefab, _gameplayObjectsParent);
                arObject.Initialize();
                
                _lessonObjects.Add(arObject);
            }
        }
    }
}