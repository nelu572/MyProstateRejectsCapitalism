using System.Collections.Generic;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class ExhibitionFocusVisuals : MonoBehaviour
    {
        private enum Look { Background, Rim, LocalKey, InkOutline, OffsetOutline }
        private enum Channel { Background, Rim, Key, Contact }

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int InspectionRim = Shader.PropertyToID("_InspectionRim");
        private static readonly int InspectionKey = Shader.PropertyToID("_InspectionKey");
        private static readonly int InspectionKeyWeight = Shader.PropertyToID("_InspectionKeyWeight");

        [SerializeField] private Look look;
        [SerializeField] private Renderer[] background;
        [SerializeField] private Renderer[] porcelain;
        [SerializeField] private Renderer[] plinth;
        [SerializeField] private Renderer[] contacts;
        [SerializeField] private Vector3 keyOffset = new(.6f, 1.48f, -1f);
        [SerializeField, Min(.1f)] private float keyRadius = 2.12f;
        [SerializeField] private MeshFilter[] outlineSources;
        [SerializeField] private Shader outlineShader;
        [SerializeField] private MeshFilter[] subduedOutlineSources;
        [SerializeField] private bool smoothSubduedOutlineSeams;
        [SerializeField, Min(0.1f)] private float outlineWidth = 4f;
        [SerializeField] private Vector2 outlineEchoOffset = new(6f, -4f);

        private readonly List<Override> active = new();
        private Vector4 key;
        private ExhibitionFocusOutline outline;
        private ExhibitionFocusOutline subduedOutline;

        private sealed class Override
        {
            internal readonly Renderer Renderer;
            internal readonly Channel Channel;
            internal readonly MaterialPropertyBlock Original = new();
            internal readonly MaterialPropertyBlock Working = new();
            internal readonly Color Color;

            internal Override(Renderer renderer, Channel channel)
            {
                Renderer = renderer;
                Channel = channel;
                renderer.GetPropertyBlock(Original);
                renderer.GetPropertyBlock(Working);
                if (channel == Channel.Background || channel == Channel.Contact)
                    Color = Original.HasColor(BaseColor) ? Original.GetColor(BaseColor)
                        : renderer.sharedMaterial.GetColor(BaseColor);
            }
        }

        internal void Begin()
        {
            Restore();
            if (look == Look.InkOutline || look == Look.OffsetOutline)
            {
                float width = look == Look.OffsetOutline ? outlineWidth : 3f;
                float echoWidth = look == Look.OffsetOutline ? .75f * width : 0f;
                outline ??= new ExhibitionFocusOutline(transform, outlineSources, outlineShader, width, echoWidth,
                    outlineEchoOffset);
                outline.Show();
                if (subduedOutlineSources is { Length: > 0 })
                {
                    const float scale = .60f;
                    subduedOutline ??= new ExhibitionFocusOutline(transform, subduedOutlineSources, outlineShader,
                        width * scale, echoWidth * scale, outlineEchoOffset * scale, smoothSubduedOutlineSeams);
                    subduedOutline.Show();
                }
                return;
            }
            if (look != Look.Rim)
                Capture(background, Channel.Background);
            if (look == Look.Rim)
                Capture(porcelain, Channel.Rim);
            else if (look == Look.LocalKey)
            {
                Capture(porcelain, Channel.Key);
                Capture(plinth, Channel.Key);
                Capture(contacts, Channel.Contact);
                Vector3 position = transform.TransformPoint(keyOffset);
                key = new Vector4(position.x, position.y, position.z, 1f / (keyRadius * keyRadius));
            }
        }

        internal void SetWeight(float weight)
        {
            if (outline != null)
            {
                outline.SetWeight(weight);
                subduedOutline?.SetWeight(weight);
                return;
            }
            var tint = look == Look.Background ? new Color(.86f, .88f, .91f, 1f) : new Color(.90f, .92f, .95f, 1f);
            foreach (var item in active)
            {
                if (item.Renderer == null)
                    continue;

                switch (item.Channel)
                {
                    case Channel.Background:
                        item.Working.SetColor(BaseColor, Color.LerpUnclamped(item.Color, item.Color * tint, weight));
                        break;
                    case Channel.Rim:
                        item.Working.SetFloat(InspectionRim, weight);
                        break;
                    case Channel.Key:
                        item.Working.SetVector(InspectionKey, key);
                        item.Working.SetFloat(InspectionKeyWeight, weight);
                        break;
                    case Channel.Contact:
                        var contact = item.Color;
                        contact.a = Mathf.Clamp01(contact.a * (1f + .46f * weight));
                        item.Working.SetColor(BaseColor, contact);
                        break;
                }
                item.Renderer.SetPropertyBlock(item.Working);
            }
        }

        internal void Restore()
        {
            outline?.Hide();
            subduedOutline?.Hide();
            foreach (var item in active)
                if (item.Renderer != null)
                    item.Renderer.SetPropertyBlock(item.Original.isEmpty ? null : item.Original);
            active.Clear();
        }

        private void Capture(Renderer[] renderers, Channel channel)
        {
            foreach (var renderer in renderers)
                if (renderer != null)
                    active.Add(new Override(renderer, channel));
        }

        private void OnDisable() => Restore();
        private void OnDestroy()
        {
            outline?.Destroy();
            subduedOutline?.Destroy();
        }
    }
}
