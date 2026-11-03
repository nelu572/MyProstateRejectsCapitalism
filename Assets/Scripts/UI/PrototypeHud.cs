using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;

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

        private void Start()
        {
            SetObjective(initialObjective);
        }

        public void SetObjective(string objective)
        {
            objectiveLabel.text = "목표: " + objective;
        }

        private void LateUpdate()
        {
            crosshairLabel.enabled = player.CanControl;
            if (player.InputBlocked)
            {
                interactionLabel.text = string.Empty;
                return;
            }

            if (!player.CanControl)
            {
                interactionLabel.text = "화면 클릭: 조작 시작 / Esc: 커서 해제";
                return;
            }

            var target = interactor.CurrentTarget as Component;
            var usable = target != null && target.gameObject.activeInHierarchy
                && (!(target is Behaviour behaviour) || behaviour.isActiveAndEnabled);
            interactionLabel.text = usable ? "E - " + interactor.CurrentTarget.InteractionPrompt : string.Empty;
        }
    }
}
