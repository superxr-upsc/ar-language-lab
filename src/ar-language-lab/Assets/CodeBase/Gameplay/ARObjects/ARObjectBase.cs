using CodeBase.Gameplay.ARObjects.UI;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.WindowsManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        public ARObjectObserver Observer => _observer;

        private IWindowsManagementService _windowsManagementService;
        private ILocalizationService _localization;
        private ARObjectObserver _observer;
        private Speaker _speaker;
        
        private ARObjectPresenter _presenter;

        private ARObjectConfig _data;
        private ARObjectViewData _model;

        public void Initialize(ARObjectConfig arObjectConfig, 
            IWindowsManagementService windowsManagementService,
            ILocalizationService localization,
            ARObjectObserver observer,
            Speaker speaker)
        {
            _data = arObjectConfig;
            _speaker = speaker;
            _localization = localization;
            _observer = observer;
            _windowsManagementService = windowsManagementService;
            
            _observer.NearCameraEntered += NotifyNearCameraEntered;
            _observer.NearCameraExited += NotifyNearCameraExited;
            
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _model = new ARObjectViewData();

            _observer.SetupRenderers();
            
            CacheLicalization()
                .Forget();
        }

        private async UniTaskVoid CacheLicalization()
        {
            _model.Name = await _localization.GetStringAsync(_data.LocalisationKey);
            _model.AudioClip = await _speaker.GenerateAudioClipAsync(_model.Name);
        }

        public bool IsEqualTo(ARObjectConfig arObjectConfig) => 
            _data.Equals(arObjectConfig);

        public void Cleanup()
        {
            _observer.NearCameraEntered -= NotifyNearCameraEntered;
            _observer.NearCameraExited -= NotifyNearCameraExited;
        }

        private void NotifyNearCameraExited()
        {
            if (_presenter == null) 
                return;
            
            _presenter
                .Close()
                .Catch(UnityEngine.Debug.LogException);

            _presenter = null;
        }

        private void NotifyNearCameraEntered(float screenCoverage, float distanceToCameraMeters)
        {
            _presenter ??=
                _windowsManagementService.CreateWindow<ARObjectPresenter, ARObjectView, ARObjectViewData>(UILayer.InformationLayer,
                    _model);
        }
    }
}