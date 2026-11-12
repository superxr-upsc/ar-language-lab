using System.Threading;
using CodeBase.Gameplay.ARObjects.UI;
using CodeBase.Gameplay.Lessons.Tasks;
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
        public ARObjectConfig Config => _data;
        public string LocalizedName => _model?.Name;
        public GameObject _objectView;
        public GameObject _outlineObject;
        public Material _invisibleMaterial;

        private IWindowsManagementService _windowsManagementService;
        private ILocalizationService _localization;
        private ARObjectObserver _observer;
        private Speaker _speaker;

        private ARObjectPresenter _presenter;

        private ARObjectConfig _data;
        private ARObjectViewData _model;
        private Perspective _perspective;

        public void Initialize(ARObjectConfig arObjectConfig, 
            IWindowsManagementService windowsManagementService,
            ILocalizationService localization,
            ARObjectObserver observer,
            Speaker speaker,
            Perspective perspective)
        {
            _data = arObjectConfig;
            _speaker = speaker;
            _localization = localization;
            _observer = observer;
            _perspective = perspective;
            _windowsManagementService = windowsManagementService;
            
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _model = new ARObjectViewData();

            SetOutlineState(false);
            _observer.SetupRenderers();
        }

        public async UniTask CacheLocalizationAsync(CancellationToken cancellationToken = default)
        {
            _model.LocalizationKey = _data.LocalisationKey;
            _model.Name = await _localization.GetStringAsync(
                _data.LocalisationKey,
                LocalizationConsts.DefaultLocaleCode,
                LocalizationConsts.DefaultStringTableName);

            cancellationToken.ThrowIfCancellationRequested();

            _model.AudioClip = await _speaker.GenerateAudioClipAsync(_model.Name, cancellationToken);
        }

        public bool IsEqualTo(ARObjectConfig arObjectConfig) => 
            _data.Equals(arObjectConfig);

        public void SubscribeToNotifyEvents()
        {
            _observer.NearCameraEntered += NotifyNearCameraEntered;
            _observer.NearCameraExited += NotifyNearCameraExited;
        }

        public void SetOutlineState(bool isActive)
        {
            if (_outlineObject != null)
                _outlineObject.SetActive(isActive);
        }

        public float PlayNameAudio()
        {
            if (_model?.AudioClip == null)
                return 0f;

            _speaker.Speak(_model.AudioClip);
            return _model.AudioClip.length;
        }

        public void Cleanup()
        {
            _observer.NearCameraEntered -= NotifyNearCameraEntered;
            _observer.NearCameraExited -= NotifyNearCameraExited;
            SetOutlineState(false);
        }

        public void HideObjectViewForAudioPerspective()
        {
            var renderers = _objectView.GetComponentsInChildren<Renderer>(false);
            foreach (var renderer in renderers)
            {
                var invisibleMaterials = new Material[renderer.sharedMaterials.Length];
                
                for (int i = 0; i < invisibleMaterials.Length; i++) 
                    invisibleMaterials[i] = _invisibleMaterial;
                
                renderer.sharedMaterials = invisibleMaterials;
            }
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

            if (_perspective is Perspective.Audio or Perspective.Mix)
                _speaker.Speak(_model.AudioClip);
        }
    }
}
