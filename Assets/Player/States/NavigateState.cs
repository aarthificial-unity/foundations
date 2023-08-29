using System;
using UnityEngine;
using UnityEngine.AI;

namespace Player.States {
  public class NavigateState : PlayerState {
    [NonSerialized] public Vector3 TargetPosition;

    public override void OnEnter() {
      base.OnEnter();
      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      Player.Agent.destination = TargetPosition;
    }

    public override void OnUpdate() {
      Player.Agent.destination = TargetPosition;
      if (!Other.FollowState.IsActive
        && TryLimitWalkingDistance(out var position)) {
        Player.Agent.destination = position;
      }
    }

    public void Enter(Vector3 targetPosition) {
      TargetPosition = targetPosition;
      Player.SwitchState(this);
    }
  }
}
