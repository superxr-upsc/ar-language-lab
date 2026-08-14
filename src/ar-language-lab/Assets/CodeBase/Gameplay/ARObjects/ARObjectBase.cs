using CodeBase.Common.LoggerService;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        [SerializeField] private ARObjectObserver _observer;
        
        private ARObjectConfig _data;
        
        public void Initialize()
        {
            _observer.NearCameraEntered += NotifyNearCameraEntered;
        }

        public void Cleanup()
        {
            _observer.NearCameraEntered -= NotifyNearCameraEntered;
            Destroy(gameObject);
        }

        private void NotifyNearCameraEntered(float screenCoverage, float distanceToCameraMeters)
        {
            GameLogger.Log("OBJECT IS NEAR!");
        }
    }
}