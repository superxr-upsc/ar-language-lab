using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace CodeBase.UI.Tasks
{
    public class ActiveTaskPresenter : PresenterBase
    {
        private readonly ActiveTaskData _model;
        private readonly ActiveTaskView _viewBase;
        private readonly ILocalizationService _localizationService;
        private readonly Speaker _speaker;

        public ReactiveProperty<bool> Translated { get; }

        public ActiveTaskPresenter(ActiveTaskData model, ActiveTaskView viewBase,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService) : base(viewBase)
        {
            _model = model;
            _viewBase = viewBase;
            _localizationService = localizationService;
            _speaker = arCameraProvider.GetSpeaker();

            Translated = new ReactiveProperty<bool>(false);
            Translated.AddTo(_compositeDisposable);
            
            _viewBase.SetTaskDescription(model.TaskDescription);
            
            model.CurrentProgress
                .Subscribe(progress => viewBase.UpdateProgressBar(progress))
                .AddTo(_compositeDisposable);

            _viewBase.PlayAudioButton
                .OnClickAsObservable()
                .Subscribe(_ => PlayTaskDescriptionClip())
                .AddTo(_compositeDisposable);

            _viewBase.TranslateButton
                .OnClickAsObservable()
                .Subscribe(_ => TranslateTaskDescription())
                .AddTo(_compositeDisposable);
        }

        public void UpdateTaskDescription(string taskDescription) => 
            _viewBase.SetTaskDescription(taskDescription);
        
            private void PlayTaskDescriptionClip()
        {
            if (_model.TaskAudioClip != null) 
                _speaker.Speak(_model.TaskAudioClip);
        }

        private void TranslateTaskDescription() => 
            Translated.Value = !Translated.Value;
    }
}