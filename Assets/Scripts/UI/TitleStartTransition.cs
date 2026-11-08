using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal static class TitleStartTransition
    {
        internal static Action Bind(MonoBehaviour owner, RectTransform surface, MonumentTurntable turntable,
            Func<bool> canStart, Action start)
        {
            var curtain = MenuUi.Rect(surface, "Start transition", 0, 0, 0, 0);
            curtain.anchorMin = Vector2.zero;
            curtain.anchorMax = Vector2.one;
            var fade = curtain.gameObject.AddComponent<Image>();
            fade.color = new Color32(20, 13, 11, 0);
            curtain.gameObject.SetActive(false);
            return () =>
            {
                if (fade.gameObject.activeSelf || !canStart())
                    return;
                fade.gameObject.SetActive(true);
                owner.StartCoroutine(Play(fade, turntable, start));
            };
        }

        private static IEnumerator Play(Image fade, MonumentTurntable turntable, Action start)
        {
            const float fadeDelay = .16f;
            const float duration = .60f;
            if (turntable != null)
                turntable.EaseToStop(.43f);

            double started = Time.unscaledTimeAsDouble;
            var color = fade.color;
            float elapsed;
            do
            {
                elapsed = (float)(Time.unscaledTimeAsDouble - started);
                color.a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(fadeDelay, duration, elapsed));
                fade.color = color;
                yield return null;
            } while (elapsed < duration);

            start();
        }
    }
}
