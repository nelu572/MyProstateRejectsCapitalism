using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal static class MenuUi
    {
        internal static readonly Color32 Ink = new(19, 28, 27, 255);
        internal static readonly Color32 Paper = new(241, 230, 194, 255);
        internal static readonly Color32 Amber = new(238, 183, 80, 255);
        internal static readonly Color32 Mint = new(160, 215, 196, 255);
        internal static readonly Color32 Plate = new(41, 67, 63, 255);
        internal static readonly Color32 Muted = new(173, 186, 166, 255);

        internal static RectTransform CreateCanvas(string name, int sortOrder, out RectTransform surface)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scale = root.GetComponent<CanvasScaler>();
            scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(1920f, 1080f);
            scale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            surface = (RectTransform)root.transform;
            var layout = new GameObject("16:9 layout", typeof(RectTransform)).GetComponent<RectTransform>();
            layout.SetParent(surface, false);
            layout.sizeDelta = scale.referenceResolution;
            return layout;
        }

        internal static void EnsureEventSystem(Transform parent)
        {
            if (EventSystem.current != null || UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return;
            var input = new GameObject("Menu input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            input.transform.SetParent(parent, false);
        }

        internal static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        internal static RectTransform Panel(Transform parent, string name, float x, float y, float width, float height,
            Color color, MenuPlate.Shape shape = MenuPlate.Shape.Ticket)
        {
            var rect = Rect(parent, name, x, y, width, height);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var graphic = rect.gameObject.AddComponent<MenuPlate>();
            graphic.Profile = shape;
            graphic.color = color;
            graphic.raycastTarget = false;
            return rect;
        }

        internal static void Shade(RectTransform canvas, Color color)
        {
            var rect = Rect(canvas, "Backdrop", 0f, 0f, 0f, 0f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.SetAsFirstSibling();
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
        }

        internal static TextMeshProUGUI Text(Transform parent, TMP_FontAsset font, string text, float x, float y,
            float width, float height, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
        {
            var label = Rect(parent, text, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.text = text;
            label.color = color;
            label.alignment = align;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }

        internal static Button Button(Transform parent, TMP_FontAsset font, string title, float x, float y,
            float width, float height, Color color, Color textColor, MenuPlate.Shape shape, Action action,
            bool drawShadow = true)
        {
            if (drawShadow)
                Panel(parent, title + " shadow", x + 4f, y + 7f, width, height, new Color(0f, 0f, 0f, .65f), shape);
            var rect = Panel(parent, title, x, y, width, height, Color.white, shape);
            var graphic = rect.GetComponent<MenuPlate>();
            graphic.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = .09f,
                normalColor = color,
                highlightedColor = Color.Lerp(color, Color.white, .19f),
                selectedColor = Color.Lerp(color, Color.white, .19f),
                pressedColor = Color.Lerp(color, Ink, .30f),
                disabledColor = Color.Lerp(color, Ink, .48f)
            };
            graphic.CrossFadeColor(color, 0f, true, true);
            Text(rect, font, title, 8f, 0f, width - 16f, height, height > 88f ? 38f : 30f, textColor,
                TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            button.onClick.AddListener(() => action());
            return button;
        }

        internal static Button HitButton(RectTransform rect, Action action)
        {
            var hit = rect.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => action());
            return button;
        }

        internal static void Feedback(Button button, Color normal, Color hover, Color pressed)
        {
            var colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = hover;
            colors.selectedColor = hover;
            colors.pressedColor = pressed;
            colors.fadeDuration = .08f;
            button.colors = colors;
        }

        private static readonly Color32 ControlBlack = new(25, 20, 18, 255);
        private static readonly Color32 ControlIvory = new(239, 219, 179, 255);
        private static readonly Color32 ControlGold = new(241, 186, 73, 255);
        private static readonly Color32 ControlSteel = new(112, 83, 59, 255);
        private static readonly Color32 ControlRed = new(146, 48, 35, 255);

        private static readonly Vector2[] ChassisOutline =
        {
            new(.01f,.09f), new(.055f,.035f), new(.43f,.015f), new(.63f,.034f), new(.958f,.009f),
            new(.997f,.085f), new(.976f,.43f), new(.992f,.71f), new(.958f,.964f), new(.715f,.976f),
            new(.61f,.951f), new(.267f,.989f), new(.064f,.972f), new(.009f,.855f), new(.022f,.49f)
        };

        private static readonly Vector2[] PlateOutline =
        {
            new(0,.21f), new(.027f,.035f), new(.26f,.01f), new(.72f,.041f), new(.976f,.018f),
            new(1,.21f), new(.982f,.79f), new(.95f,.975f), new(.52f,.938f), new(.046f,1), new(.008f,.8f)
        };

        private static readonly Vector2[] DripOutline =
        {
            new(.31f,.02f), new(.66f,0), new(.91f,.13f), new(1,.37f), new(.7f,.65f), new(.77f,1),
            new(.18f,.96f), new(.36f,.61f), new(.06f,.32f), new(.04f,.14f)
        };

        internal static RectTransform BuildSettings(Transform parent, TMP_FontAsset font, float sensitivity, float speed,
            Action back)
        {
            var panel = Rect(parent, "Shared settings", 407, 106, 1106, 868);
            var print = panel.gameObject.AddComponent<SettingsPrint>();
            print.Initialize(font);
            var captionFont = LoadPanelLabelFont(font);
            var digitFont = Resources.Load<TMP_FontAsset>("Menus/PanelCounter-ChakraPetch") ?? font;
            ControlSurface(panel, "Casting shadow", 8, 15, 1106, 851, new Color32(17, 11, 9, 161), ChassisOutline);
            ControlSurface(panel, "Warped bronze chassis", 0, 5, 1106, 848, ControlSteel, ChassisOutline);
            ControlSurface(panel, "Painted iron inside the rim", 8, 12, 1089, 830, ControlBlack, ChassisOutline);
            ControlSurface(panel, "Exposed left seam", 34, 167, 5, 597, new Color32(78, 53, 38, 255));
            ControlSurface(panel, "Underplate", 45, 527, 799, 223, new Color32(77, 46, 31, 255), PlateOutline);
            Screw(panel, 42, 179);
            Screw(panel, 1032, 175);
            Screw(panel, 42, 746);
            Screw(panel, 990, 791);

            AddSettingsHeader(panel, font, captionFont, print);
            AddSlider(panel, font, digitFont, captionFont, print, "마우스 감도", 70, 188, MenuSettings.MinSensitivity, MenuSettings.MaxSensitivity,
                sensitivity, false, MenuSettings.SetSensitivity);
            AddSlider(panel, font, digitFont, captionFont, print, "자막 속도", 110, 356, MenuSettings.MinSubtitleSpeed, MenuSettings.MaxSubtitleSpeed,
                speed, true, MenuSettings.SetSubtitleSpeed);
            AddDisplaySwitches(panel, font, captionFont, print);

            Text(panel, captionFont, "ESC", 91, 760, 60, 47, 22, ControlGold);
            Text(panel, captionFont, "돌아가기", 153, 760, 220, 47, 25, ControlIvory);
            var release = ControlSurface(panel, "Offset red return bracket", 836, 660, 216, 105, ControlRed, PlateOutline);
            ControlSurface(release, "Return lever shadow", 12, 17, 200, 70, ControlBlack, PlateOutline);
            var backButton = Button(release, font, "돌아가기", 5, 12, 197, 69, ControlIvory, ControlBlack,
                MenuPlate.Shape.Terminal, back, drawShadow: false);
            print.ApplyWord(backButton.GetComponentInChildren<TextMeshProUGUI>());
            SettingsPrint.SetButtonContrast(backButton, lightSurface: true);
            Feedback(backButton, ControlIvory, new Color32(255, 235, 181, 255), new Color32(211, 147, 68, 255));
            SettingsButtonMotion.Attach(backButton);
            return panel;
        }

        internal static TMP_FontAsset LoadPanelLabelFont(TMP_FontAsset fallback) =>
            Resources.Load<TMP_FontAsset>("Menus/PanelLabel-Galmuri") ?? fallback;

        private static void AddSettingsHeader(Transform panel, TMP_FontAsset font, TMP_FontAsset captionFont,
            SettingsPrint print)
        {
            var plaque = ControlSurface(panel, "Crooked enamel nameplate", 49, 10, 493, 150,
                new Color32(115, 63, 29, 255), PlateOutline);
            plaque.localRotation = Quaternion.Euler(0, 0, 2.5f);
            ControlSurface(plaque, "Worn yellow enamel", 0, 0, 484, 141, ControlGold, PlateOutline);
            ControlSurface(plaque, "Drip from nameplate", 91, 133, 13, 30, ControlGold, DripOutline);
            ControlSurface(plaque, "Long drip from nameplate", 158, 134, 11, 38, ControlGold, DripOutline);
            ControlSurface(plaque, "Short enamel scrape", 305, 108, 104, 2, new Color32(151, 95, 37, 210));
            ControlSurface(plaque, "Enamel scrape edge", 327, 112, 76, 1, new Color32(255, 220, 123, 255));
            ControlSurface(plaque, "Enamel chipped at upper edge", 256, 6, 55, 6,
                new Color32(123, 68, 31, 230), PlateOutline);
            ControlSurface(plaque, "Enamel chipped at corner", 446, 108, 24, 11,
                new Color32(123, 68, 31, 230), PlateOutline);
            Screw(plaque, 442, 28);
            if (!print.TryTitle(plaque))
            {
                TitleSyllable(plaque, font, print, "설", 28, -3, 104, 2f);
                TitleSyllable(plaque, font, print, "정", 127, 1, 100, -3f);
            }
            Text(plaque, captionFont, "SERVICE / 05", 262, 44, 166, 51, 23, ControlBlack);

            var warning = ControlSurface(panel, "Battered warning sticker", 765, 30, 275, 58, ControlRed, PlateOutline);
            warning.localRotation = Quaternion.Euler(0, 0, -4);
            Text(warning, captionFont, "OUT OF ORDER", 15, 0, 245, 56, 25, ControlIvory, TextAlignmentOptions.Center);
            Text(panel, captionFont, "변경 사항은 즉시 적용됩니다", 581, 105, 470, 46, 26, ControlIvory,
                TextAlignmentOptions.MidlineRight);
        }

        private static void TitleSyllable(Transform plaque, TMP_FontAsset font, SettingsPrint print, string syllable, float x, float y,
            float size, float angle)
        {
            var glyph = Rect(plaque, syllable + " paint", x, y, 128, 133);
            glyph.localRotation = Quaternion.Euler(0, 0, angle);
            Text(glyph, font, syllable, 5, -14, 128, 173, size, new Color32(157, 77, 33, 255)).fontStyle = FontStyles.Bold;
            var face = Text(glyph, font, syllable, 0, -20, 128, 173, size, ControlBlack);
            face.fontStyle = FontStyles.Bold;
            print.Apply(face, headline: true);
        }

        private static void AddDisplaySwitches(Transform panel, TMP_FontAsset font, TMP_FontAsset captionFont,
            SettingsPrint print)
        {
            var module = ControlSurface(panel, "Display switch mounting plate", 62, 539, 754, 198, ControlSteel, PlateOutline);
            ControlSurface(module, "Display switch recess", 7, 6, 738, 184, new Color32(54, 39, 31, 255), PlateOutline);
            Screw(module, 13, 28);
            Screw(module, 718, 147);
            var modeLabel = Text(module, font, "화면 모드", 34, 22, 289, 48, 34, ControlGold);
            modeLabel.fontStyle = FontStyles.Bold;
            print.ApplyWord(modeLabel);
            var status = Text(module, captionFont, "", 365, 23, 325, 45, 26, ControlGold, TextAlignmentOptions.MidlineRight);
            var fullscreen = DisplaySwitch(module, font, "전체화면", 34, () => MenuSettings.SetFullscreen(true));
            var windowed = DisplaySwitch(module, font, "창모드", 367, () => MenuSettings.SetFullscreen(false));
            print.ApplyWord(fullscreen.GetComponentInChildren<TextMeshProUGUI>());
            print.ApplyWord(windowed.GetComponentInChildren<TextMeshProUGUI>());
            SettingsButtonMotion.Attach(fullscreen);
            SettingsButtonMotion.Attach(windowed);
            void ShowMode(bool full)
            {
                status.text = full ? "현재: 전체화면" : "현재: 창모드";
                StyleDisplaySwitch(fullscreen, full);
                StyleDisplaySwitch(windowed, !full);
            }
            fullscreen.onClick.AddListener(() => ShowMode(true));
            windowed.onClick.AddListener(() => ShowMode(false));
            ShowMode(MenuSettings.Fullscreen);
        }

        private static Button DisplaySwitch(Transform module, TMP_FontAsset font, string label, float x, Action action)
        {
            ControlSurface(module, label + " switch well", x, 87, 324, 75, ControlBlack, PlateOutline);
            return Button(module, font, label, x + 7, 92, 310, 62, ControlSteel, ControlIvory,
                MenuPlate.Shape.Terminal, action, drawShadow: false);
        }

        private static void StyleDisplaySwitch(Button button, bool selected)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = selected ? ControlBlack : ControlGold;
            SettingsPrint.SetButtonContrast(button, lightSurface: selected);
            if (selected)
                Feedback(button, ControlGold, new Color32(255, 213, 121, 255), new Color32(198, 124, 45, 255));
            else
                Feedback(button, new Color32(84, 57, 42, 255), new Color32(123, 77, 45, 255), ControlRed);
        }

        private static void AddSlider(RectTransform panel, TMP_FontAsset font, TMP_FontAsset digitFont,
            TMP_FontAsset captionFont, SettingsPrint print, string title, float x, float y, float min, float max, float current,
            bool wholeNumbers, Action<float> apply)
        {
            var casing = ControlSurface(panel, title + " dented housing", x, y, 918, 150, ControlSteel, PlateOutline);
            ControlSurface(casing, "Recessed iron face", 6, 6, 906, 136, new Color32(48, 36, 30, 255), PlateOutline);
            Screw(casing, 14, 16);
            Screw(casing, 893, 111);
            ControlSurface(casing, "Painted scale seam", 283, 112, 418, 1, new Color32(151, 113, 74, 204));
            for (int i = 0; i < 9; i++)
                ControlSurface(casing, "Calibration notch", 284 + 51 * i, 107, 1, 5, ControlSteel);
            var label = Text(casing, font, title, 36, 43, 245, 62, 34, ControlGold);
            label.fontStyle = FontStyles.Bold;
            print.ApplyWord(label);
            ControlSurface(casing, "Counter rim", 711, 44, 171, 68, ControlSteel, PlateOutline);
            ControlSurface(casing, "Counter recess", 716, 48, 162, 59, ControlBlack, PlateOutline);
            float digitsWidth = wholeNumbers ? 74f : 153f;
            var align = wholeNumbers ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Center;
            var lip = CounterDigits(casing, digitFont, "Counter bevel", 47.8f, digitsWidth, align,
                new Color32(255, 225, 157, 255));
            var recess = CounterDigits(casing, digitFont, "Counter shadow", 44.5f, digitsWidth, align,
                new Color32(95, 52, 26, 255));
            var number = CounterDigits(casing, digitFont, "Counter digits", 46f, digitsWidth, align, ControlGold);
            if (wholeNumbers)
                Text(casing, captionFont, "자/초", 803, 48, 69, 60, 22, ControlGold);

            var rect = Rect(casing, title + " slider", 281, 43, 421, 63);
            ControlSurface(rect, "Cast track rim", 0, 17, 421, 30, ControlSteel, PlateOutline);
            ControlSurface(rect, "Track well", 4, 21, 413, 23, ControlBlack, PlateOutline);
            var fillArea = Rect(rect, "Fill area", 22, 29, 377, 7);
            var fill = ControlSurface(fillArea, "Fill", 0, 0, 377, 7, ControlGold);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var handleArea = Rect(rect, "Handle area", 22, 0, 377, 63);
            var handle = ControlSurface(handleArea, "Handle", 0, 0, 40, 55, Color.white, PlateOutline);
            handle.anchorMin = handle.anchorMax = new Vector2(.5f, .5f);
            handle.pivot = new Vector2(.5f, .5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(40, -8);
            for (int i = 0; i < 3; i++)
                ControlSurface(handle, "Grip channel", 11 + 8 * i, 14, 2, 27, new Color32(77, 49, 32, 176));

            var hit = rect.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var slider = rect.gameObject.AddComponent<Slider>();
            slider.targetGraphic = handle.GetComponent<MenuPlate>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = slider.colors;
            colors.normalColor = ControlIvory;
            colors.highlightedColor = new Color32(255, 219, 134, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color32(201, 130, 53, 255);
            slider.colors = colors;
            slider.SetValueWithoutNotify(current);
            void Show(float value)
            {
                string digits = wholeNumbers ? Mathf.RoundToInt(value).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                number.text = lip.text = recess.text = digits;
            }
            Show(current);
            slider.onValueChanged.AddListener(value => { apply(value); Show(value); });
        }

        private static TextMeshProUGUI CounterDigits(Transform parent, TMP_FontAsset font, string name, float y,
            float width, TextAlignmentOptions alignment, Color color)
        {
            var text = Text(parent, font, string.Empty, 720, y, width, 60, 36, color, alignment);
            text.name = name;
            text.characterSpacing = 1.5f;
            return text;
        }

        private static RectTransform ControlSurface(Transform parent, string name, float x, float y,
            float width, float height, Color color, Vector2[] outline = null)
        {
            var rect = Panel(parent, name, x, y, width, height, color, MenuPlate.Shape.Rectangle);
            if (outline != null)
                rect.GetComponent<MenuPlate>().Outline = outline;
            return rect;
        }

        private static void Screw(Transform plate, float x, float y)
        {
            var screw = Panel(plate, "Recessed fastener", x, y, 14, 14, ControlSteel, MenuPlate.Shape.Seal);
            Panel(screw, "Screw slot", 3, 6, 8, 2, ControlBlack, MenuPlate.Shape.Rectangle).localRotation =
                Quaternion.Euler(0, 0, 25);
        }
    }

    internal sealed class MenuPlate : MaskableGraphic
    {
        internal enum Shape { Rectangle, Ticket, Terminal, Flush, Seal }

        private static readonly Vector2[] Rectangle = { new(0,0), new(1,0), new(1,1), new(0,1) };
        private static readonly Vector2[] Ticket = { new(0,.10f), new(.015f,0), new(.975f,0), new(1,.14f),
            new(1,.88f), new(.981f,1), new(.018f,1), new(0,.88f) };
        private static readonly Vector2[] Terminal = { new(0,.24f), new(.10f,0), new(.92f,0), new(1,.23f),
            new(1,.81f), new(.91f,1), new(.11f,1), new(0,.82f) };
        private static readonly Vector2[] Flush = { new(0,.36f), new(.044f,.14f), new(.112f,.024f), new(.22f,0),
            new(.83f,0), new(.928f,.058f), new(.98f,.22f), new(1,.49f), new(.97f,.77f), new(.91f,.93f),
            new(.81f,1), new(.20f,1), new(.086f,.93f), new(.026f,.76f), new(0,.58f) };
        private static readonly Vector2[] Seal = { new(0,.46f), new(.09f,.15f), new(.29f,.03f), new(.61f,0),
            new(.84f,.10f), new(.98f,.27f), new(1,.59f), new(.87f,.86f), new(.67f,.98f),
            new(.31f,1), new(.12f,.88f), new(.03f,.68f) };

        internal Shape Profile { get; set; }
        internal Vector2[] Outline { get; set; }

        internal MenuPlate()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            var points = Outline ?? (Profile switch
            {
                Shape.Ticket => Ticket, Shape.Terminal => Terminal, Shape.Flush => Flush, Shape.Seal => Seal, _ => Rectangle
            });
            mesh.AddVert(rect.center, color, Vector2.zero);
            foreach (var point in points)
                mesh.AddVert(rect.min + Vector2.Scale(rect.size, point), color, Vector2.zero);
            for (int i = 0; i < points.Length; i++)
                mesh.AddTriangle(0, (i + 1) % points.Length + 1, i + 1);
        }
    }
}
