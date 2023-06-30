using System.Collections.Generic;
using System.Linq;
using Aarthificial.Typewriter.Common;
using Aarthificial.Typewriter.Editor.Common;
using Aarthificial.Typewriter.Editor.Descriptors;
using Aarthificial.Typewriter.Entries;
using Typewriter;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;

namespace Editor.Typewriter
{
    [CustomEntryDescriptor(typeof(DialogueEntry))]
    public class DialogueEntryDescriptor : TextRuleEntryDescriptor
    {
        public override string Name => "Dialogue";
    }
}