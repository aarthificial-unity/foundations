using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using UnityEngine;

namespace Typewriter {
  public class EntryReferences : ScriptableObject {
    [EntryFilter(Type = EntryType.Fact)] public EntryReference GlobalScope;

    [EntryFilter(Type = EntryType.Fact)]
    public EntryReference ConversationScope;

    [EntryFilter(Type = EntryType.Fact)] public EntryReference CurrentSpeaker;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference Initiator;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference Listener;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference IsRTPresent;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference IsLTPresent;
  }
}
