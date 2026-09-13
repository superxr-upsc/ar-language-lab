using CodeBase.Infrastructure.Localization;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Localization.Localisation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPText : MonoBehaviour
    {
        [SerializeField] private string _localizationId;
        [SerializeField] private TMP_Text _tmpText;

        private ILocalizationService _localizationService;

        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        private void Awake()
        {
            if (_tmpText == null)
                _tmpText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (_localizationService == null)
                return;

            _localizationService.LocaleChanged += OnLocaleChanged;
            UpdateLocalizedText().Forget();
        }

        private void OnDisable()
        {
            if (_localizationService == null)
                return;

            _localizationService.LocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(string _)
        {
            UpdateLocalizedText().Forget();
        }

        private async UniTaskVoid UpdateLocalizedText()
        {
            if (_tmpText == null || _localizationService == null || string.IsNullOrWhiteSpace(_localizationId))
                return;

            try
            {
                var localizedText = await _localizationService.GetStringAsync(_localizationId);

                if (this == null || !isActiveAndEnabled)
                    return;

                _tmpText.text = localizedText;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Failed to localize TMP_Text '{name}' with key '{_localizationId}'.\n{exception}", this);
            }
        }
    }
}

