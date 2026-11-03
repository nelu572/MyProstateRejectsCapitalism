using UnityEngine;

namespace MyProstateRejectsCapitalism.Interaction
{
    [DisallowMultipleComponent]
    public sealed class MessageInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Inspect test object";
        [SerializeField, TextArea] private string message = "Interaction successful!";

        public string InteractionPrompt => interactionPrompt;

        public void Interact(GameObject interactor)
        {
            Debug.Log(message, this);
        }
    }
}
