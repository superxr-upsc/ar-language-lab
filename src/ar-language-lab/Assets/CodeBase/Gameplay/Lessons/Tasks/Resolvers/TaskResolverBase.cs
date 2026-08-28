using System;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class TaskResolverBase : IDisposable
    {
        protected readonly TaskData _taskData;
        
        private readonly Speaker _speaker;

        protected TaskResolverBase(TaskData taskData, IARCameraProvider cameraProvider)
        {
            _taskData = taskData;
            _speaker = cameraProvider.GetSpeaker();
        }

        public event Action<TaskData> TaskCompleted;

        public virtual void Run()
        {
            if (!TryResolveTargets())
            {
                CompleteTask();
                return;
            }
            
            PlayQuestDescriptionForQuest()
                .Forget();
        }
        
        public virtual void Dispose() { }

        protected virtual void CompleteTask() => 
            TaskCompleted?.Invoke(_taskData);

        protected virtual bool TryResolveTargets() => 
            false;

        protected virtual async UniTask<string> GetQuestDescription() => 
            string.Empty;
        
        private async UniTaskVoid PlayQuestDescriptionForQuest()
        {
            var questDescription = await GetQuestDescription();
            await _speaker.SpeakAsync(questDescription);
        }
    }
}