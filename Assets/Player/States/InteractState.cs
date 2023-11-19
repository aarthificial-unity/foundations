using Interactions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Player.States {
  public class InteractState : PlayerState {
    private Interactable _interactable;

    public override void OnEnter() {
      base.OnEnter();
      Assert.IsNotNull(
        _interactable,
        "Interact state was entered without an interactable"
      );

      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      _interactable.OnFocusEnter(Player);
      Player.Agent.stoppingDistance = 0;
      Player.Agent.destination = _interactable.GetPosition(Player);
    }

    public override void OnExit() {
      base.OnExit();
      _interactable.OnFocusExit(Player);
      _interactable = null;
    }

    public override void OnUpdate() {
      var isReady = _interactable.IsReady(Player);
      var scale = isReady ? 0.5f : 1f;
      Player.Agent.speed = Player.Config.WalkSpeed * scale;
      Player.Agent.acceleration = Player.Config.Acceleration * scale;
      Player.Agent.destination = _interactable.GetPosition(Player);
      if (!isReady
        && Other.InteractState.IsActive
        && Other.InteractState._interactable != _interactable
        && TryLimitWalkingDistance(
          Player.Agent.destination,
          out var position
        )) {
        Player.NavigateState.Enter(position);
      }

      base.OnUpdate();
      if (isReady) {
        TargetRotation = Quaternion.Lerp(
          _interactable.GetRotation(Player),
          TargetRotation,
          Player.Agent.remainingDistance
        );
      }
    }

    public void Enter(Interactable interactable) {
      Assert.AreEqual(IsActive, _interactable != null);

      if (_interactable == interactable) {
        return;
      }

      Player.SwitchState(null);
      _interactable = interactable;
      Player.SwitchState(this);
    }
  }
}
