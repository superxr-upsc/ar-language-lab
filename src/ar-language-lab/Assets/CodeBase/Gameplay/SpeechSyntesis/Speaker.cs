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

        [Inject]
        private void Construct(ITTSService ttsService)
        {
            _ttsService = ttsService;
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void Speak(string text)
        {
            SpeckAsync(text).Forget();
        }

        public async UniTaskVoid SpeckAsync(string text)
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
    }
}