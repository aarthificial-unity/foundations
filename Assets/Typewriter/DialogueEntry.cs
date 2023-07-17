using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

namespace Typewriter {
  public class DialogueEntry : TextRuleEntry {
    [EntryFilter(Type = EntryType.Fact)] public EntryReference Speaker;
  }
}
