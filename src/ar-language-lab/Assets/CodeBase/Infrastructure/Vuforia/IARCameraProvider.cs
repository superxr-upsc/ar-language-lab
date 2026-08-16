using CodeBase.Gameplay.SpeechSyntesis;
using UnityEngine;

namespace CodeBase.Infrastructure.Vuforia
{
    public interface IARCameraProvider
    {
        Camera GetActiveCamera();
        Speaker GetSpeaker();
        ARCamera GetActiveARCamera();
    }
}

