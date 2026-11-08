using UnityEngine;
using UnityEngine.EventSystems;

namespace MyProstateRejectsCapitalism.UI
{
    internal sealed class MonumentMechanicalMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private Transform surface;
        private Vector3 origin, travel;
        private Quaternion rotation, pressedRotation;
        private bool hover, down;

        internal void Bind(Transform target, Vector3 distance, float angle)
        {
            surface = target;
            origin = target.localPosition;
            rotation = target.localRotation;
            travel = distance;
            pressedRotation = rotation * Quaternion.Euler(0, 0, angle);
        }

        public void OnPointerEnter(PointerEventData data) => hover = true;
        public void OnPointerExit(PointerEventData data) => hover = false;
        public void OnPointerDown(PointerEventData data) => down = data.button == PointerEventData.InputButton.Left;
        public void OnPointerUp(PointerEventData data) => down = false;

        private void Update()
        {
            float blend = 1 - Mathf.Exp(-27 * Time.unscaledDeltaTime);
            bool pressed = hover && down;
            surface.localPosition = Vector3.Lerp(surface.localPosition, origin + (pressed ? travel : Vector3.zero), blend);
            surface.localRotation = Quaternion.Slerp(surface.localRotation, pressed ? pressedRotation : rotation, blend);
        }

        private void OnDisable()
        {
            hover = down = false;
            if (surface == null)
                return;
            surface.localPosition = origin;
            surface.localRotation = rotation;
        }
    }
}
