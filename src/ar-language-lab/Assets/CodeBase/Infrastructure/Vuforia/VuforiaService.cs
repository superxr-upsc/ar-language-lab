using System;
using CodeBase.Common.LoggerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuforia;

namespace CodeBase.Infrastructure.Vuforia
{
    public class VuforiaService : IVuforiaService, IDisposable
    {
        private readonly VuforiaApplication _application;
        private readonly VuforiaConfiguration _configuration;
        private VuforiaBehaviour _behaviour;

        private bool _isInitialized;
        private UniTaskCompletionSource _initializeSource;

        public VuforiaService()
        {
            _application = VuforiaApplication.Instance;
            _configuration = VuforiaConfiguration.Instance;
            
            SubscribeToVuforiaEvents();
        }

        public async UniTask InitializeVuforia()
        {
            if (_isInitialized)
                return;

            if (_initializeSource != null)
            {
                await _initializeSource.Task;
                return;
            }

            _initializeSource = new UniTaskCompletionSource();

            try
            {
                _application.Initialize();
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
            VuforiaBehaviour.Instance.CameraDevice.SetFlash(on);

        public bool SetDeviceFocusMode(FocusMode focusMode) => 
            VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(focusMode);

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
            _isInitialized = false;
            GameLogger.Log("Vuforia deinitialized!");
        }

        private void OnVuforiaInitialized(VuforiaInitError initError)
        {
            if (initError == VuforiaInitError.NONE)
            {
                _isInitialized = true;
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