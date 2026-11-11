using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.Localization.Localisation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LocalizedTMPDropdown : MonoBehaviour
    {
        [SerializeField] private string[] _localizationIds;
        [SerializeField] private TMP_Dropdown _tmpDropdown;

        private ILocalizationService _localizationService;

        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        private void Awake()
        {
            if (_tmpDropdown == null)
                _tmpDropdown = GetComponent<TMP_Dropdown>();
        }

        private void OnEnable()
        {
            if (_localizationService == null)
                return;

            _localizationService.LocaleChanged += OnLocaleChanged;
            UpdateLocalizedOptions().Forget();
        }

        private void OnDisable()
        {
            if (_localizationService == null)
                return;

            _localizationService.LocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(string _)
        {
            UpdateLocalizedOptions().Forget();
        }

        private async UniTaskVoid UpdateLocalizedOptions()
        {
            if (_tmpDropdown == null || _localizationService == null || _localizationIds == null || _localizationIds.Length == 0)
                return;

            try
            {
                for (var i = 0; i < _localizationIds.Length; i++)
                {
                    var localizationId = _localizationIds[i];
                    if (string.IsNullOrWhiteSpace(localizationId))
                        continue;

                    var localizedText = await _localizationService.GetStringAsync(localizationId);

                    if (this == null || !isActiveAndEnabled)
                        return;

                    if (i < _tmpDropdown.options.Count)
                        _tmpDropdown.options[i].text = localizedText;
                    else
                        _tmpDropdown.options.Add(new TMP_Dropdown.OptionData(localizedText));
                }

                _tmpDropdown.RefreshShownValue();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to localize TMP_Dropdown '{name}'.\n{exception}", this);
            }
        }
    }
}
