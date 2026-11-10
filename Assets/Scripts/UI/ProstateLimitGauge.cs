using MyProstateRejectsCapitalism.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal sealed class ProstateLimitGauge
    {
        private static readonly Color32 Case = new(41, 45, 45, 242);
        private static readonly Color32 Bezel = new(101, 99, 81, 255);
        private static readonly Color32 Channel = new(20, 25, 25, 255);
        private static readonly Color32 Ink = new(232, 224, 193, 255);
        private static readonly Color32 Amber = new(231, 187, 69, 255);
        private static readonly Color32 Alarm = new(245, 139, 62, 255);
        private static readonly Color32 Rim = new(150, 137, 101, 242);
        private static readonly Color32 SmokedGlass = new(16, 20, 20, 226);

        private readonly RectTransform fill;
        private readonly Image liquid;
        private readonly Image warningMark;
        private readonly TMP_Text readout;
        private readonly MenuPlate rim;
        private int shownPercent = -1;
        private bool warningShown;
        private bool pulsing;

        internal ProstateLimitGaugeLayout Layout { get; }
        internal GameObject Root { get; }

        internal ProstateLimitGauge(Transform parent, TMP_FontAsset font, ProstateLimitGaugeLayout layout)
        {
            Layout = layout;
            bool vertical = layout == ProstateLimitGaugeLayout.Vertical;
            var plate = MenuUi.Panel(parent, "Prostate limit", 0, 0, vertical ? 106 : 288, vertical ? 310 : 58,
                vertical ? Rim : Case, MenuPlate.Shape.Terminal);
            Root = plate.gameObject;
            plate.anchorMin = plate.anchorMax = plate.pivot = vertical
                ? new Vector2(1f, .5f) : new Vector2(.5f, 1f);
            plate.anchoredPosition = vertical ? new Vector2(-25f, 30f) : new Vector2(175f, -31f);

            RectTransform trough;
            if (vertical)
            {
                rim = plate.GetComponent<MenuPlate>();
                var face = MenuUi.Panel(plate, "Smoked gauge face", 2, 2, 102, 306,
                    SmokedGlass, MenuPlate.Shape.Terminal);
                Block(face, "Upper fastener", 12, 12, 4, 4, Bezel);
                Block(face, "Lower fastener", 86, 288, 4, 4, Bezel);
                Label(face, font, "전립선", 0, 12, 102, 22, 19);
                Label(face, font, "한계", 0, 35, 102, 18, 15);
                Block(face, "Tube frame", 32, 62, 38, 186, Rim);
                trough = Block(face, "Tube interior", 35, 65, 32, 180, Channel).rectTransform;
                foreach (float mark in new[] { 0f, .5f, .75f, 1f })
                {
                    float y = 65 + (1f - mark) * 180;
                    Block(face, "Scale tick", 20, y, 9, 1, Rim);
                    Block(face, "Scale tick", 73, y, 9, 1, Rim);
                }
                warningMark = Block(face, "30 percent mark", 73, 190, 14, 3, Bezel);
                readout = Label(face, font, string.Empty, 0, 249, 102, 42, 31);
            }
            else
            {
                Label(plate, font, "전립선 한계", 14, 5, 108, 20, 16);
                readout = Label(plate, font, string.Empty, 219, 4, 53, 22, 18);
                Block(plate, "Track frame", 15, 30, 258, 12, Bezel);
                trough = Block(plate, "Track interior", 16, 31, 256, 10, Channel).rectTransform;
                warningMark = Block(plate, "30 percent mark", 92, 43, 2, 5, Bezel);
            }

            liquid = Block(trough, "Liquid", 0, 0, 0, 0, Amber);
            fill = liquid.rectTransform;
            fill.pivot = Vector2.zero;
            fill.anchorMin = Vector2.zero;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            if (vertical)
            {
                var sheen = Block(fill, "Tube side reflection", 0, 0, 0, 0,
                    new Color32(255, 233, 161, 80)).rectTransform;
                sheen.anchorMin = Vector2.zero;
                sheen.anchorMax = new Vector2(0f, 1f);
                sheen.offsetMin = new Vector2(3f, 0f);
                sheen.offsetMax = new Vector2(7f, 0f);
            }
            var gleam = Block(fill, "Level highlight", 0, 0, 0, 0, new Color32(255, 229, 145, 215)).rectTransform;
            gleam.anchorMin = vertical ? new Vector2(0f, 1f) : new Vector2(1f, 0f);
            gleam.anchorMax = Vector2.one;
            gleam.offsetMin = vertical ? new Vector2(0f, -2f) : new Vector2(-2f, 0f);
            gleam.offsetMax = Vector2.zero;
        }

        internal void Update(bool visible, float remaining)
        {
            if (Root.activeSelf != visible)
                Root.SetActive(visible);
            if (!visible)
                return;

            fill.anchorMax = Layout == ProstateLimitGaugeLayout.Vertical
                ? new Vector2(1f, remaining) : new Vector2(remaining, 1f);
            bool hasLiquid = remaining > 0f;
            if (fill.gameObject.activeSelf != hasLiquid)
                fill.gameObject.SetActive(hasLiquid);
            int percent = hasLiquid ? Mathf.Max(1, Mathf.RoundToInt(remaining * 100f)) : 0;
            bool warning = remaining <= .30f;
            bool pulse = rim != null && remaining <= .15f;
            if (shownPercent < 0 || warning != warningShown || pulse || pulsing)
            {
                pulsing = pulse;
                warningShown = warning;
                Color signal = Alarm;
                if (pulse)
                {
                    float breathe = .5f + .5f * Mathf.Sin(Time.time * 5.4f);
                    signal = Color.Lerp(Alarm, new Color32(255, 210, 103, 255), breathe * .58f);
                }
                liquid.color = warning ? signal : (Color)Amber;
                warningMark.color = warning ? signal : (Color)Bezel;
                readout.color = warning ? signal : (Color)Ink;
                if (rim != null)
                    rim.color = warning ? Color.Lerp(Rim, signal, .42f) : (Color)Rim;
            }
            if (percent == shownPercent)
                return;
            shownPercent = percent;
            readout.SetText("{0:0}%", percent);
        }

        private static TMP_Text Label(Transform parent, TMP_FontAsset font, string value, float x, float y,
            float width, float height, int size) =>
            MenuUi.Text(parent, font, value, x, y, width, height, size, Ink, TextAlignmentOptions.Center);

        private static Image Block(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            var image = MenuUi.Rect(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
