using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(SliceActFlow))]
    internal sealed class CompanionAutoDialogue : MonoBehaviour
    {
        private enum Cue { AfterExhibit, SeeAdvertisingGate, UpperDetour }

        [SerializeField] private Cue cue;
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private DialogueSequence lines;
        [SerializeField] private Collider advertisingGate;
        [SerializeField] private Bounds detourWalkway;

        private static int playedCues;
        private SliceActFlow flow;
        private ExhibitionInspection inspection;
        private float quietAfter;
        private Vector2? exhibitDeparture;

        internal static void ResetRun() => playedCues = 0;

        private void Awake()
        {
            flow = GetComponent<SliceActFlow>();
            inspection = player.GetComponent<ExhibitionInspection>();
        }

        private void LateUpdate()
        {
            int flag = 1 << (int)cue;
            if ((playedCues & flag) != 0 || flow.IsTransitioning || flow.IsEndingVisible)
                return;
            if (dialogue.IsPlaying || (inspection != null && inspection.SuppressesPause))
            {
                quietAfter = Time.time + .65f;
                return;
            }
            if (!player.CanControl || Time.time < quietAfter || !ShouldSpeak())
                return;
            if (dialogue.TryStartAutomatic(lines, player))
                playedCues |= flag;
        }

        private bool ShouldSpeak()
        {
            switch (cue)
            {
                case Cue.AfterExhibit:
                    if (!flow.RequirementCompleted)
                        return false;
                    var position = player.transform.position;
                    var horizontal = new Vector2(position.x, position.z);
                    exhibitDeparture ??= horizontal;
                    return (horizontal - exhibitDeparture.Value).sqrMagnitude >= .65f * .65f;

                case Cue.SeeAdvertisingGate:
                    return !flow.RequirementCompleted && GateIsVisible();

                case Cue.UpperDetour:
                    return detourWalkway.Contains(player.transform.position)
                        && player.HorizontalVelocity.sqrMagnitude > .25f;

                default:
                    return false;
            }
        }

        private bool GateIsVisible()
        {
            var camera = player.ViewCamera;
            var target = advertisingGate.bounds.center;
            var origin = camera.transform.position;
            if ((target - origin).sqrMagnitude > 8.5f * 8.5f)
                return false;
            var viewport = camera.WorldToViewportPoint(target);
            if (viewport.z <= 0f || viewport.x < .1f || viewport.x > .9f
                || viewport.y < .1f || viewport.y > .9f)
                return false;
            return !Physics.Linecast(origin, target, out var hit, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore) || hit.collider == advertisingGate
                || hit.transform.IsChildOf(advertisingGate.transform);
        }
    }
}
