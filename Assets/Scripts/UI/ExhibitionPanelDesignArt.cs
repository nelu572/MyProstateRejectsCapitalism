using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal enum ExhibitionPanelDesign { Original, DarkCatalogue, Porcelain, PatinatedSign, SalesPoster, QuietExhibition }

    internal sealed class ExhibitionPanelDesignArt
    {
        private readonly Texture2D surface;
        private readonly ExhibitionPanelDesign design;
        private TMP_Text graphicIndex;

        internal TMP_Text Number { get; private set; }
        internal TMP_Text Title { get; private set; }
        internal TMP_Text Material { get; private set; }
        internal TMP_Text Description { get; private set; }

        internal ExhibitionPanelDesignArt(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls,
            ExhibitionPanelDesign design)
        {
            this.design = design;
            surface = CreateSurface(design);
            var ground = MenuUi.Rect(panel, "Panel surface", 0, 0, 594, 1080).gameObject.AddComponent<RawImage>();
            ground.texture = surface;
            ground.raycastTarget = false;

            switch (design)
            {
                case ExhibitionPanelDesign.DarkCatalogue: DarkCatalogue(panel, font, controls); break;
                case ExhibitionPanelDesign.Porcelain: Porcelain(panel, font, controls); break;
                case ExhibitionPanelDesign.PatinatedSign: PatinatedSign(panel, font, controls); break;
                case ExhibitionPanelDesign.QuietExhibition: QuietExhibition(panel, font, controls); break;
            }
        }

        internal void SetHeadline(string originalNumber, string originalTitle)
        {
            if (graphicIndex != null)
                graphicIndex.text = Regex.Match(originalNumber, "[0-9]+").Value;

            if (design == ExhibitionPanelDesign.QuietExhibition)
            {
                Number.text = Regex.Match(originalNumber, "[0-9]+").Value;
                int split = originalTitle.LastIndexOf(' ');
                Title.text = split > 0
                    ? FitQuietTitleLine(originalTitle[..split], 84) + "\n"
                        + FitQuietTitleLine("<b>" + originalTitle[(split + 1)..] + "</b>", 110)
                    : FitQuietTitleLine(originalTitle, 110);
            }
            else
                Title.text = originalTitle;
        }

        private string FitQuietTitleLine(string line, int size)
        {
            string formatted = $"<size={size}>{line}</size>";
            float preferredWidth = Title.GetPreferredValues(formatted, Mathf.Infinity, Mathf.Infinity).x;
            float availableWidth = Title.rectTransform.rect.width - 4f;
            return preferredWidth <= availableWidth
                ? formatted
                : $"<size={Mathf.FloorToInt(size * availableWidth / preferredWidth)}>{line}</size>";
        }

        internal void Destroy() => Object.Destroy(surface);

        private void QuietExhibition(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls)
        {
            var ivory = Tone(239, 231, 214);
            var secondary = Tone(192, 181, 165);
            var accent = Tone(166, 138, 119);
            Block(panel, "Registration line", 68, 0, 1, 111, Tone(166, 138, 119, 155));
            Number = Label(panel, font, "Object number", "", 88, 67, 128, 54, 37, accent);
            Number.characterSpacing = 2f;

            Title = Label(panel, font, "Exhibition title", "", 68, 207, 454, 276, 110, ivory,
                TextAlignmentOptions.TopRight);
            Title.characterSpacing = -1.25f;
            Title.lineSpacing = -7f;

            Block(panel, "Text divider", 68, 508, 454, 1, Tone(191, 174, 153, 112));
            Material = Label(panel, font, "Object classification", "", 68, 544, 454, 38, 23, secondary);
            Description = Body(panel, font, 68, 618, 454, 28, 15, ivory);

            Block(panel, "Return rule", 68, 944, 40, 1, Tone(191, 174, 153, 112));
            Label(panel, controls, "Return hint", "F / Esc   돌아가기", 68, 965, 454, 37, 21, secondary);
        }

        private void DarkCatalogue(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls)
        {
            var ivory = Tone(245, 238, 222);
            var bronze = Tone(182, 156, 119);
            var secondary = Tone(204, 191, 169);
            Block(panel, "Cloth spine shadow", 0, 0, 25, 1080, Tone(8, 8, 8, 55));
            Block(panel, "Spine foil", 25, 0, 1, 1080, Tone(164, 139, 102, 104));
            Block(panel, "Catalogue trim", 57, 97, 480, 1, Tone(174, 153, 122, 104));
            Block(panel, "Index rule", 62, 188, 49, 2, bronze);

            Number = Label(panel, font, "Exhibit index", "", 62, 127, 472, 38, 22, bronze);
            Number.characterSpacing = 4f;
            Title = Label(panel, font, "Catalogue title", "", 57, 226, 481, 103, 66, ivory);
            Title.fontStyle = FontStyles.Bold;
            Title.characterSpacing = -1.4f;
            Material = Label(panel, font, "Classification", "", 62, 348, 472, 42, 24, secondary);
            Description = Body(panel, font, 62, 455, 472, 31, 16, Tone(226, 220, 208));

            graphicIndex = Label(panel, font, "Foil folio", "", 277, 710, 257, 210, 151,
                Tone(132, 112, 86), TextAlignmentOptions.BottomRight);
            graphicIndex.characterSpacing = -3f;
            Block(panel, "Footer hairline", 62, 950, 472, 1, Tone(162, 143, 116, 150));
            Label(panel, controls, "Return hint", "F / Esc   돌아가기", 62, 979, 472, 37, 22, secondary);
        }

        private void Porcelain(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls)
        {
            var ink = Tone(43, 48, 43);
            var secondary = Tone(67, 73, 65);
            Block(panel, "Ceramic edge shade", 0, 0, 594, 9, Tone(93, 102, 94, 91));
            Block(panel, "Ceramic side shade", 585, 9, 9, 1062, Tone(93, 102, 94, 96));
            Block(panel, "Ceramic lower shade", 0, 1070, 594, 10, Tone(93, 102, 94, 121));
            Block(panel, "Glaze lip left", 10, 12, 3, 1055, Tone(253, 251, 239, 155));
            Block(panel, "Glaze lip top", 13, 12, 568, 3, Tone(253, 251, 239, 155));
            Frame(panel, "Inset porcelain seat", 24, 26, 546, 1028, Tone(100, 110, 101, 104));
            Block(panel, "Recessed exhibit cartouche", 214, 122, 166, 45, Tone(116, 125, 115, 51));
            Block(panel, "Cartouche cut edge", 214, 122, 166, 1, Tone(100, 108, 96, 149));
            Block(panel, "Cartouche glaze edge", 214, 166, 166, 1, Tone(255, 252, 241, 166));

            Number = Label(panel, font, "Printed exhibit number", "", 218, 124, 158, 41, 19, secondary,
                TextAlignmentOptions.Center);
            Number.characterSpacing = 1.9f;
            Title = Label(panel, font, "Porcelain title", "", 50, 230, 494, 100, 59, ink, TextAlignmentOptions.Center);
            Title.fontStyle = FontStyles.Bold;
            Material = Label(panel, font, "Centered classification", "", 54, 343, 486, 45, 23, secondary,
                TextAlignmentOptions.Center);
            Block(panel, "Glaze divider recess", 251, 429, 92, 1, Tone(99, 108, 99, 155));
            Block(panel, "Glaze divider highlight", 251, 430, 92, 1, Tone(255, 253, 241, 137));
            Description = Body(panel, font, 75, 487, 444, 30, 13, ink);

            Block(panel, "Lower printed field", 25, 924, 544, 129, Tone(115, 125, 111, 21));
            Block(panel, "Lower seam", 25, 924, 544, 1, Tone(109, 117, 107, 130));
            Label(panel, controls, "Return hint", "F / Esc   돌아가기", 53, 964, 488, 45, 22, secondary,
                TextAlignmentOptions.Center);
        }

        private void PatinatedSign(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls)
        {
            var brass = Tone(189, 155, 103);
            var ivory = Tone(246, 237, 214);
            var muted = Tone(184, 202, 190);
            Frame(panel, "Oxidized mounting edge", 9, 9, 576, 1062, Tone(9, 28, 28, 207));
            Frame(panel, "Brass perimeter", 19, 19, 556, 1042, Tone(197, 161, 108, 186));
            Frame(panel, "Recessed seam", 25, 25, 544, 1030, Tone(137, 115, 78, 137));
            foreach (int x in new[] { 35, 552 })
            foreach (int y in new[] { 35, 1038 })
            {
                Block(panel, "Fastener", x, y, 7, 7, Tone(164, 138, 99));
                Block(panel, "Fastener slot", x + 1, y + 3, 5, 1, Tone(62, 61, 46));
            }

            Block(panel, "Index plate bed", 57, 103, 149, 151, Tone(11, 35, 35, 174));
            Frame(panel, "Index plate brass", 57, 103, 149, 151, Tone(201, 168, 113, 191));
            graphicIndex = Label(panel, font, "Stamped digits", "", 59, 104, 145, 149, 77, Tone(220, 193, 144),
                TextAlignmentOptions.Center);
            graphicIndex.characterSpacing = -1.6f;
            Number = Label(panel, font, "Stamped exhibit number", "", 232, 140, 300, 39, 21, brass);
            Number.characterSpacing = 3.2f;
            Block(panel, "Index face score", 232, 204, 299, 1, Tone(189, 155, 103, 109));
            Title = Label(panel, font, "Sign title", "", 54, 318, 486, 101, 61, ivory);
            Title.fontStyle = FontStyles.Bold;
            Material = Label(panel, font, "Sign classification", "", 57, 441, 478, 41, 23, muted);
            Block(panel, "Description rail", 57, 537, 2, 156, Tone(187, 153, 102, 156));
            Description = Body(panel, font, 78, 538, 454, 29, 16, ivory);

            Block(panel, "Return legend bed", 57, 929, 480, 72, Tone(10, 32, 32, 151));
            Frame(panel, "Return legend rim", 57, 929, 480, 72, Tone(158, 132, 92, 174));
            Label(panel, controls, "Return hint", "F / Esc   돌아가기", 77, 947, 440, 36, 22, muted);
        }

        private static TMP_Text Body(RectTransform parent, TMP_FontAsset font, float x, float y, float width,
            float size, float spacing, Color ink)
        {
            var text = Label(parent, font, "Exhibit description", "", x, y, width, 206, size, ink);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.lineSpacing = spacing;
            return text;
        }

        private static TMP_Text Label(RectTransform parent, TMP_FontAsset font, string name, string text, float x,
            float y, float width, float height, float size, Color ink, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var label = MenuUi.Text(parent, font, text, x, y, width, height, size, ink, align);
            label.name = name;
            return label;
        }

        private static void Frame(RectTransform parent, string name, float x, float y, float width, float height, Color ink)
        {
            Block(parent, name + " top", x, y, width, 1, ink);
            Block(parent, name + " bottom", x, y + height - 1, width, 1, ink);
            Block(parent, name + " left", x, y + 1, 1, height - 2, ink);
            Block(parent, name + " right", x + width - 1, y + 1, 1, height - 2, ink);
        }

        private static void Block(RectTransform parent, string name, float x, float y, float width, float height, Color color)
        {
            var image = MenuUi.Rect(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static Texture2D CreateSurface(ExhibitionPanelDesign design)
        {
            const int width = 198, height = 360;
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = x / (width - 1f), v = y / (height - 1f);
                float grain = Grain(x, y);
                Color32 color;
                if (design == ExhibitionPanelDesign.DarkCatalogue)
                {
                    float fiber = 1.4f * Grain(x, 0) + .7f * Grain(0, y);
                    float ink = 4f * v - 4f * Mathf.Abs(u - .48f) + 1.7f * grain + fiber;
                    color = Tone(28 + ink, 28 + ink, 27 + ink);
                }
                else if (design == ExhibitionPanelDesign.Porcelain)
                {
                    float diagonal = u + .54f * (1f - v) - .43f;
                    float glaze = 12f * Mathf.Exp(-diagonal * diagonal * 9f) + 4f * v + 1.2f * grain;
                    color = Tone(202 + glaze, 205 + glaze, 197 + glaze);
                }
                else if (design == ExhibitionPanelDesign.QuietExhibition)
                {
                    float paper = v * 2.1f + grain * 1.5f;
                    color = Tone(30 + paper, 28 + paper, 26 + paper);
                }
                else
                {
                    float oxide = Mathf.Sin(u * 14f + Mathf.Sin(v * 11f) * 2f)
                        * Mathf.Cos(v * 16f - u * 4f);
                    float brush = 1.5f * Grain(x, 0) + .9f * grain;
                    float light = 6f * (1f - Mathf.Abs(u - .4f)) + 3.5f * oxide + brush;
                    color = Tone(19 + light, 49 + light, 48 + light);
                }
                pixels[y * width + x] = color;
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = design + " review surface",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static float Grain(int x, int y)
        {
            uint hash = unchecked((uint)x * 374761393u + (uint)y * 668265263u + 1597334677u);
            hash = unchecked((hash ^ (hash >> 13)) * 1274126177u);
            return ((hash ^ (hash >> 16)) & 255u) / 127.5f - 1f;
        }

        private static Color32 Tone(float red, float green, float blue, byte alpha = 255)
            => new((byte)Mathf.RoundToInt(red), (byte)Mathf.RoundToInt(green), (byte)Mathf.RoundToInt(blue), alpha);
    }
}
