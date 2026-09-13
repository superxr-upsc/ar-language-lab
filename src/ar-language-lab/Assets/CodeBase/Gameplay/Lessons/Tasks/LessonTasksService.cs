using System;
using System.Collections.Generic;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks.Extensions;
using CodeBase.Gameplay.Lessons.Tasks.Resolvers;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;
using R3;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class LessonTasksService : IDisposable
    {

        public event Action OnLessonComplete;
        
        private readonly LessonConfig _lessonConfig;
        private readonly IGameFactory _gameFactory;
        private readonly LessonsGameDataProvider _lessonGameDataProvider;
        private readonly IWindowsManagementService _windowsManagementService;
        private readonly ISaveService _saveService;

        private Queue<TaskResolverBase> _taskList = new();
        private TaskResolverBase _currentTask;
        
        private ActiveTaskData _activeTaskViewData;
        private ActiveTaskPresenter _activeTaskPresenter;
        
        public LessonTasksService(LessonConfig lessonConfig,
            IGameFactory gameFactory, 
            LessonsGameDataProvider lessonGameDataProvider,
            IWindowsManagementService windowsManagementService,
            ISaveService saveService)
        {
            _lessonConfig = lessonConfig;
            _gameFactory = gameFactory;
            _lessonGameDataProvider = lessonGameDataProvider;
            _windowsManagementService = windowsManagementService;
            _saveService = saveService;
            _activeTaskViewData = new ActiveTaskData();

            BuildTasksQuery();
        }

        public void SelectAndRunNewTask()
        {
            if (_taskList.Count == 0)
            {
                _lessonGameDataProvider.SaveCompletedLesson(_lessonConfig.Id);
                OnLessonComplete?.Invoke();
                return;
            }

            _currentTask = _taskList.Dequeue();

            _currentTask.TaskCompleted += OnTaskCompleted;
            _currentTask.Run(_activeTaskViewData);
            
            _activeTaskViewData.SetCurrentProgress(_lessonConfig.GetCompletedTasksValue(_lessonGameDataProvider.GetLastCompletedTaskId(_lessonConfig.Id)));
            
            CreateTaskView();
        }

        public void Dispose()
        {
            if (_currentTask != null)
            {
                _currentTask.TaskCompleted -= OnTaskCompleted;
                _currentTask.Dispose();
                _currentTask = null;
            }
            
            foreach (var taskResolver in _taskList) 
                taskResolver.Dispose();
            
            _taskList.Clear();
        }

        private void BuildTasksQuery()
        {
            _taskList = new Queue<TaskResolverBase>(_lessonConfig.Tasks.ToResolvers(_gameFactory));    
        }

        private void OnTaskCompleted(TaskData taskData)
        {
            CloseTaskView();
            
            _currentTask.TaskCompleted -= OnTaskCompleted;
            _lessonGameDataProvider.SaveCompletedTask(_lessonConfig.Id, taskData.Id);
            SelectAndRunNewTask();
        }

        private void CreateTaskView()
        {
            _activeTaskPresenter = _windowsManagementService
                .CreateWindow<ActiveTaskPresenter, ActiveTaskView, ActiveTaskData>(UILayer.NotificationLayer, _activeTaskViewData);
            
            _activeTaskPresenter.Translated
                .Subscribe(OnTaskTranslated)
                .AddTo(_activeTaskPresenter.Disposable);
        }

        private void OnTaskTranslated(bool isTranslated)
        {
            TranslateAsync(isTranslated)
                .Forget();
        }

        private async UniTaskVoid TranslateAsync(bool isTranslated)
        {
            var translation = string.Empty;
            if (isTranslated)
                translation = await _currentTask.GetQuestDescription(_saveService.SaveData.Settings.Language);
            else
                translation = await _currentTask.GetQuestDescription();

            _activeTaskPresenter.UpdateTaskDescription(translation);
        }
        
        private void CloseTaskView()
        {
            _activeTaskViewData.Cleanup();
            _activeTaskPresenter?.Close()
                .Catch(UnityEngine.Debug.LogException);
        }
    }
}