using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.UI
{
    public class SettingsPresenter : PresenterBase
    {
        private readonly SettingsData _model;
        private readonly SettingsView _view;
        
        private readonly ISaveService _saveService;
        private readonly ILocalizationService _localizationService;

        public SettingsPresenter(SettingsData data, SettingsView view,
            ISaveService saveService,
            ILocalizationService localizationService) : base(view)
        {
            _model = data;
            _view = view;
            _saveService = saveService;
            _localizationService = localizationService;

            _view.SetSelectedLanguage(_saveService.SaveData.Settings.Language);
            _view.SetAutoFocusToggle(_saveService.SaveData.Settings.AutoFocus);
            
            _view.Init();
            
            _view.AutoFocusToggle
                .OnValueChangedAsObservable()
                .Subscribe(OnAutoFocusToggleChanged)
                .AddTo(_compositeDisposable);
            
            _view.LanguageSelected
                .Subscribe(OnLanguageDropdownChanged)
                .AddTo(_compositeDisposable);
            
            _view.CloseButton.OnClickAsObservable()
                .Subscribe(_ => DisposeAsync())
                .AddTo(_compositeDisposable);
        }

        private void OnAutoFocusToggleChanged(bool isOn)
        {
            _saveService.SaveData.Settings.AutoFocus = isOn;
            _saveService.MarkDirty();
        }

        private void OnLanguageDropdownChanged(string languageId)
        {
            _saveService.SaveData.Settings.Language = languageId;
            _saveService.MarkDirty();
            
            _localizationService.SetLocaleAsync(languageId)
                .Forget();
        }

        protected override void ClearInstance()
        {
            _saveService.TrySaveIfDirtyAsync();
            
            base.ClearInstance();
        }
    }
}