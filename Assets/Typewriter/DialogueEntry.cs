using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

namespace Typewriter {
  public class DialogueEntry : TextRuleEntry {
    public bool choice;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference Speaker;
  }
}
