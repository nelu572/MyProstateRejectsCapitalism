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
        private bool inside;

        private void LateUpdate()
        {
            bool nowInside = player != null && galleryArea.Contains(player.transform.position);
            if (nowInside && !inside)
            {
                originalColors = new Color[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                    if (labels[i] != null)
                        originalColors[i] = labels[i].color;
            }
            if (!nowInside && inside)
                RestoreColors();
            inside = nowInside;
            if (!inside)
                return;

            foreach (var label in labels)
                if (label != null)
                    label.color = new Color(.13f, .15f, .16f, label.color.a);
        }

        private void RestoreColors()
        {
            if (originalColors == null)
                return;
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] != null)
                    labels[i].color = new Color(originalColors[i].r, originalColors[i].g,
                        originalColors[i].b, labels[i].color.a);
        }

        private void OnDisable()
        {
            if (inside)
                RestoreColors();
            inside = false;
        }
    }
}
