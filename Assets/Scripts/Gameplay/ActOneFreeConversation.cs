using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // ACT 1 review only. No persistent state or requirements for the next area.
    [DefaultExecutionOrder(400)]
    public sealed class ActOneFreeConversation : MonoBehaviour
    {
        public enum Location { PaidToilet, Corridor, NextArea }

        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private ActOneFirstEncounter firstEncounter;
        [SerializeField] private PaidToiletEvent paidToilet;
        [SerializeField] private SliceActFlow flow;
        [SerializeField] private TMP_Text talkHint;
        [Tooltip("PaidToilet, Corridor, NextArea 순서")]
        [SerializeField] private DialogueSequence[] conversations = new DialogueSequence[3];
        [SerializeField] private DialogueSequence[] repeatConversations = new DialogueSequence[3];
        [SerializeField] private Bounds toiletArea;
        [SerializeField] private Bounds nextArea;

        private readonly bool[] heard = new bool[3];
        private bool talking;
        private bool restoreInteractor;
        private int restoreAfterFrame;
        private bool previousInteractorEnabled;
        private Location lastLocation;
        private float hintUntil;
        private float availableAfter;

        public bool IsUnlocked { get; private set; }
        public bool IsTalking => talking;
        public Location CurrentLocation => nextArea.Contains(player.transform.position)
            ? Location.NextArea : toiletArea.Contains(player.transform.position)
                ? Location.PaidToilet : Location.Corridor;

        private void OnEnable()
        {
            dialogue.DialogueFinished += OnDialogueFinished;
            talkHint.text = string.Empty;
        }

        private void OnDialogueFinished()
        {
            if (talking)
            {
                talking = false;
                restoreInteractor = true;
                restoreAfterFrame = Time.frameCount;
                availableAfter = Time.unscaledTime + .35f;
            }
            else if (!IsUnlocked && firstEncounter.HasBegun && paidToilet.HasBeenUsed)
            {
                // The required introduction owns all input until its last line ends.
                IsUnlocked = true;
                lastLocation = CurrentLocation;
                hintUntil = Time.unscaledTime + 6f;
                availableAfter = Time.unscaledTime + .35f;
            }
        }

        private void Update()
        {
            if (restoreInteractor && Time.frameCount > restoreAfterFrame)
            {
                interactor.enabled = previousInteractorEnabled;
                restoreInteractor = false;
            }

            if (!IsUnlocked)
                return;
            if (lastLocation != CurrentLocation)
            {
                lastLocation = CurrentLocation;
                hintUntil = Time.unscaledTime + 4f;
            }

            var keyboard = Keyboard.current;
            if (!CanTalk || keyboard == null || !keyboard.fKey.wasPressedThisFrame)
                return;
            // Give an E interaction or cursor-capture click priority on the same frame.
            if (keyboard.eKey.wasPressedThisFrame
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
                return;

            int context = (int)CurrentLocation;
            var data = heard[context] ? repeatConversations : conversations;
            if (context >= data.Length || data[context] == null)
                return;

            previousInteractorEnabled = interactor.enabled;
            interactor.enabled = false;
            talking = true;
            if (dialogue.TryStart(data[context], player, null, allowMovement: true))
            {
                heard[context] = true;
                hintUntil = 0f;
            }
            else
            {
                talking = false;
                interactor.enabled = previousInteractorEnabled;
            }
        }

        private bool CanTalk => IsUnlocked && !talking && !restoreInteractor
            && !dialogue.IsPlaying && !firstEncounter.IsPaying && !flow.IsTransitioning
            && !flow.IsEndingVisible && player.CanControl && Time.unscaledTime >= availableAfter;

        private void LateUpdate()
        {
            // A brief reminder on unlock/area entry, rather than permanent companion UI.
            bool visible = CanTalk && Time.unscaledTime < hintUntil;
            talkHint.text = visible ? "F   말 걸기" : string.Empty;
            talkHint.alpha = Mathf.MoveTowards(talkHint.alpha, visible ? .62f : 0f,
                Time.unscaledDeltaTime / .2f);
        }

        private void OnDisable()
        {
            dialogue.DialogueFinished -= OnDialogueFinished;
            if ((talking || restoreInteractor) && interactor != null)
                interactor.enabled = previousInteractorEnabled;
            if (talkHint != null)
                talkHint.text = string.Empty;
        }
    }
}
