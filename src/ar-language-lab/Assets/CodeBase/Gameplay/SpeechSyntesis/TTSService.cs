using Eitan.Sherpa.Onnx.Unity.Mono.Components;
using System;
using System.Threading;
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
        private const int InitializationTimeoutSeconds = 45;

        private SpeechSynthesizerComponent _synthesizer;
        private UniTaskCompletionSource _initializeSource;
        private SherpaModelInstaller _modelInstaller;
        
        private bool _isInitialized;
        private int _initAttemptCounter;

        public bool IsInitialized => _isInitialized;

        [Inject]
        private void Construct()
        {
            _synthesizer = GetComponent<SpeechSynthesizerComponent>();
            _modelInstaller = new SherpaModelInstaller();

            AddSynthesizerListeners();
        }

        public async UniTask InitializeAsync()
        {
            var attemptId = Interlocked.Increment(ref _initAttemptCounter);

            if (_isInitialized)
                return;

            if (_initializeSource != null)
            {
                await AwaitInitializationWithTimeout(_initializeSource.Task, attemptId);
                return;
            }

            if (_synthesizer == null)
            {
                var exception = new InvalidOperationException("TTS synthesizer component is null. Ensure Construct was called and prefab has SpeechSynthesizerComponent.");
                GameLogger.LogError($"[TTSInit:{attemptId}] {exception.Message}");
                throw exception;
            }

            _initializeSource = new UniTaskCompletionSource();
            
            try
            {
                _synthesizer.ModelId = DefaultModelId;
                
                await _modelInstaller.EnsureInstalledAsync();

                if (!_synthesizer.TryLoadModule())
                {
                    throw new InvalidOperationException("Failed to start speech synthesizer module loading (TryLoadModule returned false).");
                }

                await AwaitInitializationWithTimeout(_initializeSource.Task, attemptId);
                GameLogger.Log($"[TTSInit:{attemptId}] Initialization completed.");
            }
            catch (OperationCanceledException)
            {
                GameLogger.LogError($"[TTSInit:{attemptId}] Initialization canceled.");
                throw;
            }
            catch (Exception exception)
            {
                GameLogger.LogError($"[TTSInit:{attemptId}] Initialization failed: {exception}");
                throw;
            }
            finally
            {
                _initializeSource = null;
            }
        }

        private static async UniTask AwaitInitializationWithTimeout(UniTask initializationTask, int attemptId)
        {
            var completedTaskIndex = await UniTask.WhenAny(initializationTask, UniTask.Delay(TimeSpan.FromSeconds(InitializationTimeoutSeconds)));
            if (completedTaskIndex != 0)
            {
                throw new TimeoutException($"[TTSInit:{attemptId}] Timeout after {InitializationTimeoutSeconds}s while waiting for synthesizer initialization callback.");
            }

            await initializationTask;
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

            if (_synthesizer != null)
            {
                _synthesizer.DisposeModule();
                RemoveSynthesizerListeners();
            }
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
            _isInitialized = false;
            _initializeSource?.TrySetException(exception);
            GameLogger.LogError($"[TTSService] {exception.Message}");
        }
    }
}