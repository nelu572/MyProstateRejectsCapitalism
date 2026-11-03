using System;
using MyProstateRejectsCapitalism.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProstateRejectsCapitalism.Dialogue
{
    public sealed class DialogueController : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text dialogueLabel;

        private DialogueSequence sequence;
        private FirstPersonPlayer player;
        private Action onCompleted;
        private int lastAdvanceFrame;
        private float advanceAllowedAt;

        public event Action<int> LineShown;
        public event Action DialogueFinished;
        public bool CanAdvance => IsPlaying && Time.unscaledTime >= advanceAllowedAt;

        public void HoldCurrentLine(float seconds)
        {
            advanceAllowedAt = Mathf.Max(advanceAllowedAt, Time.unscaledTime + seconds);
        }

        public bool IsPlaying => sequence != null;
        public bool AllowsMovement { get; private set; }
        public int CurrentLineIndex { get; private set; }

        private void Awake()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        public bool TryStart(DialogueSequence dialogue, FirstPersonPlayer owner, Action completed,
            bool allowMovement = false)
        {
            if (!isActiveAndEnabled || IsPlaying || owner == null || owner.InputBlocked
                || dialogue == null || dialogue.Lines == null || dialogue.Lines.Length == 0
                || dialoguePanel == null || speakerLabel == null || dialogueLabel == null)
                return false;

            sequence = dialogue;
            player = owner;
            onCompleted = completed;
            CurrentLineIndex = 0;
            lastAdvanceFrame = Time.frameCount;
            advanceAllowedAt = Time.unscaledTime;
            AllowsMovement = allowMovement;
            if (!allowMovement)
                player.SetInputBlocked(true);
            dialoguePanel.SetActive(true);
            ShowCurrentLine();
            return true;
        }

        private void Update()
        {
            if (!CanAdvance || Time.frameCount <= lastAdvanceFrame)
                return;

            var pressedE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            var clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (!pressedE && !clicked)
                return;

            lastAdvanceFrame = Time.frameCount;
            CurrentLineIndex++;
            if (CurrentLineIndex >= sequence.Lines.Length)
                Finish(true);
            else
                ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            var line = sequence.Lines[CurrentLineIndex];
            speakerLabel.text = line.speaker;
            dialogueLabel.text = line.text;
            LineShown?.Invoke(CurrentLineIndex);
        }

        private void Finish(bool completed)
        {
            var callback = onCompleted;
            onCompleted = null;
            sequence = null;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (player != null)
                player.SetInputBlocked(false);
            player = null;
            AllowsMovement = false;
            DialogueFinished?.Invoke();
            if (completed)
                callback?.Invoke();
        }

        private void OnDisable()
        {
            if (IsPlaying)
                Finish(false);
        }
    }
}
