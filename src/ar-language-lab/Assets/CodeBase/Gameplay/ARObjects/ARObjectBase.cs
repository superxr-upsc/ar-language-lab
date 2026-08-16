using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        [SerializeField] private ARObjectObserver _observer;

        private ILocalizationService _localization;
        private ARObjectConfig _data;
        private Speaker _speaker;

        public void Initialize(ARObjectConfig arObjectConfig, ILocalizationService localization, Speaker speaker)
        {
            _data = arObjectConfig;
            _speaker = speaker;
            _localization = localization;
            
            _observer.NearCameraEntered += NotifyNearCameraEntered;
        }

        public void Cleanup()
        {
            _observer.NearCameraEntered -= NotifyNearCameraEntered;
            Destroy(gameObject);
        }

        private void NotifyNearCameraEntered(float screenCoverage, float distanceToCameraMeters)
        {
            PlayLocalizedText().Forget();
        }

        private async UniTaskVoid PlayLocalizedText()
        {
            var text = await _localization.GetStringAsync(_data.LocalisationKey);
            _speaker.SpeckAsync(text).Forget();
        }
    }
}