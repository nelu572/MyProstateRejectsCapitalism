using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyProstateRejectsCapitalism.Gameplay
{
    internal sealed class ExhibitionFocusOutline
    {
        private static readonly int Ink = Shader.PropertyToID("_Ink");
        private static readonly int Width = Shader.PropertyToID("_WidthPx");
        private static readonly int Offset = Shader.PropertyToID("_OffsetPx");
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        private readonly GameObject root;
        private readonly Mesh silhouette;
        private readonly Material ink;
        private readonly Material echo;

        internal ExhibitionFocusOutline(Transform exhibit, MeshFilter[] sources, Shader shader,
            float width, float echoWidth, Vector2 echoOffset, bool smoothSeams = false)
        {
            var parts = new List<CombineInstance>();
            foreach (var source in sources)
            {
                var mesh = source.sharedMesh;
                var local = exhibit.worldToLocalMatrix * source.transform.localToWorldMatrix;
                for (int i = 0; i < mesh.subMeshCount; i++)
                    parts.Add(new CombineInstance { mesh = mesh, subMeshIndex = i, transform = local });
            }

            silhouette = new Mesh { name = "Inspection silhouette", hideFlags = HideFlags.DontSave };
            silhouette.CombineMeshes(parts.ToArray());
            if (smoothSeams)
                SmoothSeamNormals(silhouette);
            root = new GameObject("Inspection outline preview") { hideFlags = HideFlags.DontSave };
            root.layer = exhibit.gameObject.layer;
            root.transform.SetParent(exhibit, false);
            root.SetActive(false);

            if (echoWidth > 0f)
                echo = AddLayer(shader, "Rose registration offset", new Color32(175, 88, 113, 232),
                    echoWidth, new Vector4(echoOffset.x, echoOffset.y, 0f, 0f), 2899);
            ink = AddLayer(shader, "Ink silhouette", new Color32(48, 40, 43, 251),
                width, Vector4.zero, 2900);
        }

        private static void SmoothSeamNormals(Mesh mesh)
        {
            // Hard-edged plinths split shared corners into separate face vertices.
            // Use one extrusion direction per corner on the outline mesh only.
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var keys = new Vector3Int[vertices.Length];
            var sums = new Dictionary<Vector3Int, Vector3>();
            const float precision = 100000f;
            for (int i = 0; i < vertices.Length; i++)
            {
                var key = Vector3Int.RoundToInt(vertices[i] * precision);
                keys[i] = key;
                sums.TryGetValue(key, out var sum);
                sums[key] = sum + normals[i];
            }

            for (int i = 0; i < normals.Length; i++)
                normals[i] = sums[keys[i]].normalized;
            mesh.normals = normals;
        }

        internal void Show()
        {
            SetWeight(0f);
            root.SetActive(true);
        }

        internal void SetWeight(float weight)
        {
            // The camera's eased weight reaches .91 after about .29 seconds.
            float opacity = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(weight / .91f));
            ink.SetFloat(Opacity, opacity);
            if (echo != null)
                echo.SetFloat(Opacity, opacity);
        }

        internal void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        internal void Destroy()
        {
            Hide();
            Object.Destroy(root);
            Object.Destroy(silhouette);
            Object.Destroy(ink);
            if (echo != null)
                Object.Destroy(echo);
        }

        private Material AddLayer(Shader shader, string name, Color color, float width, Vector4 offset, int queue)
        {
            var material = new Material(shader) { name = name, hideFlags = HideFlags.DontSave, renderQueue = queue };
            material.SetColor(Ink, color);
            material.SetFloat(Width, width);
            material.SetVector(Offset, offset);
            var layer = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)) { layer = root.layer };
            layer.transform.SetParent(root.transform, false);
            layer.GetComponent<MeshFilter>().sharedMesh = silhouette;
            var renderer = layer.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return material;
        }
    }
}
