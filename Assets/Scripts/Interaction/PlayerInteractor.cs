using MyProstateRejectsCapitalism.Characters;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FirstPersonPlayer))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionMask = Physics.DefaultRaycastLayers;
        [SerializeField] private bool showDefaultHud = true;

        private FirstPersonPlayer player;
        private GUIStyle labelStyle;

        public IInteractable CurrentTarget { get; private set; }

        private void Awake()
        {
            player = GetComponent<FirstPersonPlayer>();
        }

        private void OnDisable()
        {
            CurrentTarget = null;
        }

        private void LateUpdate()
        {
            CurrentTarget = null;
            if (!player.CanControl || player.ViewCamera == null)
                return;

            var ray = player.ViewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (!Physics.Raycast(ray, out var hit, interactionDistance, interactionMask,
                QueryTriggerInteraction.Ignore))
                return;

            var target = hit.collider.GetComponentInParent<IInteractable>();
            if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                return;

            CurrentTarget = target;
            if (player.InteractionPressedThisFrame)
                target.Interact(gameObject);
        }

        private void OnGUI()
        {
            if (!showDefaultHud || player.InputBlocked)
                return;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18
                };
                labelStyle.normal.textColor = Color.white;
            }

            if (!player.CanControl)
            {
                GUI.Box(new Rect(Screen.width / 2f - 180f, Screen.height - 70f, 360f, 40f),
                    "Click to capture mouse | Esc to release");
                return;
            }

            GUI.Label(new Rect(Screen.width / 2f - 12f, Screen.height / 2f - 12f, 24f, 24f),
                "+", labelStyle);
            if (CurrentTarget is Component component && component != null && component.gameObject.activeInHierarchy
                && (!(component is Behaviour behaviour) || behaviour.isActiveAndEnabled))
            {
                var rect = new Rect(Screen.width / 2f - 180f, Screen.height - 90f, 360f, 44f);
                GUI.Box(rect, GUIContent.none);
                GUI.Label(rect, "E - " + CurrentTarget.InteractionPrompt, labelStyle);
            }
        }
    }
}
