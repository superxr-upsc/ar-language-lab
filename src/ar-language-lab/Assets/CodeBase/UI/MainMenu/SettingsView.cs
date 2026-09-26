using System;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class SettingsView : ViewBase
    {
        public Button CloseButton;
        public Toggle AutoFocusToggle => _autoFocusToggle;

        public Subject<string> LanguageSelected => _languageSelectedSubject;

        public LanguageToggle[] LanguageGroup => _languageGroup;
        
        [SerializeField] private LanguageToggle[] _languageGroup;
        [SerializeField] private Toggle _autoFocusToggle;

        private Subject<string> _languageSelectedSubject;
        
        private CompositeDisposable _disposable;
        
        public void Init()
        {
            _disposable = new CompositeDisposable();
            _languageSelectedSubject = new Subject<string>();
            
            foreach (var toggle in _languageGroup)
            {
                toggle.Toggle.OnValueChangedAsObservable()
                    .Subscribe(isOn => OnLanguageToggleSelected(toggle, isOn))
                    .AddTo(_disposable);
            }
        }

        private void OnLanguageToggleSelected(LanguageToggle toggle, bool isOn)
        {
            if (isOn == false)
                return;
            
            _languageSelectedSubject.OnNext(toggle.GetLanguageKey());
        }

        public void SetSelectedLanguage(string languageKey)
        {
            foreach (var languageToggle in _languageGroup)
            {
                if (languageToggle.ToggleEqualsTo(languageKey)) 
                    languageToggle.SetToggleOn();
            }
        }

        public void SetAutoFocusToggle(bool settingsAutoFocus)
        {
            _autoFocusToggle.isOn = settingsAutoFocus;
        }

        protected override void CloseWindowAnimation(Action resolve, Action<Exception> reject)
        {
            _disposable?.Dispose();
            
            base.CloseWindowAnimation(resolve, reject);
        }
    }
}