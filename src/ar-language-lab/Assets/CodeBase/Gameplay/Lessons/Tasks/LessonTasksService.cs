using System;
using System.Collections.Generic;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks.Extensions;
using CodeBase.Gameplay.Lessons.Tasks.Resolvers;
using CodeBase.Infrastructure.GameFactory;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class LessonTasksService : IDisposable
    {

        public event Action OnLessonComplete;
        
        private readonly LessonConfig _lessonConfig;
        private readonly IGameFactory _gameFactory;
        private readonly LessonsGameDataProvider _lessonGameDataProvider;

        private Queue<TaskResolverBase> _taskList = new();
        private TaskResolverBase _currentTask;
        
        
        public LessonTasksService(LessonConfig lessonConfig, IGameFactory gameFactory, LessonsGameDataProvider lessonGameDataProvider)
        {
            _lessonConfig = lessonConfig;
            _gameFactory = gameFactory;
            _lessonGameDataProvider = lessonGameDataProvider;

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
            _currentTask.Run();
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
            _currentTask.TaskCompleted -= OnTaskCompleted;
            _lessonGameDataProvider.SaveCompletedTask(_lessonConfig.Id, taskData.Id);
            SelectAndRunNewTask();
        }
    }
}