using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using Cysharp.Threading.Tasks;
using R3;

namespace CodeBase.Gameplay.ARObjects.UI
{
    public class ARObjectPresenter : PresenterBase
    {
        private readonly Speaker _speaker;
        private readonly ARObjectViewData _model;
        private readonly ARObjectView _view;
        private readonly ILocalizationService _localizationService;

        private bool _translated = false;

        public ARObjectPresenter(ARObjectViewData model, ARObjectView view,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService) : base(view)
        {
            _speaker = arCameraProvider.GetSpeaker();
            _model = model;
            _view = view;
            _localizationService = localizationService;

            _view.SetObjectName(_model.Name);
            
            _view.PlayAudioButton.OnClickAsObservable()
                .Subscribe(_ => PlayObjectNameClip())
                .AddTo(_compositeDisposable);
            
            _view.TranslateButton.OnClickAsObservable()
                .Subscribe(_ => TranslateObject())
                .AddTo(_compositeDisposable);
        }

        private void TranslateObject()
        {
            TranslateAsync()
                .Forget();
        }

        private async UniTaskVoid TranslateAsync()
        {
            var translation = string.Empty;
            if (_translated)
                translation = await _localizationService.GetStringAsync(_model.LocalizationKey,
                    LocalizationConsts.DefaultLocaleCode, LocalizationConsts.DefaultStringTableName);
            else
                translation = await _localizationService.GetStringAsync(_model.LocalizationKey);

            _translated = !_translated;
            _view.SetObjectName(translation);
        }

        private void PlayObjectNameClip()
        {
            if (_model.AudioClip != null) 
                _speaker.Speak(_model.AudioClip);
        }
    }
}