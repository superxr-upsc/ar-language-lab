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

        public async UniTask<AudioClip> GenerateAudioClipAsync(string text)
        {
            await _operationLock.WaitAsync();
            
            try
            {
                if (_ttsService.IsInitialized)
                    return await _ttsService.GenerateAudioClip(text);
                
                GameLogger.LogWarning("TTS Service is not initialized. Cannot speak.");
                return null;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public async UniTask SpeakAsync(string text)
        {
            var clip = await GenerateAudioClipAsync(text);
            Speak(clip);
        }

        public void Speak(AudioClip clip)
        {
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
    }
}