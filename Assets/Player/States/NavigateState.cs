﻿using System;
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
      if (!Other.FollowState.IsActive
        && TryLimitWalkingDistance(TargetPosition, out var position)) {
        Player.Agent.destination = position;
      } else {
        Player.Agent.destination = TargetPosition;
      }

      base.OnUpdate();
    }

    public void Enter(Vector3 targetPosition) {
      TargetPosition = targetPosition;
      Player.SwitchState(this);
    }
  }
}
