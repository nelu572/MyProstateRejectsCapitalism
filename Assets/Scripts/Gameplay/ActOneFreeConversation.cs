using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Gameplay
{
    [DefaultExecutionOrder(400)]
    public sealed class ActOneFreeConversation : MonoBehaviour
    {
        public enum Location { PaidToilet, Corridor, NextArea }

        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private ActOneFirstEncounter firstEncounter;
        [SerializeField] private PaidToiletEvent paidToilet;
        [SerializeField] private bool unlockOnArrival;
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
        private ExhibitionReadability exhibitionReadability;
        private ExhibitionInspection inspection;

        public bool IsUnlocked { get; private set; }
        public bool IsTalking => talking;
        public Location CurrentLocation => nextArea.Contains(player.transform.position)
            ? Location.NextArea : toiletArea.Contains(player.transform.position)
                ? Location.PaidToilet : Location.Corridor;

        private void OnEnable()
        {
            exhibitionReadability = FindFirstObjectByType<ExhibitionReadability>();
            inspection = player.GetComponent<ExhibitionInspection>();
            dialogue.DialogueFinished += OnDialogueFinished;
            talkHint.text = string.Empty;
            if (unlockOnArrival)
                Unlock();
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
            else if (!IsUnlocked && dialogue.LastDialogueCompleted && firstEncounter.HasBegun && paidToilet.HasBeenUsed)
            {
                // The required introduction owns all input until its last line ends.
                Unlock();
            }
        }

        private void Unlock()
        {
            IsUnlocked = true;
            lastLocation = CurrentLocation;
            hintUntil = Time.unscaledTime + 6f;
            availableAfter = Time.unscaledTime + .35f;
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
            // A focused exhibit owns F; a simultaneous click belongs to interaction.
            if (player.InteractionClickPressed || (inspection != null && inspection.CanInspect))
                return;

            int context = (int)CurrentLocation;
            var data = heard[context] ? repeatConversations : conversations;
            if (context >= data.Length || data[context] == null)
                return;

            previousInteractorEnabled = interactor.enabled;
            interactor.enabled = false;
            if (dialogue.TryStart(data[context], player, null, allowMovement: true))
            {
                talking = true;
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
            && !dialogue.IsInteractive && (firstEncounter == null || !firstEncounter.IsPaying)
            && (flow == null || (!flow.IsTransitioning && !flow.IsEndingVisible))
            && player.CanControl && Time.unscaledTime >= availableAfter;

        private void LateUpdate()
        {
            // A brief reminder on unlock/area entry, rather than permanent companion UI.
            bool inspectable = inspection != null && inspection.CanInspect;
            bool visible = inspectable || (CanTalk && Time.unscaledTime < hintUntil);
            talkHint.text = inspectable ? "F — 작품 관람" : visible ? "F   말 걸기" : string.Empty;
            bool inGallery = exhibitionReadability != null && exhibitionReadability.AppliesTo(player);
            float opacity = inGallery ? .94f : .62f;
            talkHint.alpha = inspectable ? 1f : Mathf.MoveTowards(talkHint.alpha,
                visible ? opacity : 0f, Time.unscaledDeltaTime / .2f);
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
