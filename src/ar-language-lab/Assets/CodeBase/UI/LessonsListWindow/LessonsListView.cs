using System;
using System.Collections.Generic;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.SaveLoad.Data;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI.LessonsListWindow
{
    public class LessonsListView : ViewBase
    { 
        public Button CloseButton;
        
        [SerializeField] private LessonDataView _lessonDataViewPrefab;
        [SerializeField] private Transform _lessonsListContainer;

        private IGameFactory _gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        
        public void Initialize(LessonConfig[] modelLessons, LessonsGameDataProvider lessonsProgress, Action<string> onLessonSelected)
        {
            CreateLessonsList(modelLessons, lessonsProgress, onLessonSelected);
        }

        private void CreateLessonsList(LessonConfig[] modelLessons, LessonsGameDataProvider lessonsProgress, Action<string> onLessonSelected)
        {
            foreach (var lessonConfig in modelLessons)
            {
                var lessonProgress = lessonsProgress.GetProgressById(lessonConfig.Id);
                var lessonDataView = _gameFactory.CreateFromPrefab<LessonDataView>(_lessonDataViewPrefab, _lessonsListContainer);
                lessonDataView.Initialize(lessonConfig, lessonProgress, onLessonSelected);
            }
        }
    }
}