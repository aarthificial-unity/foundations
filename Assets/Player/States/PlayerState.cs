using UnityEngine;
using UnityEngine.AI;

namespace Player.States {
  public class PlayerState : BaseState {
    protected PlayerController Player;
    protected PlayerController Other => Player.Other;
    private NavMeshPath _path;
    public Quaternion TargetRotation;

    public override void OnUpdate() {
      var velocity = Player.Agent.desiredVelocity;
      var planeVelocity = new Vector3(velocity.x, 0, velocity.z);
      if (!Player.Agent.pathPending && planeVelocity.magnitude > 0.1f) {
        TargetRotation = Quaternion.LookRotation(planeVelocity);
      } else {
        TargetRotation = Player.transform.rotation;
      }
    }

    protected virtual void Awake() {
      Player = GetComponent<PlayerController>();
      _path = new NavMeshPath();
    }

    protected bool TryLimitWalkingDistance(
      Vector3 target,
      out Vector3 position
    ) {
      Other.Agent.CalculatePath(target, _path);
      var corners = _path.corners;
      var fullLength = 0f;
      for (var i = 1; i < corners.Length; i++) {
        var length = Vector3.Distance(corners[i - 1], corners[i]);
        fullLength += length;
        if (fullLength > Player.Config.LimitDistance) {
          var difference = fullLength - Player.Config.LimitDistance;
          position = Vector3.Lerp(
            corners[i],
            corners[i - 1],
            difference / length
          );
          return true;
        }
      }

      position = default;
      return false;
    }
  }
}
