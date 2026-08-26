using System;
using System.Collections.Generic;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.Lessons.Tasks.Extensions;
using CodeBase.Infrastructure.GameFactory;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class LessonTasksService : IDisposable
    {
        private readonly LessonConfig _lessonConfig;
        private readonly IGameFactory _gameFactory;

        private Queue<TaskResolverBase> _taskList = new();
        private TaskResolverBase _currentTask;
        
        
        public LessonTasksService(LessonConfig lessonConfig, IGameFactory gameFactory)
        {
            _lessonConfig = lessonConfig;
            _gameFactory = gameFactory;

            BuildTasksQuery();
        }

        public void SelectAndRunNewTask()
        {
            if (_taskList.Count == 0)
            {
                GameLogger.Log("All tasks completed!");
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
            SelectAndRunNewTask();
        }
    }
}