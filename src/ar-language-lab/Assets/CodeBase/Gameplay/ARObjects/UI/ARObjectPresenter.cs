using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.Gameplay.ARObjects.UI
{
    public class ARObjectPresenter : PresenterBase
    {
        private readonly Speaker _speaker;
        private ARObjectViewData _model;

        public ARObjectPresenter(ARObjectViewData model, ARObjectView view,
            IARCameraProvider arCameraProvider) : base(view)
        {
            _speaker = arCameraProvider.GetSpeaker();
            _model = model;
            
            view.SetObjectName(model.Name);
            view.PlayAudioButton.OnClickAsObservable()
                .Subscribe(_ => PlayObjectNameClip())
                .AddTo(_compositeDisposable);
        }

        private void PlayObjectNameClip()
        {
            if (_model.AudioClip != null) 
                _speaker.Speak(_model.AudioClip);
        }
    }
}