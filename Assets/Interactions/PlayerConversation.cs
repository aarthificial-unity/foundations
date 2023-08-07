using Aarthificial.Typewriter;
using Player;

namespace Interactions {
  public class PlayerConversation : Interactable {
    public override void Interact(PlayerController player) {
      if (player.gameObject == gameObject) {
        if (player.InteractState.IsActive) {
          return;
        }

        Blackboard.Set(InteractionContext.InitialEvent, Event.EventReference);
        Blackboard.Set(InteractionContext.Initiator, player.Fact);
        Blackboard.Set(InteractionContext.Listener, 0);
        Blackboard.Set(
          InteractionContext.IsLTPresent,
          player.Type == PlayerType.LT ? 1 : 0
        );
        Blackboard.Set(
          InteractionContext.IsRTPresent,
          player.Type == PlayerType.RT ? 1 : 0
        );
        HasDialogue = Context.HasMatchingRule(Event.EventReference);
        if (!HasDialogue) {
          return;
        }

        player.SwitchState(player.IdleState);
        Context.Process(Event.EventReference);
      } else {
        if (player.Other.InteractState.IsActive) {
          return;
        }

        Blackboard.Set(InteractionContext.Initiator, player.Fact);
        Blackboard.Set(InteractionContext.Listener, player.Other.Fact);
        Blackboard.Set(InteractionContext.IsLTPresent, 1);
        Blackboard.Set(InteractionContext.IsRTPresent, 1);

        HasDialogue = Context.HasMatchingRule(Event.EventReference);
        if (!HasDialogue) {
          return;
        }

        player.Other.SwitchState(player.Other.IdleState);
        player.FollowState.Enter();
        Context.Process(Event.EventReference);
      }
    }
  }
}
