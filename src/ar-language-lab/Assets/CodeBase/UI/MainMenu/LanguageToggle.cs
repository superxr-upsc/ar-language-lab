using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class LanguageToggle : MonoBehaviour
    {
        public Toggle Toggle => _toggle;
        [SerializeField] private string languageId = "en";
        [SerializeField] private Toggle _toggle;

        public bool ToggleEqualsTo(string otherLanguageId) => 
            languageId == otherLanguageId;

        public void SetToggleOn() => _toggle.isOn = true;

        public string GetLanguageKey() => languageId;
    }
}