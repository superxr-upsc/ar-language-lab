using System;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay.Lessons.Tasks.Resolvers
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

        public virtual void Run(ActiveTaskData viewData)
        {
            if (!TryResolveTargets())
            {
                CompleteTask();
                return;
            }

            CacheLocalizationData(viewData)
                .Forget();
        }

        private async UniTaskVoid CacheLocalizationData(ActiveTaskData viewData)
        {
            viewData.TaskDescription = await GetQuestDescription();
            viewData.TaskAudioClip = await _speaker.GenerateAudioClipAsync(viewData.TaskDescription);
        }

        public virtual void Dispose() { }

        protected virtual void CompleteTask() => 
            TaskCompleted?.Invoke(_taskData);

        protected virtual bool TryResolveTargets() => 
            false;

        protected virtual async UniTask<string> GetQuestDescription() => 
            string.Empty;
    }
}