using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.SpeechSyntesis
{
    public interface ITTSService
    {
        bool IsInitialized { get; }
        UniTask InitializeAsync();
        UniTask<AudioClip> GenerateAudioClip(string text);
    }
}