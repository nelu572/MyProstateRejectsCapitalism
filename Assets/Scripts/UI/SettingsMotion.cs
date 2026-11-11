using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class SettingsMotion : MonoBehaviour
    {
        private const float OpenSeconds = .25f;
        private const float CloseSeconds = .18f;
        private RectTransform panel;
        private CanvasGroup group;
        private Vector2 restingPosition;
        private float progress;
        private bool opening;
        private Action afterClose;

        private void Awake()
        {
            panel = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            restingPosition = panel.anchoredPosition;
            ApplyProgress();
        }

        internal void Show()
        {
            opening = true;
            gameObject.SetActive(true);
        }

        internal bool TryClose(Action onClosed)
        {
            if (!opening)
                return false;

            opening = false;
            afterClose = onClosed;
            group.interactable = false;
            return true;
        }

        private void Update()
        {
            if (opening && progress == 1f)
                return;

            progress = Mathf.MoveTowards(progress, opening ? 1f : 0f,
                Time.unscaledDeltaTime / (opening ? OpenSeconds : CloseSeconds));
            ApplyProgress();
            if (opening || progress > 0f)
                return;

            var callback = afterClose;
            afterClose = null;
            gameObject.SetActive(false);
            callback?.Invoke();
        }

        private void ApplyProgress()
        {
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            float scale = Mathf.Lerp(.98f, 1f, eased);
            panel.localScale = new Vector3(scale, scale, 1f);
            // The existing panel is anchored at the top left. Scale around its center.
            Vector2 centerOffset = Vector2.Scale(panel.sizeDelta, new Vector2(.5f, -.5f)) * (1f - scale);
            panel.anchoredPosition = restingPosition + centerOffset + Vector2.down * (10f * (1f - eased));
            group.alpha = eased;
            group.interactable = opening && progress == 1f;
        }
    }

    internal sealed class SettingsButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private Button button;
        private RectTransform lettering;
        private Vector2 rest;
        private float offset;
        private bool hovered;
        private bool pressed;

        internal static void Attach(Button target)
        {
            var motion = target.gameObject.AddComponent<SettingsButtonMotion>();
            motion.button = target;
            motion.lettering = target.GetComponentInChildren<TextMeshProUGUI>().rectTransform;
            motion.rest = motion.lettering.anchoredPosition;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && button.IsInteractable())
                pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                pressed = false;
        }

        private void Update()
        {
            float target = hovered && button.IsInteractable() ? (pressed ? -2.5f : 1.25f) : 0f;
            if (offset == target)
                return;

            offset = Mathf.MoveTowards(offset, target, 55f * Time.unscaledDeltaTime);
            lettering.anchoredPosition = rest + Vector2.up * offset;
        }

        private void OnDisable()
        {
            hovered = pressed = false;
            offset = 0f;
            if (lettering != null)
                lettering.anchoredPosition = rest;
        }
    }
}
