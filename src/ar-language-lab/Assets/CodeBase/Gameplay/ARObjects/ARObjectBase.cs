using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        public ARObjectObserver Observer => _observer;
        
        private ILocalizationService _localization;
        private ARObjectObserver _observer;
        private ARObjectConfig _data;
        private Speaker _speaker;

        public void Initialize(ARObjectConfig arObjectConfig, ILocalizationService localization, Speaker speaker, ARObjectObserver observer)
        {
            _data = arObjectConfig;
            _speaker = speaker;
            _localization = localization;
            _observer = observer;
            
            _observer.NearCameraEntered += NotifyNearCameraEntered;
            
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _observer.SetupRenderers();
        }
        
        public bool IsEqualTo(ARObjectConfig arObjectConfig) => 
            _data.Equals(arObjectConfig);

        public void Cleanup()
        {
            _observer.NearCameraEntered -= NotifyNearCameraEntered;
        }

        private void NotifyNearCameraEntered(float screenCoverage, float distanceToCameraMeters)
        {
            PlayLocalizedText().Forget();
        }

        private async UniTaskVoid PlayLocalizedText()
        {
            var text = await _localization.GetStringAsync(_data.LocalisationKey);
            await _speaker.SpeakAsync(text);
        }
    }
}