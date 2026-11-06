using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    public sealed class ClosedRestroomDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private ClassroomToRestroomPrologue prologue;

        public string InteractionPrompt => "화장실 확인";

        public void Interact(GameObject interactor)
        {
            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (player != null && player.CanControl && prologue != null)
                prologue.TryShowRestroomObservation();
        }
    }
}
