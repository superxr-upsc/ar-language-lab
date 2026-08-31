using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.Infrastructure.Loading.UI
{
    public class LoadingScreenData : IModel
    {
        public ReactiveProperty<string> CurrentProgress { get; private set; } = new ReactiveProperty<string>();
        public ReactiveProperty<float> CurrentProgressStep { get; private set; } = new ReactiveProperty<float>();
        
        public void SetCurrentProgress(string progress) => 
            CurrentProgress.Value = progress;
        
        public void SetCurrentProgress(float progress) => 
            CurrentProgressStep.Value = progress;
        
    }
}