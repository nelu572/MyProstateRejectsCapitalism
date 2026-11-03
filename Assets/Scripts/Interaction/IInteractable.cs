using UnityEngine;

namespace MyProstateRejectsCapitalism.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameObject interactor);
    }
}
