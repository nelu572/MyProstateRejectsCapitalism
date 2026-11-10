using System;
using System.Collections.Generic;
using System.Text;
using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.UI;
using TMPro;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Dialogue
{
    public sealed class DialogueController : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text dialogueLabel;
        [SerializeField, Min(1f)] private float charactersPerSecond = 55f;
        [SerializeField] private bool centerVisibleTyping;
        [SerializeField] private bool wordWrappedSubtitles;
        [SerializeField] private TMP_Text compactAdvanceHint;

        private struct TypingLine
        {
            public float min, max, offset;
            public bool initialized;
        }

        private TypingLine[] typingLines;
        private bool centeringNeedsUpdate;
        private int centeringFrame = -1;

        private DialogueSequence sequence;
        private FirstPersonPlayer player;
        private Action onCompleted;
        private int lastAdvanceFrame;
        private float visibleCharacters;
        private int characterCount;
        private float holdUntil;

        public event Action<int> LineShown;
        public event Action DialogueFinished;
        public event Action DialogueInput;
        public bool IsTyping => IsPlaying && dialogueLabel.maxVisibleCharacters < characterCount;
        public bool CanAdvance => IsPlaying && !IsTyping && Time.time >= holdUntil;
        public bool LastDialogueCompleted { get; private set; }
        public float SubtitleSpeed => MenuSettings.SubtitleSpeed(charactersPerSecond);

        // Retained for the existing player-feel test's explicit input-lock exercise.
        public void HoldCurrentLine(float seconds)
        {
            holdUntil = Mathf.Max(holdUntil, Time.time + seconds);
        }

        public bool IsPlaying => sequence != null;
        internal bool IsAutomatic { get; private set; }
        internal bool IsInteractive => IsPlaying && !IsAutomatic;
        public bool AllowsMovement { get; private set; }
        public int CurrentLineIndex { get; private set; }

        private void Awake()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (compactAdvanceHint != null)
                compactAdvanceHint.alpha = .9f;
        }

        private void OnEnable()
        {
            if (dialogueLabel != null)
                dialogueLabel.OnPreRenderText += CenterVisibleText;
        }

        private void LateUpdate()
        {
            if (centerVisibleTyping && IsPlaying && centeringNeedsUpdate)
                dialogueLabel.SetVerticesDirty();
        }

        private void CenterVisibleText(TMP_TextInfo info)
        {
            centeringNeedsUpdate = false;
            if (!centerVisibleTyping || !IsPlaying || (wordWrappedSubtitles && info.lineCount > 1)
                || dialogueLabel.maxVisibleCharacters == int.MaxValue
                || (dialogueLabel.maxVisibleCharacters >= info.characterCount && typingLines == null))
                return;

            if (typingLines == null || typingLines.Length != info.lineCount)
                typingLines = new TypingLine[info.lineCount];
            for (int i = 0; i < typingLines.Length; i++)
            {
                typingLines[i].min = float.PositiveInfinity;
                typingLines[i].max = float.NegativeInfinity;
            }
            // TMP retains full-sentence metrics even when hidden glyph vertices are zeroed.
            // Changing only mesh positions preserves wrapping, baselines and the other labels.
            for (int i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible)
                    continue;
                int line = character.lineNumber;
                typingLines[line].min = Mathf.Min(typingLines[line].min, character.bottomLeft.x);
                typingLines[line].max = Mathf.Max(typingLines[line].max, character.topRight.x);
            }
            float blend = centeringFrame == Time.frameCount ? 0f
                : 1f - Mathf.Exp(-Time.unscaledDeltaTime / .016f);
            centeringFrame = Time.frameCount;
            for (int i = 0; i < typingLines.Length; i++)
            {
                ref var line = ref typingLines[i];
                if (float.IsInfinity(line.min))
                {
                    line.offset = 0f;
                    line.initialized = false;
                    continue;
                }
                var layout = info.lineInfo[i];
                bool complete = dialogueLabel.maxVisibleCharacters > layout.lastVisibleCharacterIndex;
                float target = complete ? 0f
                    : (layout.lineExtents.min.x + layout.lineExtents.max.x - line.min - line.max) * .5f;
                // Center the first glyph immediately, then briefly ease toward each new center.
                line.offset = !line.initialized ? target : Mathf.Lerp(line.offset, target, blend);
                if (Mathf.Abs(line.offset - target) <= .05f)
                    line.offset = target;
                line.initialized = true;
                centeringNeedsUpdate |= Mathf.Abs(line.offset - target) > .05f;
            }
            if (!centeringNeedsUpdate && dialogueLabel.maxVisibleCharacters >= info.characterCount)
            {
                typingLines = null;
                return;
            }

            // Include underline, highlight and fallback-font quads, not only character quads.
            for (int m = 0; m < info.materialCount; m++)
            {
                var mesh = info.meshInfo[m];
                var vertices = mesh.vertices;
                for (int v = 0; v + 3 < mesh.vertexCount; v += 4)
                {
                    if (vertices[v] == Vector3.zero && vertices[v + 1] == Vector3.zero
                        && vertices[v + 2] == Vector3.zero && vertices[v + 3] == Vector3.zero)
                        continue;
                    float y = (vertices[v].y + vertices[v + 1].y + vertices[v + 2].y + vertices[v + 3].y) * .25f;
                    int nearest = -1;
                    float distance = float.PositiveInfinity, baselineDistance = float.PositiveInfinity;
                    for (int i = 0; i < info.lineCount; i++)
                    {
                        if (float.IsInfinity(typingLines[i].min))
                            continue;
                        var line = info.lineInfo[i];
                        float gap = Mathf.Max(Mathf.Max(line.descender - y, y - line.ascender), 0f);
                        float baseline = Mathf.Abs(y - line.baseline);
                        if (gap < distance || (gap == distance && baseline < baselineDistance))
                        {
                            nearest = i;
                            distance = gap;
                            baselineDistance = baseline;
                        }
                    }
                    if (nearest >= 0)
                        for (int j = 0; j < 4; j++)
                            vertices[v + j].x += typingLines[nearest].offset;
                }
            }
        }

        public bool TryStart(DialogueSequence dialogue, FirstPersonPlayer owner, Action completed,
            bool allowMovement = false)
            => StartSequence(dialogue, owner, completed, allowMovement, automatic: false);

        internal bool TryStartAutomatic(DialogueSequence dialogue, FirstPersonPlayer owner)
            => StartSequence(dialogue, owner, null, allowMovement: true, automatic: true);

        internal void StopAutomatic()
        {
            if (IsAutomatic)
                Finish(false);
        }

        private bool StartSequence(DialogueSequence dialogue, FirstPersonPlayer owner, Action completed,
            bool allowMovement, bool automatic)
        {
            if (!isActiveAndEnabled || IsInteractive || (automatic && IsPlaying) || owner == null || owner.InputBlocked
                || dialogue == null || dialogue.Lines == null || dialogue.Lines.Length == 0
                || dialoguePanel == null || speakerLabel == null || dialogueLabel == null)
                return false;

            StopAutomatic();
            sequence = dialogue;
            player = owner;
            onCompleted = completed;
            CurrentLineIndex = 0;
            lastAdvanceFrame = Time.frameCount;
            LastDialogueCompleted = false;
            IsAutomatic = automatic;
            AllowsMovement = allowMovement;
            if (!allowMovement)
                player.SetInputBlocked(true, allowCursorCapture: true);
            dialoguePanel.SetActive(true);
            ShowCurrentLine();
            return true;
        }

        private void Update()
        {
            if (!IsPlaying || player.IsPaused)
                return;

            if (!IsAutomatic)
                player.ReserveDialogueInputFrame();
            bool wasTyping = IsTyping;
            if (wasTyping)
            {
                visibleCharacters += Time.deltaTime * Mathf.Max(1f, SubtitleSpeed);
                dialogueLabel.maxVisibleCharacters = Mathf.Min(characterCount, Mathf.FloorToInt(visibleCharacters));
            }

            if (compactAdvanceHint != null)
                compactAdvanceHint.text = !IsAutomatic && CanAdvance ? "▾" : string.Empty;

            if (IsAutomatic)
            {
                if (!IsTyping && holdUntil == 0f)
                    HoldCurrentLine(Mathf.Clamp(characterCount * .065f, 1.45f, 2.8f));
                if (CanAdvance)
                    AdvanceLine();
                return;
            }

            // The click that recaptures the cursor belongs to the player, regardless of Update order.
            if (Time.frameCount <= lastAdvanceFrame || !player.AcceptsDialogueInput)
                return;

            if (!player.InteractionClickPressed)
                return;
            DialogueInput?.Invoke();
            if (!wasTyping && !CanAdvance)
                return;

            lastAdvanceFrame = Time.frameCount;
            if (wasTyping)
            {
                RevealCurrentLine();
                return;
            }

            AdvanceLine();
        }

        private void AdvanceLine()
        {
            CurrentLineIndex++;
            if (CurrentLineIndex >= sequence.Lines.Length)
                Finish(true);
            else
                ShowCurrentLine();
        }

        private void RevealCurrentLine()
        {
            typingLines = null;
            centeringNeedsUpdate = false;
            visibleCharacters = characterCount;
            dialogueLabel.maxVisibleCharacters = characterCount;
        }

        private void ShowCurrentLine()
        {
            typingLines = null;
            centeringNeedsUpdate = false;
            centeringFrame = -1;
            var line = sequence.Lines[CurrentLineIndex];
            speakerLabel.text = line.speaker;
            dialogueLabel.text = line.text;
            // TMP lays out the complete sentence once; visibility never changes wrapping or rich text.
            dialogueLabel.maxVisibleCharacters = int.MaxValue;
            var overflow = dialogueLabel.overflowMode;
            if (wordWrappedSubtitles)
            {
                // The previous line's compact height must not truncate the new layout measurement.
                dialogueLabel.overflowMode = TextOverflowModes.Overflow;
                dialogueLabel.text = WrapSubtitle(line.text);
            }
            dialogueLabel.ForceMeshUpdate();
            characterCount = dialogueLabel.textInfo.characterCount;
            visibleCharacters = 0f;
            holdUntil = 0f;
            dialogueLabel.maxVisibleCharacters = 0;
            if (compactAdvanceHint != null)
            {
                compactAdvanceHint.text = string.Empty;
                FitCompactPanel(dialogueLabel, speakerLabel, (RectTransform)dialoguePanel.transform);
                ApplyPolishedColors(speakerLabel, dialogueLabel, line.speaker == "???" || line.speaker == "전립선");
            }
            LineShown?.Invoke(CurrentLineIndex);
            if (wordWrappedSubtitles)
                dialogueLabel.overflowMode = overflow;
        }

        internal static void FitCompactPanel(TMP_Text line, TMP_Text speaker, RectTransform panel)
        {
            float height = Mathf.Ceil(line.preferredHeight) + 2f;
            line.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            speaker.rectTransform.anchoredPosition = new Vector2(0f, 44f + height + 10f);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 44f + height + 10f + 24f);
        }

        internal static void ApplyPolishedColors(TMP_Text speaker, TMP_Text line, bool companion)
        {
            speaker.color = companion ? new Color(.82f, .88f, .89f) : new Color(.86f, .85f, .81f);
            line.color = companion ? new Color(.88f, .94f, .94f) : new Color(.96f, .95f, .91f);
        }

        private struct SubtitleWord
        {
            public int first, last;
            public SubtitleWord(int first, int last) { this.first = first; this.last = last; }
        }

        private string WrapSubtitle(string source)
        {
            // Measure TMP's parsed glyphs, including rich-text styles, before revealing any text.
            // Source indices let us change separators without stripping or reconstructing tags.
            dialogueLabel.textWrappingMode = TextWrappingModes.NoWrap;
            dialogueLabel.ForceMeshUpdate();
            var info = dialogueLabel.textInfo;
            float width = Mathf.Max(1f, dialogueLabel.rectTransform.rect.width
                - dialogueLabel.margin.x - dialogueLabel.margin.z - 8f);
            var replacements = new Dictionary<int, string>();
            int paragraphStart = 0;
            for (int i = 0; i <= info.characterCount; i++)
            {
                if (i < info.characterCount && info.characterInfo[i].character != '\n'
                    && info.characterInfo[i].character != '\r')
                    continue;
                WrapParagraph(source, info, paragraphStart, i, width, replacements);
                paragraphStart = i + 1;
            }
            var result = new StringBuilder(source.Length + replacements.Count);
            for (int i = 0; i < source.Length; i++)
                result.Append(replacements.TryGetValue(i, out var value) ? value : source[i].ToString());
            return result.ToString();
        }

        private static void WrapParagraph(string source, TMP_TextInfo info, int start, int end, float width,
            Dictionary<int, string> replacements)
        {
            var chars = info.characterInfo;
            var words = new List<SubtitleWord>();
            for (int i = start; i < end;)
            {
                if (char.IsWhiteSpace(chars[i].character)) { i++; continue; }
                int first = i++;
                while (i < end && !char.IsWhiteSpace(chars[i].character)) i++;
                // Only an individual word wider than the entire subtitle can split at glyphs.
                int chunk = first;
                for (int j = first + 1; j < i; j++)
                    if (chars[j].xAdvance - chars[chunk].origin > width)
                    {
                        words.Add(new SubtitleWord(chunk, j - 1));
                        chunk = j;
                    }
                words.Add(new SubtitleWord(chunk, i - 1));
            }
            int count = words.Count;
            var lines = new int[count + 1];
            var cost = new float[count + 1];
            var next = new int[count];
            for (int i = count - 1; i >= 0; i--)
            {
                lines[i] = int.MaxValue;
                cost[i] = float.PositiveInfinity;
                for (int j = i; j < count; j++)
                {
                    float used = chars[words[j].last].xAdvance - chars[words[i].first].origin;
                    if (used > width && j > i) break;
                    float slack = Mathf.Max(0f, width - used) / width;
                    char tail = chars[words[j].last].character;
                    // Prefer fewer, reasonably balanced lines, with a small punctuation bonus.
                    float candidate = cost[j + 1] + slack * slack
                        - (j + 1 < count && ".,!?;:…".IndexOf(tail) >= 0 ? .035f : 0f);
                    int lineCount = 1 + lines[j + 1];
                    if (lineCount < lines[i] || (lineCount == lines[i] && candidate < cost[i]))
                    {
                        lines[i] = lineCount;
                        cost[i] = candidate;
                        next[i] = j + 1;
                    }
                }
            }
            for (int i = 0; i < count && next[i] < count; i = next[i])
            {
                int left = words[next[i] - 1].last, right = words[next[i]].first;
                bool separator = false;
                for (int j = left + 1; j < right; j++)
                    if (char.IsWhiteSpace(chars[j].character))
                    {
                        replacements[chars[j].index] = separator ? string.Empty : "\n";
                        separator = true;
                    }
                if (!separator)
                    replacements[chars[right].index] = "\n" + source[chars[right].index];
            }
        }

        private void Finish(bool completed)
        {
            var callback = onCompleted;
            onCompleted = null;
            sequence = null;
            visibleCharacters = 0f;
            characterCount = 0;
            centeringNeedsUpdate = false;
            holdUntil = 0f;
            LastDialogueCompleted = completed;
            if (dialogueLabel != null)
            {
                dialogueLabel.maxVisibleCharacters = int.MaxValue;
                if (!completed)
                    dialogueLabel.text = string.Empty;
            }
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (player != null && !IsAutomatic)
                player.SetInputBlocked(false);
            player = null;
            AllowsMovement = false;
            IsAutomatic = false;
            DialogueFinished?.Invoke();
            if (completed)
                callback?.Invoke();
        }

        private void OnDisable()
        {
            if (IsPlaying)
                Finish(false);
            if (dialogueLabel != null)
                dialogueLabel.OnPreRenderText -= CenterVisibleText;
        }
    }
}
