using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Gameplay;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    [DefaultExecutionOrder(100)]
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private TMP_Text objectiveLabel;
        [SerializeField] private TMP_Text interactionLabel;
        [SerializeField] private TMP_Text crosshairLabel;
        [SerializeField] private string initialObjective = "유료 화장실을 찾아 이용해 보자.";
        [SerializeField] private Collider advertisingGate;
        [SerializeField] private DialogueController advertisingDialogue;
        [SerializeField] private SliceActFlow advertisingFlow;
        [SerializeField] private GameObject[] signsAfterAdvertising = System.Array.Empty<GameObject>();
        private TMP_Text controlsLabel;
        private ProstateLimitGauge prostateLimitGauge;
        private GameObject cursorResumeHint;
        private readonly Plane[] advertisingViewPlanes = new Plane[6];
        private bool advertisingSignsRevealed;

        private void Start()
        {
            SetObjective(initialObjective);
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                if (label.name == "Controls")
                    controlsLabel = label;
            CreateCursorResumeHint();
        }

        public void SetObjective(string objective)
        {
            objectiveLabel.text = "목표: " + objective;
        }

        internal void SetProstateLimit(bool visible, float remaining, ProstateLimitGaugeLayout layout)
        {
            if (visible && (prostateLimitGauge == null || prostateLimitGauge.Layout != layout))
            {
                if (prostateLimitGauge != null)
                {
                    prostateLimitGauge.Root.SetActive(false);
                    Destroy(prostateLimitGauge.Root);
                }
                prostateLimitGauge = new ProstateLimitGauge(objectiveLabel.rectTransform.parent,
                    objectiveLabel.font, layout);
            }
            prostateLimitGauge?.Update(visible, remaining);
        }

        private void LateUpdate()
        {
            if (advertisingGate != null)
                UpdateAdvertisingGuidance();

            if (cursorResumeHint.activeSelf != player.NeedsCursorCapture)
                cursorResumeHint.SetActive(player.NeedsCursorCapture);

            crosshairLabel.enabled = player.CanControl;
            if (!player.CanControl)
            {
                interactionLabel.text = string.Empty;
                return;
            }

            var target = interactor.CurrentTarget as Component;
            var usable = target != null && target.gameObject.activeInHierarchy
                && (!(target is Behaviour behaviour) || behaviour.isActiveAndEnabled);
            interactionLabel.text = usable ? PlayerInteractor.InputPrompt + interactor.CurrentTarget.InteractionPrompt : string.Empty;
        }

        private void UpdateAdvertisingGuidance()
        {
            if (!advertisingSignsRevealed && advertisingFlow != null && advertisingFlow.Ready)
            {
                foreach (var sign in signsAfterAdvertising)
                    if (sign != null)
                        sign.SetActive(true);
                advertisingSignsRevealed = true;
            }

            var camera = player.ViewCamera;
            var gate = advertisingGate.bounds;
            bool close = gate.SqrDistance(camera.transform.position) <= 3.6f * 3.6f;
            GeometryUtility.CalculateFrustumPlanes(camera, advertisingViewPlanes);
            bool gateInView = GeometryUtility.TestPlanesAABB(advertisingViewPlanes, gate);
            bool hideInstructions = (close && gateInView)
                || (advertisingDialogue != null && advertisingDialogue.IsInteractive);
            objectiveLabel.enabled = !hideInstructions;
            if (controlsLabel != null)
                controlsLabel.enabled = !hideInstructions;
        }

        private void CreateCursorResumeHint()
        {
            cursorResumeHint = new GameObject("Cursor Resume Hint", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)cursorResumeHint.transform;
            rect.SetParent(interactionLabel.rectTransform.parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(340f, 36f);
            var background = cursorResumeHint.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, .78f);
            background.raycastTarget = false;

            var label = new GameObject("Label", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(rect, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.font = interactionLabel.font;
            label.fontSize = 20f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = "클릭하여 게임으로 돌아가기";
            cursorResumeHint.SetActive(false);
        }
    }
}
