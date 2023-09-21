using System;
using Interactions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Player.States {
  public class InteractState : PlayerState {
    [NonSerialized] public Conversation Conversation;

    public override void OnEnter() {
      base.OnEnter();
      Assert.IsNotNull(
        Conversation,
        "Interact state was entered without an interactable"
      );

      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      Conversation.OnFocusEnter(Player);
      Player.Agent.stoppingDistance = 0;
      Player.Agent.destination = Conversation.GetPosition(Player);
    }

    public override void OnExit() {
      base.OnExit();
      Conversation.OnFocusExit(Player);
      Conversation = null;
    }

    public override void OnUpdate() {
      var isReady = Conversation.IsReady(Player);
      var scale = isReady ? 0.5f : 1f;
      Player.Agent.speed = Player.Config.WalkSpeed * scale;
      Player.Agent.acceleration = Player.Config.Acceleration * scale;
      Player.Agent.destination = Conversation.GetPosition(Player);
      if (!isReady
        && Other.InteractState.IsActive
        && TryLimitWalkingDistance(out var position)) {
        Player.NavigateState.Enter(position);
      }

      base.OnUpdate();
      if (isReady) {
        TargetRotation = Quaternion.Lerp(
          Conversation.GetRotation(Player),
          TargetRotation,
          Player.Agent.remainingDistance
        );
      }
    }

    public void Enter(Conversation conversation) {
      Assert.AreEqual(IsActive, Conversation != null);

      if (Conversation == conversation) {
        return;
      }

      Player.SwitchState(null);
      Conversation = conversation;
      Player.SwitchState(this);
    }

    public bool IsInteractingWith(Conversation conversation) {
      return IsActive && Conversation == conversation;
    }

    public bool IsActivelyInteracting() {
      return IsActive && Conversation.IsInteracting;
    }
  }
}
