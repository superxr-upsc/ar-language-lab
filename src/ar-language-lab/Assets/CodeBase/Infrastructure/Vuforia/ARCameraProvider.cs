using CodeBase.Gameplay.SpeechSyntesis;
using UnityEngine;

namespace CodeBase.Infrastructure.Vuforia
{
    public class ARCameraProvider : IARCameraProvider
    {
        private ARCamera _cachedCamera;

        public Speaker GetSpeaker() => GetCachedArCamera().Speaker;
        public Camera GetActiveCamera() => GetCachedArCamera().Camera;
        public ARCamera GetActiveARCamera() => GetCachedArCamera();

        private ARCamera GetCachedArCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
            {
                return _cachedCamera;
            }

            _cachedCamera = Camera.main.GetComponent<ARCamera>();
            if (_cachedCamera != null)
            {
                return _cachedCamera;
            }

            _cachedCamera = Object.FindAnyObjectByType<ARCamera>();
            return _cachedCamera;
        }
    }
}

