using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Interactions;
using Player;

namespace Typewriter {
  public class DialogueEntry : RuleEntry {
    public enum BubbleStyle {
      Speech,
      Thought,
      Action,
    }

    public string Content;
    public BubbleStyle Style;
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference Speaker;

    public bool IsLT => Speaker == InteractionContext.LT;

    public PlayerType PlayerType => IsLT ? PlayerType.LT : PlayerType.RT;
  }
}
