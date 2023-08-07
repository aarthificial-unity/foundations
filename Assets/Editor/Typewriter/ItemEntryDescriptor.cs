using Aarthificial.Typewriter.Editor.Descriptors;
using Typewriter;

namespace Editor.Typewriter
{
    [CustomEntryDescriptor(typeof(ItemEntry))]
    public class ItemEntryDescriptor : FactEntryDescriptor
    {
        public override string Name => "Item";
    }
}
