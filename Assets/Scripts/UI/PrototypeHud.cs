using MyProstateRejectsCapitalism.Characters;
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
        private GameObject cursorResumeHint;

        private void Start()
        {
            SetObjective(initialObjective);
            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                if (label.name == "Controls")
                    label.text = label.text.Replace("E 확인", "클릭 / E 확인");
            CreateCursorResumeHint();
        }

        public void SetObjective(string objective)
        {
            objectiveLabel.text = "목표: " + objective;
        }

        private void LateUpdate()
        {
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
