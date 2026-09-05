using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.UI.Tasks
{
    public class ActiveTaskPresenter : PresenterBase
    {
        private readonly ActiveTaskData _model;
        private readonly Speaker _speaker;

        public ActiveTaskPresenter(ActiveTaskData model, ActiveTaskView viewBase, IARCameraProvider arCameraProvider) : base(viewBase)
        {
            _model = model;
            _speaker = arCameraProvider.GetSpeaker();
            
            viewBase.SetTaskDescription(model.TaskDescription);
            
            model.CurrentProgress
                .Subscribe(progress => viewBase.UpdateProgressBar(progress))
                .AddTo(_compositeDisposable);

            viewBase.PlayAudioButton
                .OnClickAsObservable()
                .Subscribe(_ => PlayTaskDescriptionClip())
                .AddTo(_compositeDisposable);
        }
        
        private void PlayTaskDescriptionClip()
        {
            if (_model.TaskAudioClip != null) 
                _speaker.Speak(_model.TaskAudioClip);
        }
    }
}