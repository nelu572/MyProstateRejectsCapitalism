using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using MyProstateRejectsCapitalism.UI;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    public sealed class PaidToiletEvent : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueSequence refusalDialogue;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private PrototypeHud hud;
        [SerializeField] private string nextObjective = "다른 화장실을 찾아보자.";
        [SerializeField] private ActOneFirstEncounter firstEncounter;

        public bool HasBeenUsed { get; private set; }
        public string InteractionPrompt => "유료 화장실 이용 시도 (500원)";

        public void Interact(GameObject interactor)
        {
            if (HasBeenUsed || !isActiveAndEnabled || dialogueController == null || hud == null)
                return;

            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (player == null || !player.CanControl)
                return;

            if (firstEncounter != null)
            {
                if (firstEncounter.TryBegin(player, refusalDialogue, () => hud.SetObjective(nextObjective)))
                {
                    HasBeenUsed = true;
                    enabled = false;
                }
                return;
            }

            if (dialogueController.TryStart(refusalDialogue, player, () => hud.SetObjective(nextObjective)))
            {
                HasBeenUsed = true;
                enabled = false;
            }
        }
    }
}
