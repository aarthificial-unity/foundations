using Aarthificial.Typewriter.Editor.Descriptors;
using Typewriter;

namespace Editor.Typewriter
{
    [CustomEntryDescriptor(typeof(ChoiceEntry))]
    public class ChoiceEntryDescriptor : RuleEntryDescriptor
    {
        public override string Name => "Choice";
    }
}
