using System;
using UnityEngine;
using UnityEngine.AI;

namespace Player.States {
  public class NavigateState : PlayerState {
    [NonSerialized] public Vector3 TargetPosition;
    private NavMeshPath _path;

    protected override void Awake() {
      base.Awake();
      _path = new NavMeshPath();
    }

    public override void OnEnter() {
      base.OnEnter();
      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      //Player.FootstepAudio.SetParameter("focus", 1);
      Player.ResetAgent();
      Player.Agent.destination = TargetPosition;
    }

    public override void OnUpdate() {
      Player.Agent.destination = TargetPosition;
      if (!Other.FollowState.IsActive) {
        LimitWalkingDistance();
      }
    }

    public void Enter(Vector3 targetPosition) {
      TargetPosition = targetPosition;
      Player.SwitchState(this);
    }

    private void LimitWalkingDistance() {
      Other.Agent.CalculatePath(Player.Agent.destination, _path);
      var corners = _path.corners;
      var fullLength = 0f;
      for (var i = 1; i < corners.Length; i++) {
        var length = Vector3.Distance(corners[i - 1], corners[i]);
        fullLength += length;
        if (fullLength > Player.Config.LimitDistance) {
          var difference = fullLength - Player.Config.LimitDistance;
          var position = Vector3.Lerp(
            corners[i],
            corners[i - 1],
            difference / length
          );
          Player.Agent.destination = position;
          break;
        }
      }
    }
  }
}
