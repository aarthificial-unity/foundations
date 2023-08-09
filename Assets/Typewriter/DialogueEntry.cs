using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

namespace Typewriter {
  public class DialogueEntry : LocalizedRuleEntry {
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference Speaker;
  }
}
