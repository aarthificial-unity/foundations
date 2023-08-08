using Aarthificial.Typewriter.Editor.Descriptors;
using Typewriter;

namespace Editor.Typewriter
{
    [CustomEntryDescriptor(typeof(DialogueEntry))]
    public class DialogueEntryDescriptor : LocalizedRuleEntryDescriptor
    {
        public override string Name => "Dialogue";
    }
}
