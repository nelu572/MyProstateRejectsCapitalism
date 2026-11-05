using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // A short, scene-specific wake-up and unqueued observations; no shared cutscene system.
    [DefaultExecutionOrder(200)]
    public sealed class ClassroomWakePrologue : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private ClassroomSlidingDoor door;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private CanvasGroup blackout;
        [SerializeField] private Vector3 seatedPosition = new Vector3(-6.1f, .03f, 14.65f);
        [SerializeField] private Vector3 standingPosition = new Vector3(-5.35f, .03f, 14.65f);
        [SerializeField] private float wakeSeconds = 6.2f;

        private Camera view;
        private CharacterController controller;
        private PlayerInteractor interactor;
        private float beganAt, freeSeconds, lineUntil, quietUntil;
        private bool firstThought, observation, doorThought, reaction;
        public float WakeElapsed { get; private set; }
        public float ActualWakeSeconds { get; private set; }
        public bool WakeComplete { get; private set; }
        public bool WasSkipped { get; private set; }
        public bool CorridorEntered { get; private set; }
        public bool CorridorRevealed => reaction;
        public float ObservationSeconds => freeSeconds;
        public int ThoughtsShown { get; private set; }

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
                view.transform.localPosition = Vector3.Lerp(sittingHead, new Vector3(0f, 1.6f, 0f), stand);
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
            hint.text = player.CanControl && interactor.CurrentTarget == door && !door.IsOpen
                ? "E — " + door.InteractionPrompt : string.Empty;
            if (!player.CanControl)
                return;
            freeSeconds += Time.deltaTime;
            var pos = player.transform.position;
            bool nearDoor = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(-2.1f, 9.9f)) < 3.2f;
            bool lookingOrWalking = Mathf.Abs(Mathf.DeltaAngle(180f, view.transform.eulerAngles.y)) > 20f
                || Vector3.Distance(pos, standingPosition) > .6f;

            if (!CorridorEntered && door.IsOpen && pos.x > -1.72f)
            {
                CorridorEntered = true;
                observation = doorThought = true; // Drop stale room cues, never queue them.
                subtitle.text = string.Empty;
                lineUntil = 0f;
                quietUntil = Time.time + 1.5f;
            }
            if (CorridorEntered)
            {
                var forward = view.transform.forward;
                bool longView = Mathf.Abs(forward.z) > .82f && Mathf.Abs(forward.y) < .45f
                    && !Physics.Raycast(view.transform.position, forward, 25f, Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore);
                if (!reaction && longView && CanSpeak())
                {
                    reaction = true;
                    Say("……복도가 원래 이렇게 길었나?", 3.5f);
                }
                return;
            }
            if (!observation && ((freeSeconds >= 4.5f && lookingOrWalking) || nearDoor) && CanSpeak())
            {
                observation = true;
                Say("뭐야, 언제 이렇게 늦었지…….", 3f);
            }
            else if (observation && !doorThought && nearDoor && freeSeconds >= 14f
                && !door.IsOpening && !door.IsOpen && CanSpeak())
            {
                doorThought = true;
                Say("다들 갔나……?", 2.2f);
            }
            if (door.IsOpening || door.IsOpen)
                doorThought = true;
        }

        private void ReturnControl()
        {
            player.transform.SetPositionAndRotation(standingPosition, Quaternion.Euler(0f, 180f, 0f));
            view.transform.localPosition = new Vector3(0f, 1.6f, 0f);
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
            // Keep the immediate physical need even if the player skips or leaves quickly.
            Say("아…… 화장실.", 2f);
        }

        private bool CanSpeak() => Time.time >= lineUntil && Time.time >= quietUntil;

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
