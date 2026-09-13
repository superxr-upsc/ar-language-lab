using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class SettingsView : ViewBase
    {
        public Button CloseButton;
        public TMP_Dropdown LanguageDropdown => _languageDropdown;
        public Toggle AutoFocusToggle => _autoFocusToggle;

        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Toggle _autoFocusToggle;

        public void SetSelectedLanguage(int index)
        {
            if (index >= 0 && index < _languageDropdown.options.Count)
                _languageDropdown.value = index;
        }

        public void SetAutoFocusToggle(bool settingsAutoFocus)
        {
            _autoFocusToggle.isOn = settingsAutoFocus;
        }
    }
}