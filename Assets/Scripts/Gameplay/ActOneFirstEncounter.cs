using System;
using System.Collections;
using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Used only by the ACT 1 first-encounter review scene.
    [DefaultExecutionOrder(300)]
    public sealed class ActOneFirstEncounter : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private TMP_Text thought;
        [SerializeField] private TMP_Text thoughtDisplay;
        [SerializeField] private TMP_Text speaker;
        [SerializeField] private TMP_Text line;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private TMP_Text interaction;
        [SerializeField] private TMP_Text crosshair;
        [SerializeField] private CanvasGroup thoughtGroup;
        [SerializeField] private CanvasGroup dialogueGroup;
        [SerializeField] private Volume atmosphere;
        [SerializeField] private bool polishedPresentation;
        [SerializeField] private bool completeFirstMeeting;
        [SerializeField] private bool compactDialogueHints;
        [SerializeField] private TMP_Text firstDialogueHint;

        private readonly Color ownColor = new Color(.94f, .93f, .89f);
        private readonly Color unknownColor = new Color(.77f, .87f, .91f);
        private string lastThought;
        private float thoughtAppearedAt;
        private float lineAppearedAt;
        private bool payment;
        private bool began;
        private bool fadingDialogue;
        private float dialogueFinishedAt;
        private VolumeProfile runtimeProfile;
        private ColorAdjustments grading;
        private float originalExposure;
        private bool originalExposureOverride;
        private float pulseStartedAt = -10f;
        private bool paymentSettled;
        private float lineFadeSeconds = .12f;
        private Vignette vignette;
        private float originalVignette;
        private bool originalVignetteOverride;
        private bool firstDialogueStarted;
        private bool guidanceShown;
        private bool guidanceDismissed;
        private float guidanceStartedAt;
        private float guidanceUntil;

        public bool HasBegun => began;
        public bool IsPaying => payment;

        private void Awake()
        {
            thoughtGroup.alpha = 0f;
            thoughtDisplay.text = string.Empty;
            dialogueGroup.alpha = 0f;
            if (compactDialogueHints)
            {
                hint.text = string.Empty;
                if (firstDialogueHint != null)
                    firstDialogueHint.text = string.Empty;
            }
            if (atmosphere != null)
            {
                // Volume.profile instantiates a runtime copy; shared grading remains untouched.
                runtimeProfile = atmosphere.profile;
                if (runtimeProfile.TryGet(out grading))
                {
                    originalExposure = grading.postExposure.value;
                    originalExposureOverride = grading.postExposure.overrideState;
                }
                if (polishedPresentation && runtimeProfile.TryGet(out vignette))
                {
                    originalVignette = vignette.intensity.value;
                    originalVignetteOverride = vignette.intensity.overrideState;
                }
            }
        }

        private void OnEnable()
        {
            dialogue.LineShown += OnLineShown;
            dialogue.DialogueFinished += OnDialogueFinished;
            dialogue.DialogueInput += OnDialogueInput;
        }

        public bool TryBegin(FirstPersonPlayer owner, DialogueSequence sequence, Action completed)
        {
            if (began || !isActiveAndEnabled || owner != player || !owner.CanControl
                || dialogue.IsPlaying || sequence == null || sequence.Lines.Length < 4)
                return false;

            began = true;
            payment = true;
            thought.text = string.Empty;
            thoughtGroup.alpha = 0f;
            player.SetInputBlocked(true);
            player.SetDialogueLookAllowed(false);
            StartCoroutine(BeginAfterPayment(sequence, completed));
            return true;
        }

        private IEnumerator BeginAfterPayment(DialogueSequence sequence, Action completed)
        {
            if (polishedPresentation)
            {
                yield return new WaitForSecondsRealtime(.34f);
                paymentSettled = true;
                yield return new WaitForSecondsRealtime(.14f);
            }
            else
                yield return new WaitForSecondsRealtime(.45f);
            payment = false;
            player.SetInputBlocked(false);
            if (!dialogue.TryStart(sequence, player, completed))
                Debug.LogError("ACT 1 first encounter could not start its existing dialogue.", this);
        }

        private void OnLineShown(int index)
        {
            if (compactDialogueHints)
            {
                if (!firstDialogueStarted)
                {
                    firstDialogueStarted = true;
                    guidanceStartedAt = Time.unscaledTime;
                }
                hint.text = string.Empty;
                if (firstDialogueHint != null)
                    firstDialogueHint.text = string.Empty;
                // Size the complete block once, before typing; its lower edge stays anchored.
                float height = Mathf.Ceil(line.preferredHeight) + 2f;
                line.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                speaker.rectTransform.anchoredPosition = new Vector2(0f, 44f + height + 10f);
                ((RectTransform)dialogueGroup.transform).SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, 44f + height + 10f + 24f);
            }
            bool walkingConversation = dialogue.AllowsMovement;
            lineAppearedAt = Time.unscaledTime;
            lineFadeSeconds = polishedPresentation ? (index == 0 ? .09f : .18f) : .12f;
            dialogueGroup.alpha = 0f;
            bool unknown = speaker.text == "???" || (completeFirstMeeting && speaker.text == "전립선");
            speaker.color = polishedPresentation
                ? (unknown ? new Color(.82f, .88f, .89f) : new Color(.86f, .85f, .81f))
                : (unknown ? unknownColor : new Color(.78f, .77f, .73f));
            line.color = polishedPresentation
                ? (unknown ? new Color(.88f, .94f, .94f) : new Color(.96f, .95f, .91f))
                : (unknown ? unknownColor : ownColor);
            // A brief searching beat: look is free, movement and interaction remain reserved.
            if (!walkingConversation)
                player.SetDialogueLookAllowed(index == 1 || (completeFirstMeeting && index >= 3));
            if (!walkingConversation && index == 0)
                pulseStartedAt = Time.unscaledTime;
        }

        private void OnDialogueFinished()
        {
            if (compactDialogueHints)
            {
                guidanceDismissed = true;
                hint.text = string.Empty;
                if (firstDialogueHint != null)
                    firstDialogueHint.text = string.Empty;
            }
            player.SetDialogueLookAllowed(false);
            fadingDialogue = true;
            dialogueFinishedAt = Time.unscaledTime;
            dialogueGroup.gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if (player.IsPaused)
                return;

            // Timed exploration thoughts use a separate label and never show an advance marker.
            if (compactDialogueHints && !dialogue.IsPlaying)
            {
                hint.text = string.Empty;
                if (firstDialogueHint != null)
                    firstDialogueHint.text = string.Empty;
            }
            if (began && (payment || dialogue.IsPlaying))
            {
                thought.text = string.Empty;
                thoughtDisplay.text = string.Empty;
                thoughtGroup.alpha = 0f;
            }
            else
            {
                if (lastThought != thought.text)
                {
                    lastThought = thought.text;
                    if (!string.IsNullOrEmpty(lastThought))
                    {
                        thoughtAppearedAt = Time.unscaledTime;
                        thoughtDisplay.text = lastThought;
                    }
                }
                float target = string.IsNullOrEmpty(thought.text) ? 0f
                    : Mathf.Clamp01((Time.unscaledTime - thoughtAppearedAt) / (polishedPresentation ? .22f : .18f));
                if (polishedPresentation)
                    target = Mathf.SmoothStep(0f, 1f, target);
                if (!string.IsNullOrEmpty(thought.text))
                    thoughtDisplay.text = thought.text;
                thoughtGroup.alpha = target > 0f ? target
                    : Mathf.MoveTowards(thoughtGroup.alpha, 0f, Time.unscaledDeltaTime / (polishedPresentation ? .22f : .16f));
                if (thoughtGroup.alpha == 0f && string.IsNullOrEmpty(thought.text))
                    thoughtDisplay.text = string.Empty;
            }

            if (dialogue.IsPlaying)
            {
                float alpha = Mathf.Clamp01((Time.unscaledTime - lineAppearedAt) / lineFadeSeconds);
                dialogueGroup.alpha = polishedPresentation ? Mathf.SmoothStep(0f, 1f, alpha) : alpha;
                if (!dialogue.IsTyping)
                    dialogueGroup.alpha = 1f;
                UpdateAdvanceHint();
            }
            else if (fadingDialogue)
            {
                hint.text = string.Empty;
                float alpha = Mathf.Clamp01((Time.unscaledTime - dialogueFinishedAt) / (polishedPresentation ? .22f : .14f));
                dialogueGroup.alpha = 1f - (polishedPresentation ? Mathf.SmoothStep(0f, 1f, alpha) : alpha);
                if (dialogueGroup.alpha <= 0f)
                {
                    fadingDialogue = false;
                    dialogueGroup.gameObject.SetActive(false);
                }
            }

            if (payment)
                interaction.text = polishedPresentation ? (paymentSettled ? string.Empty : "결제 요청…") : "결제 중…";
            else if (began && (dialogue.IsPlaying || player.InputBlocked))
                interaction.text = string.Empty;
            else if (interactor.CurrentTarget is PaidToiletEvent)
                interaction.text = "E   결제하기 · 500원";

            crosshair.text = "·";
            crosshair.color = new Color(1f, 1f, 1f, interactor.CurrentTarget != null ? .7f : .28f);

            if (grading != null)
            {
                float t = (Time.unscaledTime - pulseStartedAt) / .38f;
                // Less than a tenth of a stop; a single quiet interruption, without flicker.
                grading.postExposure.value = originalExposure
                    - (t >= 0f && t <= 1f ? .09f * Mathf.Sin(t * Mathf.PI) : 0f);
            }
            if (vignette != null)
            {
                float t = (Time.unscaledTime - pulseStartedAt) / .52f;
                bool pulsing = t >= 0f && t <= 1f;
                vignette.intensity.overrideState = pulsing || originalVignetteOverride;
                vignette.intensity.value = originalVignette + (pulsing ? .045f * Mathf.Sin(t * Mathf.PI) : 0f);
            }
        }

        private void UpdateAdvanceHint()
        {
            if (!compactDialogueHints)
            {
                hint.text = dialogue.IsTyping ? "E / 클릭   바로 표시" : "E / 클릭   계속";
                if (polishedPresentation)
                    hint.alpha = Mathf.MoveTowards(hint.alpha, .9f, Time.unscaledDeltaTime / .18f);
                return;
            }

            bool ready = dialogue.CanAdvance;
            if (ready && !guidanceDismissed && !guidanceShown
                && Time.unscaledTime - guidanceStartedAt >= 3f)
            {
                guidanceShown = true;
                guidanceUntil = Time.unscaledTime + 4f;
            }
            hint.text = ready ? "▾" : string.Empty;
            hint.alpha = ready ? .9f : 0f;
            if (firstDialogueHint != null)
            {
                string notice = ready && guidanceShown && !guidanceDismissed && Time.unscaledTime < guidanceUntil
                    ? "E / 클릭: 완성·진행" : string.Empty;
                firstDialogueHint.text = notice;
                firstDialogueHint.alpha = notice.Length > 0 ? .9f : 0f;
                hint.rectTransform.anchoredPosition = new Vector2(notice.Length > 0
                    ? firstDialogueHint.preferredWidth * .5f + 20f : 0f, 8f);
            }
        }

        private void OnDialogueInput()
        {
            guidanceDismissed = true;
            if (compactDialogueHints && firstDialogueHint != null)
                firstDialogueHint.text = string.Empty;
        }

        private void OnDisable()
        {
            if (compactDialogueHints)
            {
                hint.text = string.Empty;
                if (firstDialogueHint != null)
                    firstDialogueHint.text = string.Empty;
            }
            dialogue.LineShown -= OnLineShown;
            dialogue.DialogueFinished -= OnDialogueFinished;
            dialogue.DialogueInput -= OnDialogueInput;
            StopAllCoroutines();
            if (payment)
                player.SetInputBlocked(false);
            player.SetDialogueLookAllowed(false);
            if (grading != null)
            {
                grading.postExposure.value = originalExposure;
                grading.postExposure.overrideState = originalExposureOverride;
            }
            if (vignette != null)
            {
                vignette.intensity.value = originalVignette;
                vignette.intensity.overrideState = originalVignetteOverride;
            }
        }

        private void OnDestroy()
        {
            if (runtimeProfile == null)
                return;
            foreach (var component in runtimeProfile.components)
                Destroy(component);
            Destroy(runtimeProfile);
        }
    }
}
