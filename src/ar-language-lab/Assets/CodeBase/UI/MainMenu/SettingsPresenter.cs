using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using Cysharp.Threading.Tasks;
using R3;

namespace CodeBase.UI
{
    public class SettingsPresenter : PresenterBase
    {
        private readonly SettingsData _model;
        private readonly SettingsView _view;
        
        private readonly ISaveService _saveService;
        private readonly ILocalizationService _localizationService;

        private Dictionary<int, string> _languageCodeToLanguageName = new Dictionary<int, string>() 
            {
                { 0, "en" },
                { 1, "ru" },
                { 2, "ro" },
            };

        public SettingsPresenter(SettingsData data, SettingsView view,
            ISaveService saveService,
            ILocalizationService localizationService) : base(view)
        {
            _model = data;
            _view = view;
            _saveService = saveService;
            _localizationService = localizationService;

            var languageIndex = _languageCodeToLanguageName.FirstOrDefault(x => x.Value == _saveService.SaveData.Settings.Language).Key;
            _view.SetSelectedLanguage(languageIndex);
            _view.SetAutoFocusToggle(_saveService.SaveData.Settings.AutoFocus);
            
            _view.AutoFocusToggle
                .OnValueChangedAsObservable()
                .Subscribe(OnAutoFocusToggleChanged)
                .AddTo(_compositeDisposable);
            
            _view.LanguageDropdown
                .OnValueChangedAsObservable()
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

        private void OnLanguageDropdownChanged(int index)
        {
            var languageCode = _languageCodeToLanguageName[index];
            _saveService.SaveData.Settings.Language = languageCode;
            _saveService.MarkDirty();
            
            _localizationService.SetLocaleAsync(languageCode)
                .Forget();
        }

        protected override void ClearInstance()
        {
            _saveService.TrySaveIfDirtyAsync();
            
            base.ClearInstance();
        }
    }
}