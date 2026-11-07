using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    public sealed class FinalToiletArrival : MonoBehaviour
    {
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private DialogueSequence response;

        private void OnTriggerStay(Collider other)
        {
            if (!enabled || !other.TryGetComponent<FirstPersonPlayer>(out var player) || !player.CanControl)
                return;

            if (dialogue.TryStart(response, player, null))
                enabled = false;
        }
    }
}
