using MyProstateRejectsCapitalism.Characters;
using TMPro;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // The existing subtitle layout and typing stay intact; only contrast changes inside the white gallery.
    [DefaultExecutionOrder(1000)]
    public sealed class ExhibitionReadability : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private Bounds galleryArea;
        [SerializeField] private TMP_Text[] labels;

        private Color[] originalColors;
        private Material[] originalMaterials;
        private Material[] galleryMaterials;
        private bool inside;

        internal bool AppliesTo(FirstPersonPlayer owner) => isActiveAndEnabled && owner != null
            && owner == player && galleryArea.Contains(owner.transform.position);

        private void LateUpdate()
        {
            bool nowInside = AppliesTo(player);
            if (nowInside && !inside)
                ApplyGalleryMaterials();
            if (!nowInside && inside)
                RestoreLabels();
            inside = nowInside;
            if (!inside)
                return;

            foreach (var label in labels)
                if (label != null)
                    label.color = new Color(.05f, .065f, .08f, label.color.a);
        }

        private void ApplyGalleryMaterials()
        {
            if (galleryMaterials == null)
            {
                originalColors = new Color[labels.Length];
                originalMaterials = new Material[labels.Length];
                galleryMaterials = new Material[labels.Length];
            }

            for (int i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                if (label == null)
                    continue;
                originalColors[i] = label.color;
                originalMaterials[i] = label.fontSharedMaterial;
                var source = originalMaterials[i];
                if (source == null || !source.HasProperty("_UnderlayColor"))
                    continue;
                if (galleryMaterials[i] == null)
                {
                    var ink = new Material(source);
                    ink.EnableKeyword("UNDERLAY_ON");
                    ink.DisableKeyword("UNDERLAY_INNER");
                    ink.SetColor("_UnderlayColor", new Color(1f, 1f, 1f, .82f));
                    ink.SetFloat("_UnderlayOffsetX", 0f);
                    ink.SetFloat("_UnderlayOffsetY", 0f);
                    ink.SetFloat("_UnderlayDilate", .28f);
                    ink.SetFloat("_UnderlaySoftness", .12f);
                    if (ink.HasProperty("_FaceDilate"))
                        ink.SetFloat("_FaceDilate", .12f);
                    galleryMaterials[i] = ink;
                }
                label.fontSharedMaterial = galleryMaterials[i];
            }
        }

        private void RestoreLabels()
        {
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] != null)
                {
                    labels[i].color = new Color(originalColors[i].r, originalColors[i].g,
                        originalColors[i].b, labels[i].color.a);
                    labels[i].fontSharedMaterial = originalMaterials[i];
                }
        }

        private void OnDisable()
        {
            if (inside)
                RestoreLabels();
            inside = false;
        }

        private void OnDestroy()
        {
            if (galleryMaterials != null)
                foreach (var material in galleryMaterials)
                    if (material != null)
                        Destroy(material);
        }
    }
}
