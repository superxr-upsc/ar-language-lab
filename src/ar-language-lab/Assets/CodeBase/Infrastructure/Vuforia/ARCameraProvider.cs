using UnityEngine;
using Vuforia;

namespace CodeBase.Infrastructure.Vuforia
{
    public class ARCameraProvider : IARCameraProvider
    {
        private Camera _cachedCamera;

        public Camera GetActiveCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
            {
                return _cachedCamera;
            }

            _cachedCamera = Camera.main;
            if (_cachedCamera != null)
            {
                return _cachedCamera;
            }

            var vuforiaBehaviour = VuforiaBehaviour.Instance;
            if (vuforiaBehaviour != null)
            {
                _cachedCamera = vuforiaBehaviour.GetComponent<Camera>();
                if (_cachedCamera != null)
                {
                    return _cachedCamera;
                }
            }

            _cachedCamera = Object.FindObjectOfType<Camera>();
            return _cachedCamera;
        }
    }
}

