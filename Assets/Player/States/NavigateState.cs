using UnityEngine;
using UnityEngine.AI;

namespace Player.States {
  public class NavigateState : BaseState {
    private readonly NavMeshPath _path = new();

    public NavigateState(PlayerController playerController) : base(
      playerController
    ) { }

    public override void OnEnter() {
      base.OnEnter();
      if (Other.NavigateState.IsActive) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      Player.Agent.destination = Player.Manager.TargetPosition;
    }

    public override void OnUpdate() {
      if (Player.IsActivelyNavigating()) {
        Player.Agent.destination = Player.Manager.TargetPosition;
      }

      if (Other.InteractState.IsActive) {
        LimitWalkingDistance();
      }
    }

    public void Enter() {
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
