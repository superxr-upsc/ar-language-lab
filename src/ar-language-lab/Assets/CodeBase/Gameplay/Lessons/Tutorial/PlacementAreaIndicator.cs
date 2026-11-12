using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tutorial
{
    public class PlacementAreaIndicator : MonoBehaviour
    {
        [SerializeField] private float _pulseSpeed = 3f;
        [SerializeField] private float _pulseAmount = 0.08f;

        private Vector3 _baseScale;
        private Material _lineMaterial;
        private Material _discMaterial;
        private Mesh _discMesh;

        public static PlacementAreaIndicator Create(Transform parent, Vector3 localPosition, float radius = 0.12f)
        {
            var go = new GameObject("PlacementAreaIndicator");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var indicator = go.AddComponent<PlacementAreaIndicator>();
            indicator.SetupVisuals(radius);
            return indicator;
        }

        private void SetupVisuals(float radius)
        {
            _baseScale = Vector3.one;

            var shader = Shader.Find("Universal Render Pipeline/Unlit") 
                         ?? Shader.Find("Sprites/Default") 
                         ?? Shader.Find("Unlit/Color");

            // LineRenderer for the glowing border ring
            var lineObj = new GameObject("Ring");
            lineObj.transform.SetParent(transform, false);
            lineObj.transform.localPosition = Vector3.zero;

            var lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = 36;
            lineRenderer.startWidth = 0.015f;
            lineRenderer.endWidth = 0.015f;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            _lineMaterial = new Material(shader)
            {
                color = new Color(0.15f, 0.85f, 1f, 0.95f)
            };
            lineRenderer.material = _lineMaterial;

            for (var i = 0; i < 36; i++)
            {
                var angle = i * Mathf.PI * 2f / 36f;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.005f, Mathf.Sin(angle) * radius));
            }

            // Procedural flat circle mesh for the highlighted inner ground area
            var discObj = new GameObject("Disc");
            discObj.transform.SetParent(transform, false);
            discObj.transform.localPosition = Vector3.zero;

            var meshFilter = discObj.AddComponent<MeshFilter>();
            var meshRenderer = discObj.AddComponent<MeshRenderer>();

            _discMesh = CreateCircleMesh(radius, 36);
            meshFilter.sharedMesh = _discMesh;

            _discMaterial = new Material(shader)
            {
                color = new Color(0.15f, 0.85f, 1f, 0.25f)
            };
            meshRenderer.material = _discMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private static Mesh CreateCircleMesh(float radius, int segments)
        {
            var mesh = new Mesh { name = "PlacementDiscMesh" };
            var vertices = new Vector3[segments + 1];
            var uvs = new Vector2[segments + 1];
            var triangles = new int[segments * 3];

            vertices[0] = new Vector3(0f, 0.002f, 0f);
            uvs[0] = new Vector2(0.5f, 0.5f);

            for (var i = 0; i < segments; i++)
            {
                var angle = i * Mathf.PI * 2f / segments;
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);

                vertices[i + 1] = new Vector3(cos * radius, 0.002f, sin * radius);
                uvs[i + 1] = new Vector2(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);

                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i == segments - 1 ? 1 : i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void Update()
        {
            var scale = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
            transform.localScale = new Vector3(_baseScale.x * scale, _baseScale.y, _baseScale.z * scale);
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
                Destroy(_lineMaterial);

            if (_discMaterial != null)
                Destroy(_discMaterial);

            if (_discMesh != null)
                Destroy(_discMesh);
        }
    }
}
