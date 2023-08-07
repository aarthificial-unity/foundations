using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

namespace Typewriter {
  public class DialogueEntry : LocalizedRuleEntry {
    [EntryFilter(Variant = EntryVariant.Event)]
    public EntryReference Speaker;
  }
}
