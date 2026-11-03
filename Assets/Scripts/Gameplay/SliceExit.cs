using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    public sealed class SliceExit : MonoBehaviour, IInteractable
    {
        [SerializeField] private SliceActFlow flow;
        [SerializeField] private string prompt = "다음 공간으로 이동";
        [SerializeField] private string lockedPrompt;

        public string InteractionPrompt => flow.Ready ? prompt : lockedPrompt;

        public void Interact(GameObject interactor)
        {
            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (player != null)
                flow.TryLeave(player);
        }
    }
}
