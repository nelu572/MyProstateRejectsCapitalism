using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal static class PauseMenuArt
    {
        internal static readonly Color PauseShade = new(.026f, .015f, .010f, .56f);
        private static readonly Color32 Iron = new(30, 23, 20, 255);
        private static readonly Color32 Bronze = new(130, 91, 58, 255);
        private static readonly Color32 Gold = new(241, 186, 73, 255);
        private static readonly Color32 Ivory = new(239, 219, 179, 255);
        private static readonly Color32 Red = new(137, 49, 34, 255);

        private static readonly Vector2[] Casting =
        {
            new(.007f,.14f), new(.047f,.047f), new(.24f,.023f), new(.54f,.048f), new(.90f,.012f),
            new(.991f,.07f), new(.974f,.61f), new(.991f,.86f), new(.953f,.974f), new(.55f,.951f),
            new(.31f,.984f), new(.063f,.963f), new(.022f,.842f)
        };

        private static readonly Vector2[] Enamel =
        {
            new(0,.23f), new(.043f,.057f), new(.43f,.011f), new(.972f,.046f), new(1,.21f),
            new(.975f,.86f), new(.943f,.985f), new(.55f,.951f), new(.08f,.986f), new(.015f,.75f)
        };

        private static readonly Vector2[] Runoff =
        {
            new(.28f,.04f), new(.62f,0), new(.94f,.24f), new(.76f,.56f),
            new(.80f,1), new(.19f,.99f), new(.26f,.52f), new(.02f,.28f)
        };

        internal static RectTransform Build(Transform parent, TMP_FontAsset font, Action resume, Action settings, Action title)
        {
            var pause = Resources.Load<Texture2D>("Menus/PauseMeltLettering");
            var shared = Resources.Load<Texture2D>("Menus/SettingsMeltLettering");
            var face = MenuUi.Rect(parent, "Pause control panel", 620, 248, 680, 584);
            Plate(face, "Cast shadow", 9, 13, 680, 575, new Color32(15, 8, 6, 175), Casting);
            Plate(face, "Bent bronze rim", 0, 0, 680, 575, Bronze, Casting);
            Plate(face, "Iron face", 7, 7, 666, 561, Iron, Casting);
            Plate(face, "Red primer at lower edge", 493, 542, 132, 4, Red);
            Fastener(face, 28, 172);
            Fastener(face, 637, 149);
            Fastener(face, 628, 521);

            var header = Plate(face, "Bent yellow nameplate", 43, 20, 438, 124, new Color32(137, 77, 39, 255), Enamel);
            header.localRotation = Quaternion.Euler(0, 0, 2.4f);
            Plate(header, "Face enamel", 0, 0, 429, 116, Gold, Enamel);
            Plate(header, "Enamel run at the edge", 94, 109, 20, 32, Gold, Runoff);
            Plate(header, "Scuffed edge", 363, 98, 39, 2, new Color32(162, 97, 43, 255));
            Fastener(header, 401, 20);
            Letter(header, font, pause, "일시정지", new RectInt(0, 0, 748, 244),
                new Rect(29, 6, 332, 108), light: false, fallbackSize: 64);
            var tag = Plate(face, "Interrupt tag", 497, 58, 131, 42, Red, Enamel);
            tag.localRotation = Quaternion.Euler(0, 0, -4);
            MenuUi.Text(tag, font, "PAUSE / 01", 3, 0, 125, 40, 19, Ivory, TextAlignmentOptions.Center);

            Control(face, font, "계속하기", new Rect(67, 186, 544, 123), pause,
                new RectInt(0, 256, 748, 244), 190, true, resume);
            Control(face, font, "설정", new Rect(67, 354, 177, 89), shared,
                new RectInt(0, 0, 508, 250), 80, false, settings);
            Control(face, font, "메인화면으로", new Rect(262, 354, 349, 89), pause,
                new RectInt(0, 512, 1112, 244), 238, false, title);
            Plate(face, "Footer seam", 68, 481, 542, 1, new Color32(102, 70, 47, 255));
            MenuUi.Text(face, font, "ESC", 69, 494, 62, 42, 23, Gold);
            Letter(face, font, pause, "계속하기", new RectInt(0, 256, 748, 244),
                new Rect(134, 502, 81, 26), light: false, fallbackSize: 18);
            return face;
        }

        private static void Control(Transform parent, TMP_FontAsset font, string name, Rect area, Texture2D atlas,
            RectInt source, float textWidth, bool primary, Action action)
        {
            float w = area.width, h = area.height;
            var hit = MenuUi.Rect(parent, name, area.x, area.y, w, h);
            var button = MenuUi.HitButton(hit, action);
            Plate(hit, "Cast key surround", 0, 0, w, h, Bronze, Enamel);
            Plate(hit, "Deep key bed", 5, 4, w - 10, h - 8, new Color32(16, 11, 9, 255), Enamel);
            var moving = MenuUi.Rect(hit, "Moving pause key", 8, 6, w - 16, h - 18);
            var surface = Plate(moving, "Key surface", 0, 0, w - 16, h - 18, Color.white, Enamel);
            Plate(moving, "Top bevel", 14, 6, w - 45, 2, new Color32(255, 219, 157, 102));
            float textHeight = textWidth * source.height / source.width;
            Letter(moving, font, atlas, name, source, new Rect((w - 16 - textWidth) * .5f,
                (h - 18 - textHeight) * .5f, textWidth, textHeight), primary, primary ? 45 : 32);
            button.targetGraphic = surface.GetComponent<MenuPlate>();
            if (primary)
                MenuUi.Feedback(button, Ivory, new Color32(255, 236, 179, 255), new Color32(215, 155, 78, 255));
            else
                MenuUi.Feedback(button, new Color32(74, 47, 34, 255), new Color32(114, 70, 39, 255),
                    new Color32(150, 73, 38, 255));
            hit.gameObject.AddComponent<MonumentMechanicalMotion>().Bind(moving, Vector3.down * 6, 0);
        }

        private static void Letter(Transform parent, TMP_FontAsset font, Texture2D atlas, string name, RectInt source,
            Rect placement, bool light, float fallbackSize)
        {
            if (atlas != null)
            {
                var ink = SettingsPrint.DrawLettering(parent, name + " artwork", atlas, source, placement);
                ink.color = light ? new Color32(113, 79, 45, 255) : Color.white;
                return;
            }
            MenuUi.Text(parent, font, name, placement.x, placement.y, placement.width, placement.height, fallbackSize,
                light ? Iron : Ivory, TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
        }

        private static RectTransform Plate(Transform parent, string name, float x, float y, float w, float h,
            Color color, Vector2[] outline = null)
        {
            var plate = MenuUi.Panel(parent, name, x, y, w, h, color, MenuPlate.Shape.Rectangle);
            if (outline != null)
                plate.GetComponent<MenuPlate>().Outline = outline;
            return plate;
        }

        private static void Fastener(Transform parent, float x, float y)
        {
            var screw = MenuUi.Panel(parent, "Screw head", x, y, 13, 13, Bronze, MenuPlate.Shape.Seal);
            Plate(screw, "Screw slot", 3, 6, 8, 2, Iron).localRotation = Quaternion.Euler(0, 0, -21);
        }
    }
}
