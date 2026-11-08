using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal static class RecessedTitlePanel
    {
        internal static readonly Color32 Oxide = new(42, 39, 35, 255);
        internal static readonly Color32 Brass = new(177, 133, 75, 255);
        internal static readonly Color32 Bronze = new(104, 72, 47, 255);
        private static readonly Color32 MenuInk = new(48, 31, 26, 255);
        private static readonly Color32 MenuIvory = new(251, 236, 204, 255);

        internal static GameObject Build(Transform terminalMount, Transform[] insetSteps, Camera camera, TMP_FontAsset font,
            Action start, Action settings, Action quit, bool polished)
        {
            var housing = new GameObject("Recessed stair controls").transform;
            float projection = polished ? -.54f : -.49f;
            housing.SetPositionAndRotation(terminalMount.position - terminalMount.forward * (terminalMount.localScale.z * .5f + projection)
                + terminalMount.right * .15f + terminalMount.up * .35f, terminalMount.rotation);
            var material = terminalMount.GetComponent<Renderer>().sharedMaterial;
            const float cutWidth = 4.65f;
            foreach (var step in insetSteps)
                NotchStep(step, housing.position, cutWidth);

            TerminalPart(housing, "Cast insert", new Vector3(0, 0, .14f), new Vector3(4.59f, .88f, .26f),
                material, Bronze);
            float lipDepth = polished ? .27f : .22f;
            float lipThickness = polished ? .055f : .07f;
            Color32 lipFinish = polished ? new Color32(139, 104, 69, 255) : Brass;
            TerminalPart(housing, "Recess lintel", new Vector3(0, .441f, .005f), new Vector3(cutWidth, lipThickness, lipDepth),
                material, lipFinish);
            TerminalPart(housing, "Recess sill", new Vector3(0, -.445f, .003f), new Vector3(cutWidth, lipThickness, lipDepth),
                material, lipFinish);

            var panel = HardwareCanvas(camera, housing, new Vector3(0, 0, -.098f), 450, 87);
            Bar(panel, "Recess inner finish", 0, 0, 450, 87, Oxide);
            Bar(panel, "Cast index mark", 13, 15, 3, 55, Brass);
            if (polished)
            {
                foreach (float side in new[] { -1f, 1f })
                    TerminalPart(housing, "Cast return against stone", new Vector3(side * 2.282f, 0, .035f),
                        new Vector3(.082f, .86f, .30f), material, lipFinish);

                PhysicalKey(font, panel, "게임 시작", 24, 10, 234, 67, 37, start, true, 6, true);
                PhysicalKey(font, panel, "설정", 277, 17, 72, 53, 25, settings, false, 4, true);
                PhysicalKey(font, panel, "종료", 363, 17, 72, 53, 25, quit, false, 4, true);
                return housing.gameObject;
            }

            PhysicalKey(font, panel, "게임 시작", 25, 12, 230, 63, 36, start, true, 6);
            PhysicalKey(font, panel, "설정", 272, 15, 77, 57, 25, settings, false, 4);
            PhysicalKey(font, panel, "종료", 358, 15, 77, 57, 25, quit, false, 4);
            return housing.gameObject;
        }

        private static void NotchStep(Transform step, Vector3 recess, float width)
        {
            var size = step.localScale;
            var front = step.position - step.forward * (size.z * .5f);
            var center = Vector3.Dot(recess - step.position, step.right);
            var depth = Mathf.Clamp(Vector3.Dot(recess - front, step.forward) + .22f, 0, size.z);
            if (depth <= .001f)
                return;

            var renderer = step.GetComponent<Renderer>();
            var material = renderer.sharedMaterial;
            var color = material.GetColor("_BaseColor");
            var frame = new GameObject(step.name + " with terminal recess").transform;
            frame.SetParent(step.parent, false);
            frame.SetPositionAndRotation(step.position, step.rotation);
            renderer.enabled = false;

            var left = center - width * .5f;
            var right = center + width * .5f;
            var leftWidth = left + size.x * .5f;
            var rightWidth = size.x * .5f - right;
            TerminalPart(frame, "Left stone course", new Vector3(left - leftWidth * .5f, 0, 0),
                new Vector3(leftWidth, size.y, size.z), material, color);
            TerminalPart(frame, "Right stone course", new Vector3(right + rightWidth * .5f, 0, 0),
                new Vector3(rightWidth, size.y, size.z), material, color);
            if (depth < size.z)
                TerminalPart(frame, "Stone behind recess", new Vector3(center, 0, depth * .5f),
                    new Vector3(width, size.y, size.z - depth), material, color);
        }

        internal static RectTransform HardwareCanvas(Camera camera, Transform parent, Vector3 point, float w, float h)
        {
            var layer = new GameObject("Hardware controls", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var rect = layer.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localPosition = point;
            rect.localScale = Vector3.one * .01f;
            rect.sizeDelta = new Vector2(w, h);
            var canvas = layer.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            return rect;
        }

        internal static void PhysicalKey(TMP_FontAsset font, Transform parent, string title, float x, float y, float w, float h,
            float size, Action action, bool primary, float travel, bool polished = false)
        {
            var root = MenuUi.Rect(parent, title, x, y, w, h);
            var button = MenuUi.HitButton(root, action);
            Bar(root, "Key frame", 0, 0, w, h, Brass);
            Bar(root, "Dark key well", 2, 2, w - 4, h - 4, Oxide);
            var face = MenuUi.Rect(root, "Moving physical key", 4, 4, w - 8, h - 8);
            face.localPosition += Vector3.back * 3;
            var surface = Bar(face, "Key face", 0, 0, w - 8, h - 8, Color.white).GetComponent<MenuPlate>();
            Bar(face, "Bevel gleam", 1, 0, w - 10, 2, new Color32(255, 230, 170, 152));
            MenuUi.Text(face, font, title, 0, 0, w - 8, h - (polished ? 8 : 10), size, primary ? MenuInk : MenuIvory,
                TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            button.targetGraphic = surface;
            Color32 idle = primary ? new Color32(231, 196, 124, 255) : new Color32(91, 75, 54, 255);
            if (polished)
                idle = primary ? new Color32(242, 211, 147, 255) : new Color32(77, 65, 49, 255);
            MenuUi.Feedback(button, idle,
                primary ? new Color32(255, 235, 176, 255) : new Color32(145, 110, 65, 255),
                primary ? new Color32(184, 114, 53, 255) : new Color32(110, 67, 44, 255));
            root.gameObject.AddComponent<MonumentMechanicalMotion>().Bind(face, new Vector3(0, -2, travel), 0);
        }

        internal static void TerminalPart(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Color color)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Collider>().enabled = false;
            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var tint = new MaterialPropertyBlock();
            tint.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(tint);
        }

        private static RectTransform Bar(Transform parent, string name, float x, float y, float w, float h, Color color) =>
            MenuUi.Panel(parent, name, x, y, w, h, color, MenuPlate.Shape.Rectangle);
    }
}
