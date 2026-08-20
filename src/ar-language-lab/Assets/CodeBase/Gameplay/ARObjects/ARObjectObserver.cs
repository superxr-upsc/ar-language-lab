using System;
using CodeBase.Infrastructure.Vuforia;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectObserver : DefaultObserverEventHandler
    {
        public event Action<float, float> NearCameraEntered;

        [Header("Coverage thresholds in viewport space [0..1]")]
        [SerializeField, Range(0.01f, 1f)] private float _enterCoverage = 0.2f;
        [SerializeField, Range(0.001f, 1f)] private float _exitCoverage = 0.15f;

        [Header("Trigger behavior")]
        [SerializeField] private bool _triggerOnlyOnce;

        private IARCameraProvider _cameraProvider;
        private readonly Vector3[] _corners = new Vector3[8];
        private Renderer[] _renderers;
        private Camera _camera;

        private bool _isTracked;
        private bool _isNear;
        private bool _hasTriggered;
        
        [Inject]
        private void Construct(IARCameraProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
            
            if (_exitCoverage >= _enterCoverage)
                _exitCoverage = Mathf.Max(0.001f, _enterCoverage * 0.8f);
        }

        public void SetupRenderers() => 
            _renderers = GetComponentsInChildren<Renderer>(includeInactive: false);

        private void Update()
        {
            if (!_isTracked)
            {
                _isNear = false;
                return;
            }

            if (_triggerOnlyOnce && _hasTriggered)
            {
                return;
            }

            var targetCamera = _cameraProvider.GetActiveCamera();
            if (targetCamera == null)
            {
                return;
            }

            var coverage = CalculateViewportCoverage(targetCamera);

            if (!_isNear)
            {
                if (coverage < _enterCoverage)
                {
                    return;
                }

                _isNear = true;
                _hasTriggered = true;

                var distance = Vector3.Distance(targetCamera.transform.position, transform.position);
                NearCameraEntered?.Invoke(coverage, distance);
                return;
            }

            if (coverage <= _exitCoverage)
            {
                _isNear = false;
            }
        }
        
        protected override void OnTrackingFound()
        {
            base.OnTrackingFound();

            _isTracked = true;
        }

        protected override void OnTrackingLost()
        {
            base.OnTrackingLost();
            
            _isTracked = false;
            _isNear = false;
        }
        
        private float CalculateViewportCoverage(Camera targetCamera)
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                return 0f;
            }

            var hasBounds = false;
            var bounds = new Bounds(transform.position, Vector3.zero);

            foreach (var rendererComponent in _renderers)
            {
                if (rendererComponent == null || !rendererComponent.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = rendererComponent.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(rendererComponent.bounds);
                }
            }

            if (!hasBounds)
            {
                return 0f;
            }

            var extents = bounds.extents;
            var center = bounds.center;

            _corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            _corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
            _corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            _corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
            _corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
            _corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            _corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
            _corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

            var visiblePoints = 0;
            var minX = 1f;
            var maxX = 0f;
            var minY = 1f;
            var maxY = 0f;

            for (var i = 0; i < _corners.Length; i++)
            {
                var viewportPoint = targetCamera.WorldToViewportPoint(_corners[i]);
                if (viewportPoint.z <= 0f)
                {
                    continue;
                }

                visiblePoints++;
                minX = Mathf.Min(minX, viewportPoint.x);
                maxX = Mathf.Max(maxX, viewportPoint.x);
                minY = Mathf.Min(minY, viewportPoint.y);
                maxY = Mathf.Max(maxY, viewportPoint.y);
            }

            if (visiblePoints < 2)
            {
                return 0f;
            }

            minX = Mathf.Clamp01(minX);
            maxX = Mathf.Clamp01(maxX);
            minY = Mathf.Clamp01(minY);
            maxY = Mathf.Clamp01(maxY);

            var width = Mathf.Max(0f, maxX - minX);
            var height = Mathf.Max(0f, maxY - minY);
            return width * height;
        }
    }
}