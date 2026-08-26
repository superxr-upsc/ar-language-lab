using System;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class TaskResolverBase : IDisposable
    {
        protected readonly TaskData _taskData;

        public TaskResolverBase(TaskData taskData)
        {
            _taskData = taskData;
        }

        public event Action<TaskData> TaskCompleted;

        public virtual void Run()
        {
            
        }

        public virtual void Dispose()
        {
            
        }

        protected virtual void CompleteTask()
        {
            TaskCompleted?.Invoke(_taskData);
        }

        protected virtual void CheckTaskCompletion()
        {
            
        }
    }
}