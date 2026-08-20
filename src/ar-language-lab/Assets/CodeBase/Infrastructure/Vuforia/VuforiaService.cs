using System;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.ARObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuforia;
using Zenject;

namespace CodeBase.Infrastructure.Vuforia
{
    public class VuforiaService : IVuforiaService, IDisposable
    {
        private const string DatabaseName = "ar-language-lab";

        private readonly IInstantiator _instantiator;
        private readonly VuforiaApplication _application;
        private readonly VuforiaConfiguration _configuration;

        private UniTaskCompletionSource _initializeSource;
        private VuforiaBehaviour _behaviour;

        public VuforiaService(IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _application = VuforiaApplication.Instance;
            _configuration = VuforiaConfiguration.Instance;
            
            _behaviour = _application.GetVuforiaBehaviour();
            
            SubscribeToVuforiaEvents();
        }

        public async UniTask InitializeVuforia()
        {
            if (_application.IsInitialized)
                return;

            if (_initializeSource != null)
            {
                await _initializeSource.Task;
                return;
            }

            _initializeSource = new UniTaskCompletionSource();

            try
            {
                if (_application.IsInitialized) 
                    _initializeSource.TrySetResult();

                await _initializeSource.Task;
            }
            finally
            {
                _initializeSource = null;
            }
        }

        //Vuforia Behaviour is placed on AR camera and should be setup after camera is created
        public void SetupVuforiaBehaviour()
        {
            _behaviour = _application.GetVuforiaBehaviour();
            SetRecomendedTargetFPS();
        }

        public World GetWorld() => 
            _behaviour.World;

        public bool SetDeviceFlashTorch(bool on) => 
            _behaviour.CameraDevice.SetFlash(on);

        public bool SetDeviceFocusMode(FocusMode focusMode) => 
            _behaviour.CameraDevice.SetFocusMode(focusMode);

        public MultiTargetBehaviour CreateTarget(string vuforiaKey)
        {
            var target = _behaviour.ObserverFactory.CreateMultiTarget(GetDatabasePath(), vuforiaKey);
            _instantiator.InstantiateComponent<ARObjectObserver>(target.gameObject);

            return target;
        }

        private string GetDatabasePath() => 
            $"Vuforia/{DatabaseName}.xml";

        public void Dispose()
        {
            _initializeSource?.TrySetCanceled();
            UnsubscribeFromVuforiaEvents();
        }

        private void OnVuforiaStopped()
        {
            GameLogger.Log("Vuforia stopped!");
        }

        private void OnVuforiaStarted()
        {
            GameLogger.Log("Vuforia started!");
        }

        private void OnVuforiaPaused(bool state)
        {
            GameLogger.Log("Vuforia paused!");
        }

        private void OnVuforiaError(VuforiaEngineError error)
        {
            GameLogger.LogError($"Vuforia error: {error}");
        }

        private void OnVuforiaDeinitialized()
        {
            GameLogger.Log("Vuforia deinitialized!");
        }

        private void OnVuforiaInitialized(VuforiaInitError initError)
        {
            if (initError == VuforiaInitError.NONE)
            {
                GameLogger.Log("Vuforia initialized without any errors!");
                _initializeSource?.TrySetResult();
                return;
            }

            var exception = new InvalidOperationException($"Vuforia not initialized, error: {initError}");
            GameLogger.LogError(exception.Message);
            _initializeSource?.TrySetException(exception);
        }

        private void OnBeforeVuforiaInitialized()
        {
            GameLogger.Log("Vuforia start initialization process!");
        }

        private void SetRecomendedTargetFPS()
        {
            var targetFps = _behaviour.CameraDevice.GetRecommendedFPS();
            if (targetFps <= 0)
                return;
            
            Application.targetFrameRate = targetFps;
            GameLogger.Log($"Vuforia recommended target FPS: {targetFps}");
        }

        private void SubscribeToVuforiaEvents()
        {
            _application.OnBeforeVuforiaInitialized += OnBeforeVuforiaInitialized;
            _application.OnVuforiaInitialized += OnVuforiaInitialized;
            _application.OnVuforiaDeinitialized += OnVuforiaDeinitialized;
            _application.OnVuforiaError += OnVuforiaError;
            _application.OnVuforiaPaused += OnVuforiaPaused;
            _application.OnVuforiaStarted += OnVuforiaStarted;
            _application.OnVuforiaStopped += OnVuforiaStopped;
        }
        
        private void UnsubscribeFromVuforiaEvents()
        {
            _application.OnBeforeVuforiaInitialized -= OnBeforeVuforiaInitialized;
            _application.OnVuforiaInitialized -= OnVuforiaInitialized;
            _application.OnVuforiaDeinitialized -= OnVuforiaDeinitialized;
            _application.OnVuforiaError -= OnVuforiaError;
            _application.OnVuforiaPaused -= OnVuforiaPaused;
            _application.OnVuforiaStarted -= OnVuforiaStarted;
            _application.OnVuforiaStopped -= OnVuforiaStopped;
        }
    }
}