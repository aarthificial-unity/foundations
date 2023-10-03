using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils.Tweening {
  public struct SpringTween {
    private Vector4 _target;
    private Vector4 _position;
    private Vector4 _velocity;
    private bool _settled;
    private bool _notDirty;

    public Vector4 Value {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _position;
    }

    public Quaternion Quaternion {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new Quaternion(_position.x, _position.y, _position.z, _position.w);
    }

    public float X {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _position.x;
    }

    public Vector4 Velocity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _velocity;
    }

    public Vector4 Target {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _target;
    }

    public void ForceSet(Quaternion target) {
      ForceSet(new Vector4(target.x, target.y, target.z, target.w));
    }

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
      _target = target;
      _position = target;
      _velocity = Vector4.zero;
      _settled = true;
      _notDirty = false;
    }

    public void Set(Quaternion target) {
      Set(new Vector4(target.x, target.y, target.z, target.w));
    }

    public void Set(float x, float y, float z) {
      Set(new Vector4(x, y, z, 0));
      _position.w = 0;
      _velocity.w = 0;
    }

    public void Set(float x, float y) {
      Set(new Vector4(x, y, 0, 0));
      _position.z = 0;
      _velocity.z = 0;
      _position.w = 0;
      _velocity.w = 0;
    }

    public void Set(float target) {
      Set(new Vector4(target, 0, 0, 0));
      _position.y = 0;
      _velocity.y = 0;
      _position.z = 0;
      _velocity.z = 0;
      _position.w = 0;
      _velocity.w = 0;
    }

    public void Set(Vector4 target) {
      _target = target;
      _settled = false;
      _notDirty = false;
    }

    public bool Update(SpringConfig config) {
      return Update(Time.deltaTime, config);
    }

    public bool UnscaledUpdate(SpringConfig config) {
      return Update(Time.unscaledDeltaTime, config);
    }

    public bool FixedUpdate(SpringConfig config) {
      return Update(Time.fixedDeltaTime, config);
    }

    public bool FixedUnscaledUpdate(SpringConfig config) {
      return Update(Time.fixedUnscaledDeltaTime, config);
    }

    public void AddImpulse(float impulse) {
      _settled = false;
      _velocity.x += impulse;
    }

    public void AddImpulse(Vector4 impulse) {
      _settled = false;
      _velocity += impulse;
    }

    public void Settle() {
      ForceSet(_position);
    }

    public bool Update(float dt, SpringConfig config) {
      if (dt == 0 || _settled) {
        return !_notDirty;
      }

      var k2Stable = Mathf.Max(
        Mathf.Max(config.K2, dt * dt / 2 + dt * config.K1 / 2),
        dt * config.K1
      );

      _position += dt * _velocity;
      _velocity +=
        dt * (_target - _position - config.K1 * _velocity) / k2Stable;

      if (_velocity.RoughlyEquals(Vector4.zero)
        && _position.RoughlyEquals(_target)) {
        _settled = true;
        _position = _target;
        _velocity = Vector4.zero;
      }

      _notDirty = true;
      return true;
    }
  }
}
