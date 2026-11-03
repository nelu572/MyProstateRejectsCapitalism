using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using MyProstateRejectsCapitalism.UI;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Temporary responses for the two independent level experiments.
    public sealed class LevelExperimentInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt;
        [SerializeField] private DialogueSequence response;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private PrototypeHud hud;
        [SerializeField] private string nextObjective;

        public bool HasBeenUsed { get; private set; }
        public string InteractionPrompt => prompt;

        public void Interact(GameObject interactor)
        {
            if (HasBeenUsed || !isActiveAndEnabled || dialogueController == null || hud == null)
                return;

            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (player == null || !player.CanControl)
                return;

            if (dialogueController.TryStart(response, player, () => hud.SetObjective(nextObjective)))
            {
                HasBeenUsed = true;
                enabled = false;
            }
        }
    }
}
