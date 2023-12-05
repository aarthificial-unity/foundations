using System;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter;
using Typewriter;

namespace Interactions {
  [Serializable]
  public class InteractionContext : ITypewriterContext {
    public Blackboard Context;
    [NonSerialized] public IBlackboard Interaction;
    [NonSerialized] public IBlackboard Global = EmptyBlackboard.Instance;
    public Interactable Interactable;

    public bool TryGetBlackboard(int scope, out IBlackboard blackboard) {
      switch (scope) {
        case Facts.InteractionScope:
          blackboard = Interaction;
          return true;
        case Facts.ContextScope:
          blackboard = Context;
          return true;
        case Facts.GlobalScope:
          blackboard = Global;
          return true;
        default:
          blackboard = default;
          return false;
      }
    }

    public void Setup(Interactable interactable) {
      Interactable = interactable;
      Context.Set(Facts.CurrentSpeaker, 0);
      Context.Set(Facts.LT, Facts.LT);
      Context.Set(Facts.RT, Facts.RT);
    }

    public void SetSpeaker(int speaker) {
      Context.Set(Facts.CurrentSpeaker, speaker);
    }
  }
}
