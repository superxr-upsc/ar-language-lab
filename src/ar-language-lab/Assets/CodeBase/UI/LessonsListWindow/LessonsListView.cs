using System;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI.LessonsListWindow
{
    public class LessonsListView : ViewBase
    { 
        public Button CloseButton;
        public TMP_Dropdown PerspectiveDropDown;
        
        [SerializeField] private LessonDataView _lessonDataViewPrefab;
        [SerializeField] private Transform _lessonsListContainer;

        private IGameFactory _gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }
        
        public void Initialize(LessonConfig[] modelLessons, LessonsGameDataProvider lessonsProgress, Perspective perspective, Action<string> onLessonSelected)
        {
            PerspectiveDropDown.SetValueWithoutNotify((int)perspective);
            
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