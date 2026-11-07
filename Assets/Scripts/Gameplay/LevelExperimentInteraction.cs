using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Interaction;
using MyProstateRejectsCapitalism.UI;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Shared observation interaction; a showroom can use one first response across its exhibits.
    public sealed class LevelExperimentInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt;
        [SerializeField] private DialogueSequence response;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private PrototypeHud hud;
        [SerializeField] private string nextObjective;
        [SerializeField] private SliceActFlow sharedProgress;
        [SerializeField] private DialogueSequence additionalResponse;

        public bool HasBeenUsed { get; private set; }
        public string InteractionPrompt => prompt;

        public void Interact(GameObject interactor)
        {
            if (HasBeenUsed || !isActiveAndEnabled || dialogueController == null || hud == null)
                return;

            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (player == null || !player.CanControl)
                return;

            var sequence = sharedProgress != null && sharedProgress.RequirementCompleted && additionalResponse != null
                ? additionalResponse : response;
            if (dialogueController.TryStart(sequence, player, () => hud.SetObjective(nextObjective)))
            {
                HasBeenUsed = true;
                enabled = false;
            }
        }
    }
}
