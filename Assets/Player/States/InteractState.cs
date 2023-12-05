using Interactions;
using Settings.Bundles;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Player.States {
  public class InteractState : PlayerState {
    [Inject] [SerializeField] private GameplaySettingsBundle _bundle;
    [NonSerialized] public Interactable Interactable;
    private bool _autoEnter;

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
      var isReady = Interactable.IsReady(Player);
      var scale = isReady ? 0.5f : 1f;
      Player.Agent.speed = Player.Config.WalkSpeed * scale;
      Player.Agent.acceleration = Player.Config.Acceleration * scale;
      Player.Agent.destination = Interactable.GetPosition(Player);
      if (!isReady
        && Other.InteractState.IsActive
        && Other.InteractState.Interactable != Interactable
        && TryLimitWalkingDistance(
          Player.Agent.destination,
          out var position
        )) {
        Player.NavigateState.Enter(position);
      }

      base.OnUpdate();
      if (isReady) {
        TargetRotation = Quaternion.Lerp(
          Interactable.GetRotation(Player),
          TargetRotation,
          Player.Agent.remainingDistance
        );

        if (_autoEnter) {
          _autoEnter = false;
          if (Player.IsCurrent) {
            Interactable.Interact(Player);
          }
        }
      }
    }

    public void Enter(Interactable interactable) {
      Assert.AreEqual(IsActive, Interactable != null);

      if (Interactable == interactable) {
        return;
      }

      _autoEnter = _bundle.AutoDialogue.GetBool();
      Player.SwitchState(null);
      Interactable = interactable;
      Player.SwitchState(this);
    }
  }
}
