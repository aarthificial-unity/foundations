﻿using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
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

    public bool IsLT => Speaker == Facts.LT;

    public PlayerType PlayerType => IsLT ? PlayerType.LT : PlayerType.RT;

    public override void Apply(ITypewriterContext context) {
      context.Set(Facts.CurrentSpeaker, Speaker.ID);
      base.Apply(context);
    }
  }
}
