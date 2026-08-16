using Eitan.Sherpa.Onnx.Unity.Mono.Components;
using System;
using CodeBase.Common.LoggerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.SpeechSyntesis
{
    [RequireComponent(typeof(SpeechSynthesizerComponent))]
    public class TTSService : MonoBehaviour, ITTSService
    {
        private const string DefaultModelId = "vits-piper-en_GB-sweetbbak-amy";
        
        private SpeechSynthesizerComponent _synthesizer;
        private UniTaskCompletionSource _initializeSource;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        [Inject]
        private void Construct()
        {
            _synthesizer = GetComponent<SpeechSynthesizerComponent>();
            
            AddSynthesizerListeners();
        }

        public async UniTask InitializeAsync()
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
                _synthesizer.ModelId = DefaultModelId;
                if (!_synthesizer.TryLoadModule())
                {
                    throw new InvalidOperationException("Failed to start speech synthesizer module loading.");
                }

                await _initializeSource.Task;
            }
            finally
            {
                _initializeSource = null;
            }
        }

        public async UniTask<AudioClip> GenerateAudioClip(string text)
        {
            if (_isInitialized)
                return await _synthesizer.GenerateClipAsync(text.Trim());
            
            GameLogger.LogWarning("TTS Service is not initialized. Cannot generate audio clip.");
            return null;

        }

        private void OnDestroy()
        {
            _initializeSource?.TrySetCanceled();
            
            _synthesizer.DisposeModule();
            RemoveSynthesizerListeners();
        }

        private void AddSynthesizerListeners()
        {
            _synthesizer.InitializationStateChangedEvent.AddListener(OnInitializationStateChanged);
            _synthesizer.SynthesisStartedEvent.AddListener(OnSynthesisStartedEvent);
            _synthesizer.SynthesisFailedEvent.AddListener(OnSynthesisFailedEvent);
            _synthesizer.ClipReadyEvent.AddListener(OnClipReadyEvent);
            _synthesizer.ErrorEvent.AddListener(OnErrorEvent);
        }

        private void RemoveSynthesizerListeners()
        {
            _synthesizer.InitializationStateChangedEvent.RemoveListener(OnInitializationStateChanged);
            _synthesizer.SynthesisStartedEvent.RemoveListener(OnSynthesisStartedEvent);
            _synthesizer.SynthesisFailedEvent.RemoveListener(OnSynthesisFailedEvent);
            _synthesizer.ClipReadyEvent.RemoveListener(OnClipReadyEvent);
            _synthesizer.ErrorEvent.RemoveListener(OnErrorEvent);
        }

        private void OnInitializationStateChanged(bool isReady)
        {
            if (!isReady)
                return;

            _isInitialized = true;
            _initializeSource?.TrySetResult();
        }

        private void OnSynthesisStartedEvent()
        {
            GameLogger.Log("[TTSService] Synthesis started.");
        }

        private void OnSynthesisFailedEvent(string error)
        {
            GameLogger.LogError($"[TTSService] Synthesis failed: {error}");
        }

        private void OnClipReadyEvent(AudioClip result)
        {
            GameLogger.Log($"[TTSService] Clip ready: {(result != null ? result.length.ToString("F2") : "null")}s");
        }

        private void OnErrorEvent(string error)
        {
            var exception = new InvalidOperationException($"Speech synthesizer error: {error}");
            _initializeSource?.TrySetException(exception);
            GameLogger.LogError($"[TTSService] {exception.Message}");
        }
    }
}