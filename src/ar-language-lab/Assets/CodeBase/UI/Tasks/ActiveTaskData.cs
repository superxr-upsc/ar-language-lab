using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;
using UnityEngine;

namespace CodeBase.UI.Tasks
{
    public class ActiveTaskData : IModel
    {
        public string TaskDescription;
        public AudioClip TaskAudioClip;
        
        public ReactiveProperty<float> CurrentProgress { get; private set; } = new ReactiveProperty<float>();
        
        public void SetCurrentProgress(float progress) => 
            CurrentProgress.Value = progress;

        public void Cleanup()
        {
            TaskDescription = null;
            TaskAudioClip = null;
            SetCurrentProgress(0f);
        }
    }
}