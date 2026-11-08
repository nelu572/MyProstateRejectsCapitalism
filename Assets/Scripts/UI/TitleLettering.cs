using TMPro;
using UnityEngine;

namespace MyProstateRejectsCapitalism.UI
{
    [DisallowMultipleComponent]
    public sealed class TitleLettering : MonoBehaviour
    {
        [SerializeField] private Camera titleCamera;
        [SerializeField] private Light practical;

        private const float CameraPeriod = 12f;
        private const float DesignAspect = 16f / 9f;
        private static readonly Color32 Ivory = new(247, 233, 198, 255);
        private static readonly Color32 Gold = new(255, 185, 71, 255);
        private static readonly Color32 DarkEdge = new(32, 38, 31, 255);

        private Transform letters;
        private Vector3 cameraOrigin;
        private Quaternion cameraRotation;
        private float beganAt;
        private float lightRest;
        private float fieldOfView;

        internal void Build(TMP_FontAsset font)
        {
            var cameraTransform = titleCamera.transform;
            cameraOrigin = cameraTransform.position;
            cameraRotation = cameraTransform.rotation;
            lightRest = practical.intensity;
            beganAt = Time.time;
            fieldOfView = titleCamera.fieldOfView;

            letters = new GameObject("Spatial title lettering").transform;
            letters.SetParent(transform, false);

            titleCamera.aspect = DesignAspect;
            AddWallLine(font, "내 전립선은", new Vector2(.314f, .714f), .121f, 2.63f);
            AddWallLine(font, "자본주의를", new Vector2(.340f, .548f), .164f, 2.55f);

            const float foregroundDepth = 4.90f;
            var foreground = titleCamera.ViewportToWorldPoint(new Vector3(.395f, .398f, foregroundDepth));
            AddBlock(font, "거부한다", foreground, Quaternion.Euler(-3f, 7f, 0f),
                HeightAtDepth(foregroundDepth, .224f), .23f, Gold);
            titleCamera.ResetAspect();
        }

        internal void Show(bool visible) => letters.gameObject.SetActive(visible);

        private void AddWallLine(TMP_FontAsset font, string text, Vector2 view, float height, float wallZ)
        {
            var ray = titleCamera.ViewportPointToRay(view);
            var position = ray.GetPoint((wallZ - ray.origin.z) / ray.direction.z);
            var depth = Vector3.Dot(position - cameraOrigin, cameraRotation * Vector3.forward);
            AddBlock(font, text, position, Quaternion.identity, HeightAtDepth(depth, height), .085f, Ivory);
        }

        private float HeightAtDepth(float depth, float viewportHeight) =>
            2f * depth * Mathf.Tan(fieldOfView * .5f * Mathf.Deg2Rad) * viewportHeight;

        private void AddBlock(TMP_FontAsset font, string text, Vector3 position, Quaternion rotation,
            float height, float thickness, Color faceColor)
        {
            var block = new GameObject(text).transform;
            block.SetParent(letters, false);
            block.SetPositionAndRotation(position, rotation);

            const int layers = 8;
            var face = AddLayer(block, font, text, faceColor, layers);
            face.ForceMeshUpdate();
            var bounds = face.textBounds;
            var scale = height / Mathf.Max(.01f, bounds.size.y);
            block.localScale = Vector3.one * scale;
            face.transform.localPosition = -bounds.center;

            for (int i = 1; i <= layers; i++)
            {
                float fraction = i / (float)layers;
                var color = Color.Lerp(faceColor, DarkEdge, .64f + fraction * .36f);
                var edge = AddLayer(block, font, text, color, layers - i);
                edge.transform.localPosition = -bounds.center + Vector3.forward * (thickness / scale * fraction);
            }
        }

        private static TextMeshPro AddLayer(Transform parent, TMP_FontAsset font, string value, Color color, int order)
        {
            var text = new GameObject("Lettering layer", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            text.transform.SetParent(parent, false);
            text.font = font;
            text.text = value;
            text.fontSize = 10f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.color = color;
            text.renderer.sortingOrder = order;
            return text;
        }

        private void LateUpdate()
        {
            float elapsed = Time.time - beganAt;
            float phase = elapsed * (2f * Mathf.PI / CameraPeriod);
            float side = Mathf.Sin(phase);
            float arc = 1f - Mathf.Cos(phase);
            var position = cameraOrigin + cameraRotation * new Vector3(.42f * side, .06f * arc, .18f * arc);
            var rotation = cameraRotation * Quaternion.Euler(.50f * arc, -1.65f * side, .65f * side);
            titleCamera.transform.SetPositionAndRotation(position, rotation);
            float widthFit = Mathf.Max(1f, DesignAspect / titleCamera.aspect);
            titleCamera.fieldOfView = Mathf.Atan(Mathf.Tan(fieldOfView * .5f * Mathf.Deg2Rad) * widthFit)
                * (2f * Mathf.Rad2Deg);

            float dip = Mathf.Pow(.5f + .5f * Mathf.Sin(phase * 2f + .65f), 10f);
            practical.intensity = lightRest * (1f - .11f * dip);
        }
    }
}
