using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Characters
{
    // Local controls for the development comparison scene only.
    public sealed class PlayerFeelComparison : MonoBehaviour, IInteractable
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private DialogueSequence inputLockCheck;
        [SerializeField] private Vector3 startPosition = new Vector3(0f, .03f, -8f);
        [SerializeField] private bool sceneControlsOnly;
        [SerializeField] private bool showOverlay = true;

        public string ActivePreset { get; private set; } = "B";
        public int InteractionCount { get; private set; }
        public string InteractionPrompt => "Input lock check";

        private void Start()
        {
            SelectPreset(1);
            player.ViewCamera.fieldOfView = 80f;
            if (sceneControlsOnly)
                player.JumpEnabled = true;
        }

        public void SelectPreset(int index)
        {
            switch (index)
            {
                case 0:
                    ActivePreset = "A";
                    player.SetMovementTuning(3.8f, 48f, 60f, 72f, .65f, 24f);
                    break;
                case 2:
                    ActivePreset = "C";
                    player.SetMovementTuning(3.5f, 16f, 22f, 28f, .8f, 20f);
                    break;
                default:
                    ActivePreset = "B";
                    player.SetMovementTuning(3.5f, 28f, 36f, 48f, .75f, 22f);
                    break;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.f9Key.wasPressedThisFrame)
                showOverlay = !showOverlay;
            if (player.InputBlocked)
            {
                if (!sceneControlsOnly && dialogue.IsPlaying && keyboard.f8Key.wasPressedThisFrame)
                    player.SetDialogueLookAllowed(!player.DialogueLookAllowed);
                return;
            }
            if (sceneControlsOnly && !player.CanControl)
                return;
            if (keyboard.f1Key.wasPressedThisFrame) SelectPreset(0);
            if (keyboard.f2Key.wasPressedThisFrame) SelectPreset(1);
            if (keyboard.f3Key.wasPressedThisFrame) SelectPreset(2);
            if (keyboard.f5Key.wasPressedThisFrame) player.ViewCamera.fieldOfView = 75f;
            if (keyboard.f6Key.wasPressedThisFrame) player.ViewCamera.fieldOfView = 80f;
            if (keyboard.f7Key.wasPressedThisFrame) player.ViewCamera.fieldOfView = 85f;
            if (keyboard.jKey.wasPressedThisFrame) player.JumpEnabled = !player.JumpEnabled;
            if (!sceneControlsOnly && keyboard.rKey.wasPressedThisFrame)
            {
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.SetPositionAndRotation(startPosition, Quaternion.identity);
                player.ResetMotion();
                controller.enabled = true;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (sceneControlsOnly || interactor != player.gameObject || !player.CanControl)
                return;
            if (dialogue.TryStart(inputLockCheck, player, null))
            {
                InteractionCount++;
                dialogue.HoldCurrentLine(.4f);
            }
        }

        private void OnGUI()
        {
            if (!showOverlay)
                return;
            if (sceneControlsOnly)
            {
                GUI.Box(new Rect(12f, 12f, 390f, 30f), string.Empty);
                GUI.Label(new Rect(22f, 18f, 370f, 22f),
                    "DEV   " + ActivePreset + "   |   FOV " + player.ViewCamera.fieldOfView.ToString("F0")
                    + "   |   Jump " + (player.JumpEnabled ? "ON" : "OFF") + "   |   F9 hide/show");
                return;
            }
            GUI.Box(new Rect(16f, 16f, 620f, 143f), string.Empty);
            GUI.Label(new Rect(30f, 27f, 600f, 24f),
                "Preset " + ActivePreset + "   |   FOV " + player.ViewCamera.fieldOfView.ToString("F0")
                + "   |   Jump " + (player.JumpEnabled ? "ON" : "OFF"));
            GUI.Label(new Rect(30f, 53f, 600f, 24f), "F1 / F2 / F3: A / B / C    |    F5 / F6 / F7: 75 / 80 / 85 FOV");
            GUI.Label(new Rect(30f, 79f, 600f, 24f), "WASD + mouse    |    Space: jump    |    J: jump toggle    |    R: start position");
            GUI.Label(new Rect(30f, 105f, 600f, 24f), "E: lock-check station    |    F8 during dialogue: look-only exception");
            GUI.Label(new Rect(30f, 129f, 600f, 24f), "Esc: release cursor    |    Click: capture cursor    |    Interactions: " + InteractionCount);
        }
    }
}
