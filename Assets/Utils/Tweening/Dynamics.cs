using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.Tweening {
  public struct Dynamics {
    private Vector3 _lastTarget;
    private Vector3 _target;
    private Vector3 _position;
    private Vector3 _velocity;

    public void ForceSet(float x, float y, float z) {
      ForceSet(new Vector3(x, y, z));
    }

    public void ForceSet(float x, float y) {
      ForceSet(new Vector3(x, y, 0));
    }

    public void ForceSet(float target) {
      ForceSet(new Vector3(target, 0, 0));
    }

    public void ForceSet(Vector3 target) {
      _lastTarget = target;
      _target = target;
      _position = target;
      _velocity = Vector3.zero;
    }

    public void Set(float x, float y, float z) {
      Set(new Vector3(x, y, z));
    }

    public void Set(float x, float y) {
      Set(new Vector3(x, y, 0));
    }

    public void Set(float target) {
      Set(new Vector3(target, 0, 0));
    }

    public void Set(Vector3 target) {
      _target = target;
    }

    public Vector3 Update(in SpringConfig config) {
      return Update(Time.deltaTime, in config);
    }

    public Vector3 UnscaledUpdate(in SpringConfig config) {
      return Update(Time.unscaledDeltaTime, in config);
    }

    public Vector3 FixedUpdate(in SpringConfig config) {
      return Update(Time.fixedDeltaTime, in config);
    }

    public Vector3 FixedUnscaledUpdate(in SpringConfig config) {
      return Update(Time.fixedUnscaledDeltaTime, in config);
    }

    public void AddImpulse(float impulse) {
      _velocity.x += impulse;
    }

    public void AddImpulse(Vector3 impulse) {
      _velocity += impulse;
    }

    public Vector3 Update(float dt, in SpringConfig config) {
      if (dt == 0) {
        return _position;
      }

      var targetVelocity = (_target - _lastTarget) / dt;
      _lastTarget = _target;

      var k2Stable = Mathf.Max(
        config.K2,
        dt * dt / 2 + dt * config.K1 / 2,
        dt * config.K1
      );

      Assert.IsFalse(float.IsNaN(_velocity.x));
      Assert.IsFalse(float.IsNaN(_velocity.y));
      Assert.IsFalse(float.IsNaN(_velocity.z));

      _position += dt * _velocity;
      _velocity +=
        dt
        * (_target
          + config.K3 * targetVelocity
          - _position
          - config.K1 * _velocity)
        / k2Stable;

      return _position;
    }
  }
}
