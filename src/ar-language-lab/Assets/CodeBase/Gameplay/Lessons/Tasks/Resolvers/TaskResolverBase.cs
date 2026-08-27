using System;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public abstract class TaskResolverBase : IDisposable
    {
        protected readonly TaskData _taskData;

        protected TaskResolverBase(TaskData taskData) => 
            _taskData = taskData;

        public event Action<TaskData> TaskCompleted;

        public abstract void Run();

        public abstract void Dispose();

        protected abstract bool IsTaskComplete();

        protected virtual void CompleteTask()
        {
            if (IsTaskComplete())
                TaskCompleted?.Invoke(_taskData);
        }
    }
}