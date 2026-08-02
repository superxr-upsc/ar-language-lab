using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace CodeBase.Gameplay.ARFoundation
{
    public class AROriginContainer : MonoBehaviour
    {
        public XROrigin Origin => _xROrigin;
        public ARTrackedImageManager TrackedImageManager => _trackedImageManager;
        
        [SerializeField] private XROrigin _xROrigin;
        [SerializeField] private ARTrackedImageManager _trackedImageManager;
    }
}