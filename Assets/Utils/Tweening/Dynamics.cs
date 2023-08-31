using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.Tweening {
  public struct Dynamics {
    private Vector4 _lastTarget;
    private Vector4 _target;
    private Vector4 _position;
    private Vector4 _velocity;

    public Vector4 Position => _position;
    public Vector4 Velocity => _velocity;
    public Vector4 Target => _target;

    public void ForceSet(float x, float y, float z) {
      ForceSet(new Vector4(x, y, z, 0));
    }

    public void ForceSet(float x, float y) {
      ForceSet(new Vector4(x, y, 0, 0));
    }

    public void ForceSet(float target) {
      ForceSet(new Vector4(target, 0, 0, 0));
    }

    public void ForceSet(Vector4 target) {
      _lastTarget = target;
      _target = target;
      _position = target;
      _velocity = Vector4.zero;
    }

    public void Set(float x, float y, float z) {
      Set(new Vector4(x, y, z, 0));
    }

    public void Set(float x, float y) {
      Set(new Vector4(x, y, 0, 0));
    }

    public void Set(float target) {
      Set(new Vector4(target, 0, 0, 0));
    }

    public void Set(Vector4 target) {
      _target = target;
    }

    public Vector4 Update(in SpringConfig config) {
      return Update(Time.deltaTime, in config);
    }

    public Vector4 UnscaledUpdate(in SpringConfig config) {
      return Update(Time.unscaledDeltaTime, in config);
    }

    public Vector4 FixedUpdate(in SpringConfig config) {
      return Update(Time.fixedDeltaTime, in config);
    }

    public Vector4 FixedUnscaledUpdate(in SpringConfig config) {
      return Update(Time.fixedUnscaledDeltaTime, in config);
    }

    public void AddImpulse(float impulse) {
      _velocity.x += impulse;
    }

    public void AddImpulse(Vector4 impulse) {
      _velocity += impulse;
    }

    public void Settle() {
      ForceSet(_position);
    }

    public Vector4 Update(float dt, in SpringConfig config) {
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
