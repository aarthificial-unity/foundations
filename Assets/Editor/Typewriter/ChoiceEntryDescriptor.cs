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
    [CustomEntryDescriptor(typeof(ChoiceEntry))]
    public class ChoiceEntryDescriptor : RuleEntryDescriptor
    {
        public override string Name => "Choice";
    }
}
