using System;
using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using MyProstateRejectsCapitalism.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Resolve F/Esc before the pause menu, movement, and the companion's free conversation.
    [DefaultExecutionOrder(-600)]
    [DisallowMultipleComponent]
    public sealed class ExhibitionInspection : MonoBehaviour
    {
        [Serializable]
        private sealed class Exhibit
        {
            public Transform root;
            public Transform view;
            public TMP_Text plaque;
            public Renderer plaqueBacking;
            public ExhibitionPanelDesign designOverride;
            public bool panelOnLeft;
            [Range(35f, 65f)] public float fieldOfView = 49f;
        }

        private enum Phase { Idle, Entering, Viewing, Leaving }
        private const float Duration = .36f;
        private const float CameraClearance = .14f;

        [SerializeField] private DialogueController dialogue;
        [SerializeField] private Canvas gameplayHud;
        [SerializeField] private bool useCatalogueCard;
        [SerializeField] private bool hideWorldPlaques;
        [SerializeField] private ExhibitionPanelDesign panelDesign;
        [SerializeField] private Exhibit[] exhibits;

        private readonly RaycastHit[] pathHits = new RaycastHit[8];
        private FirstPersonPlayer player;
        private PlayerInteractor interactor;
        private CharacterController body;
        private Camera viewCamera;
        private ExhibitionInspectionPanel panel;
        private Exhibit activeExhibit;
        private ExhibitionFocusVisuals focusVisuals;
        private Phase phase;
        private Pose originalLocalPose;
        private Pose originalWorldPose;
        private float originalFieldOfView;
        private CursorLockMode originalCursorLock;
        private bool originalCursorVisible;
        private bool hudWasEnabled;
        private Renderer hiddenPlaque;
        private bool plaqueWasHidden;
        private bool backingWasHidden;
        private float elapsed;
        private int restoredFrame = -1;

        internal bool CanInspect => TryGetExhibit(out _);
        internal bool SuppressesPause => phase != Phase.Idle || restoredFrame == Time.frameCount;

        private void Awake()
        {
            player = GetComponent<FirstPersonPlayer>();
            interactor = GetComponent<PlayerInteractor>();
            body = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (phase == Phase.Idle)
            {
                if (keyboard != null && keyboard.fKey.wasPressedThisFrame
                    && !keyboard.escapeKey.wasPressedThisFrame && TryGetExhibit(out var exhibit))
                    Begin(exhibit);
                return;
            }

            if (phase == Phase.Viewing)
            {
                if (keyboard != null && (keyboard.fKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
                {
                    elapsed = 0f;
                    phase = Phase.Leaving;
                }
                return;
            }

            elapsed = Mathf.Min(Duration, elapsed + Time.unscaledDeltaTime);
            float t = Mathf.SmoothStep(0f, 1f, elapsed / Duration);
            float inspectionWeight = phase == Phase.Entering ? t : 1f - t;
            var target = activeExhibit.view;
            viewCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(originalWorldPose.position, target.position, inspectionWeight),
                Quaternion.Slerp(originalWorldPose.rotation, target.rotation, inspectionWeight));
            viewCamera.fieldOfView = Mathf.Lerp(originalFieldOfView, activeExhibit.fieldOfView, inspectionWeight);
            panel.Opacity = Mathf.InverseLerp(.18f, .9f, inspectionWeight);
            if (focusVisuals != null)
                focusVisuals.SetWeight(inspectionWeight);

            if (elapsed < Duration)
                return;
            if (phase == Phase.Leaving)
                Restore(restoreCursor: true);
            else
                phase = Phase.Viewing;
        }

        private bool TryGetExhibit(out Exhibit result)
        {
            result = null;
            if (!isActiveAndEnabled || phase != Phase.Idle || restoredFrame == Time.frameCount
                || dialogue.IsInteractive || !body.isGrounded || !interactor.TryGetAimHit(out var aim))
                return false;

            foreach (var exhibit in exhibits)
            {
                if (!aim.collider.transform.IsChildOf(exhibit.root))
                    continue;
                if (!IsCameraPathClear(player.ViewCamera.transform.position, exhibit.view.position))
                    return false;
                result = exhibit;
                return true;
            }
            return false;
        }

        private bool IsCameraPathClear(Vector3 from, Vector3 to)
        {
            Vector3 path = to - from;
            float distance = path.magnitude;
            if (distance < .001f)
                return true;
            int hits = Physics.SphereCastNonAlloc(from, CameraClearance, path / distance, pathHits, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
                if (!pathHits[i].collider.transform.IsChildOf(player.transform))
                    return false;
            return true;
        }

        private void Begin(Exhibit exhibit)
        {
            var lines = exhibit.plaque.GetParsedText().Split(new[] { '\n' }, 4);
            if (lines.Length != 4)
                return;

            dialogue.StopAutomatic();
            viewCamera = player.ViewCamera;
            var view = viewCamera.transform;
            originalLocalPose = new Pose(view.localPosition, view.localRotation);
            originalWorldPose = new Pose(view.position, view.rotation);
            originalFieldOfView = viewCamera.fieldOfView;
            originalCursorLock = Cursor.lockState;
            originalCursorVisible = Cursor.visible;
            hudWasEnabled = gameplayHud.enabled;

            var design = exhibit.designOverride == ExhibitionPanelDesign.Original ? panelDesign : exhibit.designOverride;
            if (panel == null || panel.Design != design)
            {
                panel?.Destroy();
                panel = new ExhibitionInspectionPanel(exhibit.plaque.font, useCatalogueCard, design);
            }
            panel.Show(lines, exhibit.panelOnLeft);
            activeExhibit = exhibit;
            focusVisuals = exhibit.root.GetComponent<ExhibitionFocusVisuals>();
            if (focusVisuals != null)
                focusVisuals.Begin();
            if ((useCatalogueCard || hideWorldPlaques) && exhibit.plaqueBacking != null)
            {
                hiddenPlaque = exhibit.plaque.GetComponent<Renderer>();
                plaqueWasHidden = hiddenPlaque.forceRenderingOff;
                backingWasHidden = exhibit.plaqueBacking.forceRenderingOff;
                hiddenPlaque.forceRenderingOff = true;
                exhibit.plaqueBacking.forceRenderingOff = true;
            }
            phase = Phase.Entering;
            elapsed = 0f;
            player.SetExhibitInspectionLock(true);
            gameplayHud.enabled = false;
        }

        private void Restore(bool restoreCursor)
        {
            if (phase == Phase.Idle)
                return;
            phase = Phase.Idle;
            if (hiddenPlaque != null)
                hiddenPlaque.forceRenderingOff = plaqueWasHidden;
            if ((useCatalogueCard || hideWorldPlaques) && activeExhibit.plaqueBacking != null)
                activeExhibit.plaqueBacking.forceRenderingOff = backingWasHidden;
            hiddenPlaque = null;
            if (focusVisuals != null)
                focusVisuals.Restore();
            focusVisuals = null;
            activeExhibit = null;
            restoredFrame = Time.frameCount;
            panel.Hide();

            if (viewCamera != null)
            {
                viewCamera.transform.SetLocalPositionAndRotation(originalLocalPose.position, originalLocalPose.rotation);
                viewCamera.fieldOfView = originalFieldOfView;
            }
            if (gameplayHud != null)
                gameplayHud.enabled = hudWasEnabled;
            if (player != null)
                player.SetExhibitInspectionLock(false);
            if (restoreCursor && Application.isFocused)
            {
                Cursor.lockState = originalCursorLock;
                Cursor.visible = originalCursorVisible;
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                Restore(restoreCursor: false);
        }

        private void OnDisable() => Restore(restoreCursor: false);

        private void OnDestroy() => panel?.Destroy();
    }
}
