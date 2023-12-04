using UnityEngine;
using Utils;

namespace Player.States {
  public class FollowState : PlayerState {
    public bool TightDistance;

    private Vector3 _otherVelocity;

    public override void OnEnter() {
      base.OnEnter();
      Player.Agent.stoppingDistance = Player.Config.MinDistance;
    }

    public override void OnUpdate() {
      UpdateNavigation();
      base.OnUpdate();
    }

    private void UpdateNavigation() {
      var thisPosition = Player.transform.position;
      var otherPosition = Other.transform.position;
      var relativePosition = thisPosition - otherPosition;
      var distance = relativePosition.magnitude;
      var direction =
        distance != 0 ? relativePosition / distance : Vector3.forward;
      var isWalkingTowards = Vector3.Dot(
          Other.Agent.desiredVelocity.normalized,
          direction
        )
        > 0.5f;

      var minDistance = Player.Config.MinDistance;
      var maxDistance = Player.Config.MaxDistance;
      if (TightDistance) {
        minDistance = Player.Config.MinTightDistance;
        maxDistance = Player.Config.MaxTightDistance;
      }

      Player.Agent.autoBraking = false;
      Player.Agent.destination = otherPosition;
      Player.Agent.stoppingDistance = minDistance;

      if (isWalkingTowards) {
        if (distance < Player.Config.CloseDistance) {
          AvoidCollision(direction, distance);
          return;
        }

        var targetDirection =
          (thisPosition - Other.Agent.destination).normalized;
        if (Vector3.Dot(targetDirection, direction) < 0.5f) {
          Player.Agent.speed = 0;
          Player.Agent.acceleration = 8f;
          return;
        }

        Player.Agent.destination = Other.Agent.destination;
      }

      var range = Player.Agent.remainingDistance.ToClampedRange(
        minDistance,
        maxDistance
      );
      Player.Agent.speed = range.Map(1f, Player.Config.WalkSpeed);
      Player.Agent.acceleration = range.Map(8f, Player.Config.Acceleration);
    }

    public void Enter() {
      Player.SwitchState(this);
    }

    private void AvoidCollision(Vector3 direction, float distance) {
      var otherVelocity = Other.Agent.desiredVelocity.normalized;
      if (otherVelocity != Vector3.zero) {
        _otherVelocity = otherVelocity;
      }

      var perpendicular = Quaternion.AngleAxis(90, Vector3.up) * _otherVelocity;
      var dot = Vector3.Dot(perpendicular, direction);
      var escapeDirection = Vector3.Lerp(
        _otherVelocity,
        dot > 0 ? perpendicular : -perpendicular,
        0.5f
      );
      var escapePosition = Player.transform.position
        + escapeDirection * Player.Config.CloseDistance;

      var escapeRange = distance.ToClampedRange(Player.Config.CloseDistance, 0);
      Player.Agent.speed = escapeRange.Map(0, Player.Config.WalkSpeed);
      Player.Agent.acceleration =
        escapeRange.Map(0, Player.Config.Acceleration);
      Player.Agent.destination = escapePosition;
      Player.Agent.stoppingDistance = 0;
    }
  }
}
