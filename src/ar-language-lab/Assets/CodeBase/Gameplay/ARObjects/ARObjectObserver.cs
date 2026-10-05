using System;
using CodeBase.Infrastructure.Vuforia;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeBase.Gameplay.ARObjects
{
    public class ARObjectObserver : DefaultObserverEventHandler
    {
        public event Action<float, float> NearCameraEntered;
        public event Action NearCameraExited;

        [Header("Coverage thresholds in viewport space [0..1]")]
        [SerializeField, Range(0.01f, 1f)] private float _enterCoverage = 0.5f;
        [SerializeField, Range(0.001f, 1f)] private float _exitCoverage = 0.35f;

        [Header("Trigger behavior")]
        [SerializeField] private bool _triggerOnlyOnce;

        [Header("Coverage source")]
        [SerializeField] private bool _preferCollidersForCoverage = true;
        [SerializeField] private bool _ignoreEffectRenderers = true;
        [SerializeField] private Renderer[] _coverageRenderersOverride;

        [Header("Debug")]
        [SerializeField] private bool _drawCoverageGizmos = true;
        [SerializeField] private Color _boundsGizmoColor = new Color(1f, 0.9f, 0.1f, 1f);
        [SerializeField] private Color _viewportRectGizmoColor = new Color(0.2f, 0.95f, 1f, 1f);

        private IARCameraProvider _cameraProvider;
        private readonly Vector3[] _corners = new Vector3[8];
        private Renderer[] _renderers;
        private Collider[] _colliders;

        private bool _isTracked;
        private bool _isNear;
        private bool _hasTriggered;

        private const float MinVisibleAlpha = 0.02f;
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        
        [Inject]
        private void Construct(IARCameraProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
            
            if (_exitCoverage >= _enterCoverage)
                _exitCoverage = Mathf.Max(0.001f, _enterCoverage * 0.8f);
        }

        public void SetupRenderers() => 
            SetupCoverageSources();

        public void ChangeEnterTrackingMaxDistance(float distance) => 
            _enterCoverage = Mathf.Clamp01(distance);
        
        public void ChangeExitTrackingMaxDistance(float distance) => 
            _exitCoverage = Mathf.Clamp01(distance);
        
        public void SetOneShotOnlyState(bool state) =>
            _triggerOnlyOnce = state;

        private void Awake() =>
            SetupCoverageSources();

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
                NearCameraExited?.Invoke();
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
            
            if (_isNear)
                NearCameraExited?.Invoke();
            
            _isNear = false;
        }
        
        private float CalculateViewportCoverage(Camera targetCamera)
        {
            if (!TryCalculateCoverageBounds(out var bounds))
            {
                return 0f;
            }

            if (!TryCalculateViewportRect(targetCamera, bounds, out var viewportRect))
            {
                return 0f;
            }

            return viewportRect.width * viewportRect.height;
        }

        private bool TryCalculateViewportRect(Camera targetCamera, Bounds bounds, out Rect viewportRect)
        {
            viewportRect = default;

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

            var pointCount = 0;
            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;

            for (var i = 0; i < _corners.Length; i++)
            {
                var viewportPoint = targetCamera.WorldToViewportPoint(_corners[i]);
                if (viewportPoint.z <= 0f)
                {
                    continue;
                }

                pointCount++;
                minX = Mathf.Min(minX, viewportPoint.x);
                maxX = Mathf.Max(maxX, viewportPoint.x);
                minY = Mathf.Min(minY, viewportPoint.y);
                maxY = Mathf.Max(maxY, viewportPoint.y);
            }

            if (pointCount < 2)
            {
                return false;
            }

            if (maxX <= 0f || minX >= 1f || maxY <= 0f || minY >= 1f)
            {
                return false;
            }

            minX = Mathf.Clamp01(minX);
            maxX = Mathf.Clamp01(maxX);
            minY = Mathf.Clamp01(minY);
            maxY = Mathf.Clamp01(maxY);

            var width = Mathf.Max(0f, maxX - minX);
            var height = Mathf.Max(0f, maxY - minY);
            if (width <= 0f || height <= 0f)
            {
                return false;
            }

            viewportRect = new Rect(minX, minY, width, height);
            return true;
        }

        private void SetupCoverageSources()
        {
            _renderers = _coverageRenderersOverride != null && _coverageRenderersOverride.Length > 0
                ? _coverageRenderersOverride
                : GetComponentsInChildren<Renderer>(includeInactive: false);

            _colliders = GetComponentsInChildren<Collider>(includeInactive: false);
        }

        private bool TryCalculateCoverageBounds(out Bounds bounds)
        {
            if (_renderers == null || _colliders == null)
            {
                SetupCoverageSources();
            }

            if (_preferCollidersForCoverage && TryBuildBoundsFromColliders(out bounds))
            {
                return true;
            }

            // 1) strict filtered renderers (ignores hidden/effects)
            if (TryBuildBoundsFromRenderers(out bounds, strictFiltering: true))
            {
                return true;
            }

            // 2) safety fallback: legacy behavior to avoid hard tracking loss
            return TryBuildBoundsFromRenderers(out bounds, strictFiltering: false);
        }

        private bool TryBuildBoundsFromColliders(out Bounds bounds)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            if (_colliders == null || _colliders.Length == 0)
            {
                return false;
            }

            var hasBounds = false;
            foreach (var colliderComponent in _colliders)
            {
                if (colliderComponent == null || !colliderComponent.enabled || !colliderComponent.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = colliderComponent.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(colliderComponent.bounds);
                }
            }

            return hasBounds;
        }

        private bool TryBuildBoundsFromRenderers(out Bounds bounds, bool strictFiltering)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            if (_renderers == null || _renderers.Length == 0)
            {
                return false;
            }

            var hasBounds = false;
            foreach (var rendererComponent in _renderers)
            {
                if (strictFiltering)
                {
                    if (!IsRendererEligible(rendererComponent))
                    {
                        continue;
                    }
                }
                else if (!IsRendererEligibleLegacy(rendererComponent))
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

            return hasBounds;
        }

        private static bool IsRendererEligibleLegacy(Renderer rendererComponent)
        {
            if (rendererComponent == null || !rendererComponent.enabled || !rendererComponent.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (rendererComponent.forceRenderingOff || rendererComponent.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
            {
                return false;
            }

            return true;
        }

        private bool IsRendererEligible(Renderer rendererComponent)
        {
            if (rendererComponent == null || !rendererComponent.enabled || !rendererComponent.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (rendererComponent.forceRenderingOff || rendererComponent.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
            {
                return false;
            }

            if (_ignoreEffectRenderers &&
                (rendererComponent is ParticleSystemRenderer || rendererComponent is LineRenderer || rendererComponent is TrailRenderer))
            {
                return false;
            }

            return HasVisibleMaterial(rendererComponent);
        }

        private static bool HasVisibleMaterial(Renderer rendererComponent)
        {
            var materials = rendererComponent.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                return true;
            }

            foreach (var material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty(BaseColorPropertyId))
                {
                    if (material.GetColor(BaseColorPropertyId).a > MinVisibleAlpha)
                    {
                        return true;
                    }

                    continue;
                }

                if (material.HasProperty(ColorPropertyId))
                {
                    if (material.color.a > MinVisibleAlpha)
                    {
                        return true;
                    }

                    continue;
                }

                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawCoverageGizmos)
            {
                return;
            }

            if (!TryCalculateCoverageBounds(out var bounds))
            {
                return;
            }

            var debugCamera = Camera.current;
            if (debugCamera == null)
            {
                return;
            }

            Gizmos.color = _boundsGizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            if (!TryCalculateViewportRect(debugCamera, bounds, out var viewportRect))
            {
                return;
            }

            var distance = Vector3.Dot(bounds.center - debugCamera.transform.position, debugCamera.transform.forward);
            if (distance <= debugCamera.nearClipPlane)
            {
                distance = debugCamera.nearClipPlane + 0.05f;
            }

            var p00 = debugCamera.ViewportToWorldPoint(new Vector3(viewportRect.xMin, viewportRect.yMin, distance));
            var p01 = debugCamera.ViewportToWorldPoint(new Vector3(viewportRect.xMin, viewportRect.yMax, distance));
            var p11 = debugCamera.ViewportToWorldPoint(new Vector3(viewportRect.xMax, viewportRect.yMax, distance));
            var p10 = debugCamera.ViewportToWorldPoint(new Vector3(viewportRect.xMax, viewportRect.yMin, distance));

            Handles.color = _viewportRectGizmoColor;
            Handles.DrawAAPolyLine(2f, p00, p01, p11, p10, p00);

            var coverage = viewportRect.width * viewportRect.height;
            var label = $"Coverage: {coverage:0.000}\\nEnter: {_enterCoverage:0.000}  Exit: {_exitCoverage:0.000}";
            Handles.Label(bounds.center + Vector3.up * Mathf.Max(bounds.extents.y, 0.05f), label);
        }
#endif
    }
}