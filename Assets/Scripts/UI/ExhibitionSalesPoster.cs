using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal sealed class ExhibitionSalesPoster
    {
        private static readonly Color32 Ink = new(30, 20, 20, 255);
        private static readonly Color32 Wine = new(141, 34, 31, 255);
        private static readonly Color32 Mustard = new(231, 179, 57, 255);
        private static readonly Color32 Rose = new(224, 140, 145, 255);
        private static readonly Color32 Cream = new(255, 231, 155, 255);
        private static readonly Color32 Paper = new(240, 221, 171, 255);
        private readonly Texture2D print;
        private readonly Texture2D dropInk;
        private readonly Texture2D stampInk;
        private readonly Material headlineMaterial;
        private readonly Material numberMaterial;
        private readonly Material stampMaterial;
        private readonly TMP_Text bigNumber;
        private readonly RawImage[] drops = new RawImage[4];

        internal TMP_Text Number { get; }
        internal TMP_Text Title { get; }
        internal TMP_Text MaterialLine { get; }
        internal TMP_Text Description { get; }

        internal ExhibitionSalesPoster(RectTransform panel, TMP_FontAsset font, TMP_FontAsset controls)
        {
            print = MakePrint();
            dropInk = MakeDropInk();
            stampInk = MakeStampInk();
            Image(panel, "Overprinted poster stock", print, 0, 0, 594, 1080);
            MaterialReferenceManager.AddFontAsset(controls);

            Number = Label(panel, controls, "Exhibit number", "", 37, 33, 101, 49, 33, Cream);
            Number.fontStyle = FontStyles.Bold;
            bigNumber = Label(panel, font, "Oversized lot number", "", 423, 186, 150, 152, 105, Rose);
            bigNumber.fontStyle = FontStyles.Bold;
            bigNumber.characterSpacing = -7f;
            bigNumber.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -8f);
            numberMaterial = PrintInk(bigNumber, Wine, .18f, .12f);

            var sale = Label(panel, font, "Sale sticker", "판매<font=\"" + controls.name + "\">용</font>", 105, 416, 383, 70, 47, Ink,
                TextAlignmentOptions.Center);
            sale.fontStyle = FontStyles.Bold;
            sale.characterSpacing = 4f;
            sale.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -5f);

            var stamp = MenuUi.Rect(panel, "Late cancellation overprint", 0, 0, 526, 136);
            stamp.pivot = new Vector2(.5f, .5f);
            stamp.anchoredPosition = new Vector2(298, -548);
            stamp.localRotation = Quaternion.Euler(0f, 0f, 19f);
            Image(stamp, "Broken red stamp impression", stampInk, 0, 0, 526, 136);
            var denial = Label(stamp, controls, "Use prohibited stamp", "사용 불가", 15, 11, 496, 113, 78,
                new Color32(200, 35, 40, 246),
                TextAlignmentOptions.Center);
            denial.fontStyle = FontStyles.Bold;
            denial.characterSpacing = 2f;
            stampMaterial = PrintInk(denial, new Color32(255, 225, 159, 219), .12f, .055f);
            stampMaterial.DisableKeyword("UNDERLAY_ON");

            Title = Label(panel, font, "Liquid exhibit name", "", 43, 96, 518, 289, 105, Cream);
            Title.fontStyle = FontStyles.Bold;
            Title.characterSpacing = -1.4f;
            Title.lineSpacing = -12f;
            headlineMaterial = PrintInk(Title, Wine, .24f, .075f);
            for (int i = 0; i < drops.Length; i++)
            {
                drops[i] = Image(Title.rectTransform, "Lettering ink drip", dropInk, 0, 0, 10, 24);
                drops[i].color = i < 2 ? Cream : Rose;
                drops[i].enabled = false;
            }
            Title.OnPreRenderText += FlowLettering;

            MaterialLine = Label(panel, font, "Printed classification", "", 48, 773, 499, 37, 23, Wine);
            MaterialLine.fontStyle = FontStyles.Bold;
            Description = Label(panel, font, "Product description", "", 48, 826, 495, 146, 27, Ink);
            Description.textWrappingMode = TextWrappingModes.Normal;
            Description.lineSpacing = 14f;
            Label(panel, controls, "Return hint", "F / Esc   돌아가기", 47, 1011, 323, 37, 22, Paper);
        }

        internal void SetHeadline(string number, string title)
        {
            string digits = Regex.Match(number, "[0-9]+").Value;
            Number.text = "#" + digits;
            bigNumber.text = digits;
            var parts = title.Split(new[] { ' ' }, 2);
            Title.text = parts.Length == 2
                ? "<color=#FFE79B>" + parts[0] + "</color>\n<color=#E08C91>" + parts[1] + "</color>"
                : title;
        }

        internal void Destroy()
        {
            Object.Destroy(print);
            Object.Destroy(dropInk);
            Object.Destroy(stampInk);
            Object.Destroy(headlineMaterial);
            Object.Destroy(numberMaterial);
            Object.Destroy(stampMaterial);
        }

        private void FlowLettering(TMP_TextInfo info)
        {
            int glyph = 0;
            for (int i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible)
                    continue;
                var points = info.meshInfo[character.materialReferenceIndex].vertices;
                int v = character.vertexIndex;
                Vector3 center = (points[v] + points[v + 2]) * .5f;
                float bend = Mathf.Sin(glyph * 1.92f + .8f);
                Quaternion tilt = Quaternion.Euler(0f, 0f, 6.7f * bend);
                for (int j = 0; j < 4; j++)
                {
                    Vector3 offset = points[v + j] - center;
                    offset.x += offset.y * (.15f + .05f * bend);
                    offset.y *= 1f + .07f * bend;
                    points[v + j] = center + tilt * offset + new Vector3(bend * 2f, bend * 5f, 0f);
                }
                glyph++;
            }
        }

        internal void PlaceInkDrops()
        {
            Title.ForceMeshUpdate();
            var info = Title.textInfo;
            int glyph = 0, drip = 0;
            for (int i = 0; i < info.characterCount && drip < drops.Length; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible)
                    continue;
                if (glyph == 0 || glyph == 2 || glyph == 3 || glyph == 5)
                {
                    var points = info.meshInfo[character.materialReferenceIndex].vertices;
                    int v = character.vertexIndex;
                    var drop = drops[drip];
                    var basePoint = Vector3.Lerp(points[v], points[v + 3], glyph % 2 == 0 ? .68f : .57f);
                    float width = 10f + (drip % 2) * 5f;
                    drop.rectTransform.sizeDelta = new Vector2(width, 22f + (drip % 2) * 9f);
                    drop.rectTransform.anchoredPosition = new Vector2(basePoint.x - width * .5f, basePoint.y + 6f);
                    drop.enabled = true;
                    drip++;
                }
                glyph++;
            }
            for (; drip < drops.Length; drip++)
                drops[drip].enabled = false;
        }

        private static Material PrintInk(TMP_Text label, Color edge, float swell, float outline)
        {
            var material = new Material(label.fontSharedMaterial) { name = "Raised overprint ink" };
            material.SetFloat("_FaceDilate", swell);
            material.SetColor("_OutlineColor", edge);
            material.SetFloat("_OutlineWidth", outline);
            material.SetColor("_UnderlayColor", edge);
            material.SetFloat("_UnderlayOffsetX", .6f);
            material.SetFloat("_UnderlayOffsetY", -.7f);
            material.SetFloat("_UnderlayDilate", .3f);
            material.SetFloat("_UnderlaySoftness", .08f);
            material.EnableKeyword("UNDERLAY_ON");
            label.fontSharedMaterial = material;
            return material;
        }

        private static TMP_Text Label(RectTransform parent, TMP_FontAsset font, string name, string value, float x,
            float y, float width, float height, float size, Color ink, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var label = MenuUi.Text(parent, font, value, x, y, width, height, size, ink, align);
            label.name = name;
            return label;
        }

        private static RawImage Image(RectTransform parent, string name, Texture texture, float x, float y, float w, float h)
        {
            var image = MenuUi.Rect(parent, name, x, y, w, h).gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static Texture2D MakePrint()
        {
            const int width = 396, height = 720;
            var pixels = new Color32[width * height];
            for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
            {
                float x = (column + .5f) * 1.5f;
                float y = 1080f - (row + .5f) * 1.5f;
                float noise = Grain(column, row);
                float rough = 2.2f * Mathf.Sin(x * .09f) + 1.8f * Mathf.Sin(x * .24f) + 1.7f * noise;
                float rays = Mathf.Sin(Mathf.Atan2(y - 561f, x - 283f) * 12f);
                Color32 color = rays < 0 ? new Color32(199, 122, 33, 255) : Mustard;
                bool dots = (column % 7 - 3) * (column % 7 - 3) + (row % 7 - 3) * (row % 7 - 3) < 4;
                if (y > 368 && y < 751 && dots && (x < 110 || x > 505 || y > 674))
                    color = new Color32(170, 77, 32, 255);

                float ring = new Vector2((x - 297) / 230f, (y - 565) / 152f).sqrMagnitude;
                if (ring < 1.06f && y > 390)
                {
                    color = ring > .98f ? Wine : ring > .89f ? Ink : ring > .81f ? Mustard : Cream;
                    float productX = 451 + (x - 297) / 2.91f;
                    float productY = 254 + (y - 562) / 2.53f;
                    if (Toilet(productX - 4, productY - 3))
                        color = Wine;
                    if (Toilet(productX, productY))
                        color = dots ? new Color32(101, 47, 36, 255) : Ink;
                }

                float sellY = y - (x - 105f) * .087f;
                if (x > 91 + rough && x < 507 + rough && sellY > 411 + rough && sellY < 484 + rough)
                    color = sellY > 478 ? Wine : Rose;
                if (y > 27 + rough && y < 389 + rough)
                    color = y > 375 + rough ? Wine : Ink;
                if (Drip(x, y, 94, 383, 11, 25) || Drip(x, y, 527, 380, 10, 27))
                    color = Ink;
                if (x > 26 && x < 568 && y > 751 + rough && y < 982 + rough)
                    color = Paper;
                if (x > 48 && x < 545 && y > 811 && y < 813)
                    color = new Color32(165, 99, 68, 255);
                if (x < 12 + rough || x > 582 + rough || y < 13 + rough || y > 991 + rough)
                    color = Ink;
                if (x > 131 && x < 255 && y > 36 && y < 79 && ClassicMark(x - 131, y - 37))
                    color = Cream;
                if (x > 340 && x < 546 && y > 56 + rough && y < 60 + rough)
                    color = Rose;

                float dirt = noise * (y > 754 && y < 983 ? 5f : 10f) + Mathf.Sin(column * .81f) * .9f;
                if (noise > .9f && (y < 48 || x < 32 || x > 559 || y > 965))
                    dirt -= 33f;
                pixels[row * width + column] = new Color32(Shade(color.r, dirt), Shade(color.g, dirt), Shade(color.b, dirt), 255);
            }
            return Texture("Distressed sale broadside", width, height, pixels);
        }

        private static Texture2D MakeStampInk()
        {
            const int w = 263, h = 68;
            var pixels = new Color32[w * h];
            for (int row = 0; row < h; row++)
            for (int col = 0; col < w; col++)
            {
                float noise = Grain(col, row);
                float edge = Mathf.Min(Mathf.Min(col, w - 1 - col), Mathf.Min(row, h - 1 - row));
                float broken = edge + 1.4f * Mathf.Sin(col * .44f + row * .17f) + noise * 1.7f;
                float opacity = 0f;
                if (broken > 4f && broken < 9f)
                    opacity = noise > -.65f ? .91f : .18f;
                else if (broken > 1f && broken < 4f)
                    opacity = noise > .04f ? .43f : .08f;
                else if (broken > 9f && broken < 16f && noise > .89f)
                    opacity = .52f;
                pixels[row * w + col] = new Color32(204, 28, 34, (byte)(255 * opacity));
            }
            return Texture("Distressed cancellation ink", w, h, pixels);
        }

        // The supplied museum atlas has no 클 or 래 glyph. Draw the fixed class mark on the poster stock.
        private static bool ClassicMark(float x, float y)
        {
            if (x < 39)
                return Stroke(x, y, 4, 5, 33, 5) || Stroke(x, y, 33, 5, 33, 16)
                    || Stroke(x, y, 4, 12, 33, 12) || Stroke(x, y, 2, 22, 36, 22)
                    || Stroke(x, y, 7, 28, 31, 28) || Stroke(x, y, 31, 28, 31, 33)
                    || Stroke(x, y, 8, 33, 31, 33) || Stroke(x, y, 8, 33, 8, 39)
                    || Stroke(x, y, 8, 39, 32, 39);
            if (x < 82)
            {
                x -= 42;
                return Stroke(x, y, 3, 7, 15, 7) || Stroke(x, y, 15, 7, 15, 17)
                    || Stroke(x, y, 3, 17, 15, 17) || Stroke(x, y, 3, 17, 3, 31)
                    || Stroke(x, y, 3, 31, 16, 31) || Stroke(x, y, 23, 4, 23, 39)
                    || Stroke(x, y, 34, 4, 34, 39) || Stroke(x, y, 16, 20, 23, 20);
            }
            x -= 84;
            return Stroke(x, y, 4, 20, 14, 6) || Stroke(x, y, 14, 6, 24, 19)
                || Stroke(x, y, 33, 4, 33, 22) || Stroke(x, y, 8, 28, 32, 28)
                || Stroke(x, y, 32, 28, 32, 39);
        }

        private static bool Stroke(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float t = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy));
            float u = x - ax - t * dx, v = y - ay - t * dy;
            return u * u + v * v < 6.8f;
        }

        private static bool Toilet(float x, float y)
        {
            bool lid = Ellipse(x, y, 469, 228, 25, 27) && !Ellipse(x, y, 469, 225, 16, 18);
            bool tank = x > 480 && x < 501 && y > 219 && y < 266;
            bool bowl = Ellipse(x, y, 448, 262, 48, 18) && !Ellipse(x, y, 443, 255, 34, 6);
            bool baseShape = Ellipse(x, y, 464, 278, 17, 20) || Ellipse(x, y, 464, 294, 29, 6);
            return lid || tank || bowl || baseShape;
        }

        private static bool Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float u = (x - cx) / rx, v = (y - cy) / ry;
            return u * u + v * v < 1f;
        }

        private static bool Drip(float x, float y, float cx, float top, float rx, float length)
            => Ellipse(x, y, cx, top + length * .38f, rx, length * .55f)
                || Ellipse(x, y, cx + 1, top + length * .87f, rx * .47f, length * .25f);

        private static Texture2D MakeDropInk()
        {
            const int w = 28, h = 54;
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float v = 1f - y / (h - 1f);
                float u = x / (w - 1f) - .5f - .10f * v;
                float neck = Mathf.Lerp(.38f, .10f, Mathf.Clamp01(v * 2.1f));
                float distance = Mathf.Min(Mathf.Abs(u) - neck + Mathf.Max(0f, v - .76f),
                    new Vector2(u * 1.45f, v - .76f).magnitude - .205f);
                pixels[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(-distance * 24f)));
            }
            return Texture("Viscous ink shape", w, h, pixels);
        }

        private static Texture2D Texture(string name, int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static byte Shade(byte channel, float amount) => (byte)Mathf.Clamp(channel + amount, 0f, 255f);

        private static float Grain(int x, int y)
        {
            uint hash = unchecked((uint)x * 374761393u + (uint)y * 668265263u + 1597334677u);
            hash = unchecked((hash ^ (hash >> 13)) * 1274126177u);
            return ((hash ^ (hash >> 16)) & 255u) / 127.5f - 1f;
        }
    }
}
