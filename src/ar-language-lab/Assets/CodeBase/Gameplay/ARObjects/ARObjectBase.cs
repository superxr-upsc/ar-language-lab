using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectBase : MonoBehaviour
    {
        [SerializeField] private ARObjectConfig _data;
        
        public void Initialize(ARTrackedImage trackedImage)
        {
        }

        public void Refresh(ARTrackedImage trackedImage)
        {
        }

        public void Cleanup()
        {
            Destroy(gameObject);
        }
    }
}