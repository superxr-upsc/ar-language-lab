using System.Threading;
using CodeBase.Common.LoggerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.SpeechSyntesis
{
    [RequireComponent(typeof(AudioSource))]
    public class Speaker : MonoBehaviour
    {
        private AudioSource _audioSource;
        private ITTSService _ttsService;
        
        private readonly SemaphoreSlim _operationLock = new(1, 1);

        [Inject]
        private void Construct(ITTSService ttsService)
        {
            _ttsService = ttsService;
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void Speak(string text)
        {
            SpeakAsync(text)
                .Forget();
        }

        public async UniTask SpeakAsync(string text)
        {
            await _operationLock.WaitAsync();
            
            try
            {
                if (!_ttsService.IsInitialized)
                {
                    GameLogger.LogWarning("TTS Service is not initialized. Cannot speak.");
                    return;
                }
                
                var clip = await _ttsService.GenerateAudioClip(text);
                if (clip != null)
                {
                    _audioSource.Stop();
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
                else
                {
                    GameLogger.LogWarning("Failed to generate audio clip for the given text.");
                }
            }
            finally
            {
                _operationLock.Release();
            }
        }
    }
}