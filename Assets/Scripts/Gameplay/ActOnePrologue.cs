using System;
using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using TMPro;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    [DefaultExecutionOrder(200)]
    public sealed class ActOnePrologue : MonoBehaviour
    {
        [Serializable]
        private struct Thought
        {
            [TextArea] public string text;
            [Min(0.1f)] public float seconds;
        }

        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private PaidToiletEvent paidToilet;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private BoxCollider searchArea;
        [SerializeField] private Transform toiletViewTarget;
        [SerializeField, Min(0.1f)] private float inspectionDistance = 8f;
        [SerializeField] private Thought[] thoughts;

        private int highestCue = -1;
        private int nextThought;
        private float remaining;
        private float pause = 0.3f;
        private bool showing;
        private bool encounterStarted;

        private void Awake()
        {
            subtitle.text = string.Empty;
        }

        private void Update()
        {
            if (encounterStarted || !player.CanControl)
                return;

            highestCue = Mathf.Max(highestCue, 0);
            if (searchArea.bounds.Contains(player.transform.position + Vector3.up))
                highestCue = Mathf.Max(highestCue, 1);

            if (CanSeeToilet())
            {
                highestCue = Mathf.Max(highestCue, 2);
                var delta = toiletViewTarget.position - player.ViewCamera.transform.position;
                if (delta.sqrMagnitude <= inspectionDistance * inspectionDistance)
                    highestCue = Mathf.Max(highestCue, 3);
            }

            if (showing)
            {
                remaining -= Time.unscaledDeltaTime;
                if (remaining <= 0f)
                {
                    subtitle.text = string.Empty;
                    showing = false;
                    pause = 0.2f;
                }
                return;
            }

            pause -= Time.unscaledDeltaTime;
            if (pause > 0f || nextThought > highestCue || nextThought >= thoughts.Length)
                return;

            var thought = thoughts[nextThought++];
            subtitle.text = thought.text;
            remaining = thought.seconds;
            showing = true;
        }

        private bool CanSeeToilet()
        {
            var camera = player.ViewCamera;
            var screen = camera.WorldToViewportPoint(toiletViewTarget.position);
            if (screen.z <= 0f || screen.x < 0.1f || screen.x > 0.9f
                || screen.y < 0.1f || screen.y > 0.9f)
                return false;

            return !Physics.Linecast(camera.transform.position, toiletViewTarget.position,
                       out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                   || hit.collider.transform.IsChildOf(paidToilet.transform);
        }

        private void LateUpdate()
        {
            if (encounterStarted || (!paidToilet.HasBeenUsed && !dialogue.IsPlaying))
                return;

            encounterStarted = true;
            subtitle.text = string.Empty;
            subtitle.gameObject.SetActive(false);
        }
    }
}
