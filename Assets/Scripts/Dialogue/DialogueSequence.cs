using System;
using UnityEngine;

namespace MyProstateRejectsCapitalism.Dialogue
{
    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(menuName = "Dialogue/Sequence")]
    public sealed class DialogueSequence : ScriptableObject
    {
        [SerializeField] private DialogueLine[] lines;

        public DialogueLine[] Lines => lines;
    }
}
