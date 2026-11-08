using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // This validation scene reuses the classroom wake and subtitle UI, with unqueued route observations.
    [DefaultExecutionOrder(200)]
    public sealed class ClassroomToRestroomPrologue : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private ClassroomSlidingDoor door;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private CanvasGroup blackout;
        [SerializeField] private Vector3 seatedPosition = new Vector3(-6.1f, .03f, 14.65f);
        [SerializeField] private Vector3 standingPosition = new Vector3(-5.35f, .03f, 14.65f);
        [SerializeField] private float wakeSeconds = 6.2f;
        [SerializeField] private float standingEyeHeight = 1.54f;
        [SerializeField] private Transform restroomEntry;

        private Camera view;
        private CharacterController controller;
        private PlayerInteractor interactor;
        private float beganAt, freeSeconds, lineUntil, quietUntil;
        [SerializeField] private Transform restroomSign;
        [SerializeField] private ActOneFirstEncounter firstEncounter;
        [SerializeField] private DialogueController dialogue;
        private bool firstThought, observation, purpose, wandered, corridorThought;
        private float enteredAt, corridorTravel;
        private Vector3 previousPosition;
        public bool PurposeShown => purpose;
        public bool RoomObserved => observation;
        public bool RestroomDiscovered { get; private set; }
        public bool EntryReached { get; private set; }
        public float WakeElapsed { get; private set; }
        public float ActualWakeSeconds { get; private set; }
        public bool WakeComplete { get; private set; }
        public bool WasSkipped { get; private set; }
        public bool CorridorEntered { get; private set; }
        public bool CorridorRevealed => RestroomDiscovered;
        public float ObservationSeconds => freeSeconds;
        public int ThoughtsShown { get; private set; }
        public bool CorridorThoughtShown => corridorThought;

        private void Start()
        {
            view = player.ViewCamera;
            controller = player.GetComponent<CharacterController>();
            interactor = player.GetComponent<PlayerInteractor>();
            beganAt = Time.time;
            subtitle.text = string.Empty;
            blackout.alpha = 1f;
            blackout.blocksRaycasts = false;
            player.SetInputBlocked(true);
            player.enabled = false;
            controller.enabled = false;
            player.transform.SetPositionAndRotation(seatedPosition, Quaternion.Euler(0f, 180f, 0f));
            view.transform.localPosition = new Vector3(0f, .93f, .34f);
            view.transform.localRotation = Quaternion.Euler(68f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (player.IsPaused)
                return;

            if (!WakeComplete)
            {
                WakeElapsed = Time.time - beganAt;
                hint.text = WakeElapsed > .6f ? "Space — 건너뛰기" : string.Empty;
                if (WakeElapsed > .6f && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    WasSkipped = true;
                    ReturnControl();
                    return;
                }
                float head = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.4f, 3.6f, WakeElapsed));
                float stand = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(4.1f, wakeSeconds, WakeElapsed));
                player.transform.position = Vector3.Lerp(seatedPosition, standingPosition, stand);
                var sittingHead = Vector3.Lerp(new Vector3(0f, .93f, .34f), new Vector3(0f, 1.25f, .03f), head);
                view.transform.localPosition = Vector3.Lerp(sittingHead, new Vector3(0f, standingEyeHeight, 0f), stand);
                view.transform.localRotation = Quaternion.Euler(Mathf.Lerp(68f, 5f, head) * (1f - stand), 0f, 0f);
                blackout.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.55f, 1.65f, WakeElapsed));
                if (!firstThought && WakeElapsed >= 2.25f)
                {
                    firstThought = true;
                    Say("……어?", 1.5f);
                }
                ClearFinishedLine();
                if (WakeElapsed >= wakeSeconds)
                    ReturnControl();
                return;
            }

            ClearFinishedLine();
            // Payment takes over the subtitle and hint immediately; route cues never queue behind it.
            if (firstEncounter != null && firstEncounter.HasBegun)
            {
                if (firstEncounter.IsPaying || (dialogue != null && dialogue.IsPlaying))
                {
                    subtitle.text = string.Empty;
                    lineUntil = 0f;
                }
                hint.text = ClosedRestroomHint();
                return;
            }
            if (!CorridorEntered && (door.IsOpening || door.IsOpen))
            {
                observation = true;
                subtitle.text = string.Empty;
                lineUntil = 0f;
                quietUntil = Time.time + .65f;
            }
            hint.text = player.CanControl && ReferenceEquals(interactor.CurrentTarget, door) && !door.IsOpen && !door.IsOpening
                ? "E — " + door.InteractionPrompt : ClosedRestroomHint();
            if (!player.CanControl)
                return;
            freeSeconds += Time.deltaTime;
            var pos = player.transform.position;
            bool inCorridor = pos.x > -1.85f && pos.x < 1.85f;
            if (CorridorEntered && inCorridor)
                corridorTravel += Vector2.Distance(new Vector2(pos.x, pos.z),
                    new Vector2(previousPosition.x, previousPosition.z));
            previousPosition = pos;

            // Opening the door ends all room cues immediately. Nothing queues for the reveal.
            if (door.IsOpening || door.IsOpen)
            {
                observation = true;
                if (!CorridorEntered && pos.x > -1.72f)
                {
                    CorridorEntered = true;
                    enteredAt = Time.time;
                }
            }
            if (CorridorEntered)
            {
                if (!RestroomDiscovered && CanSeeRestroomSign()
                    && !(corridorThought && Time.time < lineUntil))
                {
                    RestroomDiscovered = true;
                    subtitle.text = string.Empty;
                    lineUntil = 0f;
                }
                float entryZ = restroomEntry != null ? restroomEntry.position.z : 33.7f;
                if (pos.x < -2.45f && Mathf.Abs(pos.z - entryZ) < 1.05f)
                {
                    EntryReached = RestroomDiscovered = true;
                    subtitle.text = string.Empty;
                    lineUntil = 0f;
                }
                if (!corridorThought && !RestroomDiscovered && inCorridor
                    && corridorTravel >= 3.8f && CanSpeak())
                {
                    corridorThought = true;
                    Say("아직도 좀 멍하네.", 1.6f);
                }
                if (!wandered && !RestroomDiscovered && inCorridor && Time.time - enteredAt > 40f
                    && corridorTravel > 16f && CanSpeak())
                {
                    wandered = true;
                    Say("여기쯤 아니었나.", 2.2f);
                }
                return;
            }
            if (door.IsOpening || door.IsOpen)
                return;

            var toDoor = new Vector3(-2.1f, pos.y, 9.9f) - pos;
            bool headingOut = Vector3.Distance(pos, standingPosition) > .45f
                && Vector3.Dot(player.transform.forward, toDoor.normalized) > .55f;
            bool nearDoor = toDoor.magnitude < 3.7f;
            // The purpose replaces an obsolete room cue, even on a quick route; never waits.
            if (!purpose && (headingOut || nearDoor))
            {
                purpose = observation = true;
                Say("화장실만 갔다 가자.", 2.2f);
                return;
            }
            bool lookingAround = Mathf.Abs(Mathf.DeltaAngle(180f, view.transform.eulerAngles.y)) > 25f;
            if (!observation && !purpose && freeSeconds > 2.2f && lookingAround && CanSpeak())
            {
                observation = true;
                Say("……다 갔네.", 2f);
            }
        }

        private bool CanSeeRestroomSign()
        {
            if (restroomSign == null || Vector3.Distance(view.transform.position, restroomSign.position) > 9f)
                return false;
            var screen = view.WorldToViewportPoint(restroomSign.position);
            if (screen.z <= 0f || screen.x < .05f || screen.x > .95f || screen.y < .05f || screen.y > .95f)
                return false;
            if (!Physics.Linecast(view.transform.position, restroomSign.position, out var hit,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return true;
            return hit.transform == restroomSign || hit.transform.IsChildOf(restroomSign);
        }

        private void ReturnControl()
        {
            player.transform.SetPositionAndRotation(standingPosition, Quaternion.Euler(0f, 180f, 0f));
            view.transform.localPosition = new Vector3(0f, standingEyeHeight, 0f);
            view.transform.localRotation = Quaternion.identity;
            blackout.alpha = 0f;
            controller.enabled = true;
            player.enabled = true;
            player.ResetMotion();
            player.SetInputBlocked(false);
            WakeComplete = true;
            ActualWakeSeconds = Time.time - beganAt;
            hint.text = string.Empty;
            if (WasSkipped)
            {
                subtitle.text = string.Empty;
                lineUntil = 0f;
            }
            previousPosition = player.transform.position;
            quietUntil = Time.time + .8f;
            // A skip omits the room observation but preserves the immediate purpose.
            if (WasSkipped)
            {
                purpose = observation = true;
                Say("화장실만 갔다 가자.", 2.2f);
            }
        }

        private bool CanSpeak() => Time.time >= lineUntil && Time.time >= quietUntil;

        private string ClosedRestroomHint()
        {
            if (!player.CanControl)
                return string.Empty;
            if (interactor.CurrentTarget is ClosedRestroomDoor closed)
                return "E — " + closed.InteractionPrompt;
            if (interactor.CurrentTarget is PrologueExhibitionDoor exhibition && !exhibition.IsOpen)
                return "E — " + exhibition.InteractionPrompt;
            if (interactor.CurrentTarget is LevelExperimentInteraction exhibit)
                return "E — " + exhibit.InteractionPrompt;
            if (interactor.CurrentTarget is SliceExit exit)
                return "E — " + exit.InteractionPrompt;
            return string.Empty;
        }

        public bool TryShowExhibitionObservation()
        {
            if (!WakeComplete || !player.CanControl || (dialogue != null && dialogue.IsPlaying)
                || (firstEncounter != null && firstEncounter.IsPaying))
                return false;
            Say("……여긴 뭐야?", 2.4f);
            return true;
        }

        public bool TryShowRestroomObservation()
        {
            if (!WakeComplete || !player.CanControl || Time.time < lineUntil
                || (dialogue != null && dialogue.IsPlaying)
                || (firstEncounter != null && firstEncounter.IsPaying))
                return false;

            Say("여자 화장실이네.", 1.8f);
            return true;
        }

        private void Say(string text, float seconds)
        {
            subtitle.text = text;
            lineUntil = Time.time + seconds;
            quietUntil = lineUntil + 4f;
            ThoughtsShown++;
        }

        private void ClearFinishedLine()
        {
            if (lineUntil > 0f && Time.time >= lineUntil)
            {
                subtitle.text = string.Empty;
                lineUntil = 0f;
            }
        }
    }
}
