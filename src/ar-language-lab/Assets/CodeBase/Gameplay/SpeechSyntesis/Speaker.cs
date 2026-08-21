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
        private bool _isSpeaking;

        [Inject]
        private void Construct(ITTSService ttsService)
        {
            _ttsService = ttsService;
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void Speak(string text)
        {
            SpeakAsync(text).Forget();
        }

        public async UniTask SpeakAsync(string text)
        {
            if (_isSpeaking)
            {
                GameLogger.Log("SpeakAsync call ignored because another speech is already in progress.");
                return;
            }

            if (!_ttsService.IsInitialized)
            {
                GameLogger.LogWarning("TTS Service is not initialized. Cannot speak.");
                return;
            }

            _isSpeaking = true;
            try
            {
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
                _isSpeaking = false;
            }
        }
    }
}