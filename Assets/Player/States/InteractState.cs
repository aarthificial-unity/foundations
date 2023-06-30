using System;
using Interactions;
using UnityEngine.Assertions;

namespace Player.States {
  public class InteractState : PlayerState {
    [NonSerialized] public Interactable Interactable;

    public override void OnEnter() {
      base.OnEnter();
      Assert.IsNotNull(
        Interactable,
        "Interact state was entered without an interactable"
      );

      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      Interactable.OnFocusEnter(Player);
      Player.Agent.stoppingDistance = 0;
      Player.Agent.destination = Interactable.GetPosition(Player);
    }

    public override void OnExit() {
      base.OnExit();
      Interactable.OnFocusExit(Player);
      Interactable = null;
    }

    public override void OnUpdate() {
      var scale = Interactable.IsReady(Player) ? 0.5f : 1f;
      Player.Agent.speed = Player.Config.WalkSpeed * scale;
      Player.Agent.acceleration = Player.Config.Acceleration * scale;
      Player.Agent.destination = Interactable.GetPosition(Player);
    }

    public void Enter(Interactable interactable) {
      Assert.AreEqual(IsActive, Interactable != null);

      if (Interactable == interactable) {
        return;
      }

      Player.SwitchState(null);
      Interactable = interactable;
      Player.SwitchState(this);
    }

    public bool IsInteractingWith(Interactable interactable) {
      return IsActive && Interactable == interactable;
    }

    public bool IsActivelyInteracting() {
      return IsActive && Interactable.IsInteracting;
    }
  }
}
