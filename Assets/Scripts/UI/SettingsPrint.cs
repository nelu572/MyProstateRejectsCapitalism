using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal sealed class SettingsPrint : MonoBehaviour
    {
        private static readonly int Flow = Shader.PropertyToID("_LiquidFlow");
        private static readonly int Glaze = Shader.PropertyToID("_LiquidGlaze");
        private static readonly int Gold = Shader.PropertyToID("_LiquidGold");
        private Material title;
        private Material labels;
        private Material captions;
        private Texture2D artwork;

        internal void Initialize(TMP_FontAsset font)
        {
            artwork = Resources.Load<Texture2D>("Menus/SettingsMeltLettering");
            var shader = Resources.Load<Shader>("Menus/SettingsPrint");
            if (shader == null)
                return;

            title = CreateInk(font.material, shader, 1.18f, .91f, .87f);
            labels = CreateInk(font.material, shader, .96f, .76f, 0);
            captions = CreateInk(font.material, shader, .24f, .32f, 0);
        }

        internal bool TryTitle(Transform plaque)
        {
            if (artwork == null)
                return false;
            DrawArt(plaque, "설정 artwork", 0, 508, 250, 23, 8, 240, 240 * 250f / 508);
            return true;
        }

        internal void ApplyWord(TextMeshProUGUI label)
        {
            // Each row was drawn as its own word, from curved strokes rather than from the font atlas.
            int row = label.text switch
            {
                "마우스 감도" => 1,
                "자막 속도" => 2,
                "화면 모드" => 3,
                "전체화면" => 4,
                "창모드" => 5,
                "돌아가기" => 6,
                _ => 0
            };
            if (artwork == null || row == 0)
            {
                Apply(label);
                return;
            }

            int sourceWidth = 20;
            foreach (char c in label.text)
                sourceWidth += c == ' ' ? 54 : 182;
            var area = label.rectTransform.sizeDelta;
            float height = Mathf.Min(area.y, label.fontSize * 1.42f);
            float width = height * sourceWidth / 244;
            if (width > area.x)
            {
                height *= area.x / width;
                width = area.x;
            }
            float x = label.alignment == TextAlignmentOptions.Center ? (area.x - width) * .5f : 0;
            DrawArt(label.transform, label.text + " artwork", row, sourceWidth, 244, x, (area.y - height) * .5f, width, height);
            label.enabled = false;
        }

        internal static void SetButtonContrast(Button button, bool lightSurface)
        {
            var ink = button.GetComponentInChildren<RawImage>();
            if (ink != null)
                ink.color = lightSurface ? new Color32(113, 79, 45, 255) : Color.white;
        }

        private void DrawArt(Transform parent, string name, int row, int sourceWidth, int sourceHeight,
            float x, float y, float width, float height) =>
            DrawLettering(parent, name, artwork, new RectInt(0, row * 256, sourceWidth, sourceHeight),
                new Rect(x, y, width, height));

        internal static RawImage DrawLettering(Transform parent, string name, Texture2D atlas, RectInt source,
            Rect placement)
        {
            var rect = MenuUi.Rect(parent, name, placement.x, placement.y, placement.width, placement.height);
            var image = rect.gameObject.AddComponent<RawImage>();
            image.texture = atlas;
            image.raycastTarget = false;
            image.uvRect = new Rect(source.x / (float)atlas.width, 1 - source.yMax / (float)atlas.height,
                source.width / (float)atlas.width, source.height / (float)atlas.height);
            return image;
        }

        internal void Apply(TextMeshProUGUI text, bool headline = false, bool subtle = false)
        {
            var ink = headline ? title : subtle ? captions : labels;
            if (ink != null)
            {
                text.fontSharedMaterial = ink;
                text.extraPadding = true;
            }
        }

        private static Material CreateInk(Material original, Shader shader, float flow, float glaze, float gold)
        {
            var ink = new Material(original) { shader = shader };
            ink.SetFloat(Flow, flow);
            ink.SetFloat(Glaze, glaze);
            ink.SetFloat(Gold, gold);
            return ink;
        }

        private void OnDestroy()
        {
            Destroy(title);
            Destroy(labels);
            Destroy(captions);
        }
    }
}
