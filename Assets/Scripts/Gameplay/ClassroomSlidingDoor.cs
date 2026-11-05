using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Only the classroom-wake candidate uses this single, one-way sliding door.
    public sealed class ClassroomSlidingDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform movingLeaves;
        [SerializeField] private Vector3 slideOffset = new Vector3(-.16f, 0f, 1.42f);
        [SerializeField] private float slideSeconds = .9f;
        private Vector3 closedPosition;
        public float OpenFraction { get; private set; }
        public bool IsOpening { get; private set; }
        public bool IsOpen => OpenFraction >= 1f;
        public float InteractionTime { get; private set; } = -1f;
        public float OpenedTime { get; private set; } = -1f;
        public int InteractionCount { get; private set; }
        public string InteractionPrompt => IsOpen ? string.Empty : IsOpening ? "열리는 중" : "교실 문 열기";

        private void Awake()
        {
            closedPosition = movingLeaves.localPosition;
        }

        public void Interact(GameObject interactor)
        {
            var player = interactor.GetComponent<FirstPersonPlayer>();
            if (IsOpen || IsOpening || player == null || !player.CanControl)
                return;
            InteractionTime = Time.time;
            InteractionCount++;
            IsOpening = true;
        }

        private void Update()
        {
            if (!IsOpening)
                return;
            OpenFraction = Mathf.MoveTowards(OpenFraction, 1f, Time.deltaTime / Mathf.Max(.1f, slideSeconds));
            movingLeaves.localPosition = closedPosition + slideOffset * Mathf.SmoothStep(0f, 1f, OpenFraction);
            if (IsOpen)
            {
                IsOpening = false;
                OpenedTime = Time.time;
            }
        }
    }
}
