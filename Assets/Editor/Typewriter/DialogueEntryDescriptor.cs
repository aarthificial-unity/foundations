using Aarthificial.Typewriter.Editor.Descriptors;
using Typewriter;

namespace Editor.Typewriter
{
    [CustomEntryDescriptor(typeof(DialogueEntry))]
    public class DialogueEntryDescriptor : RuleEntryDescriptor
    {
        public override string Name => "Dialogue";
    }
}
