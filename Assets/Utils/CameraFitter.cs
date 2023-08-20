using System;
using UnityEngine;

namespace Utils {
  public struct CameraFitter {
    private float _lastRatio;
    private float _lastFov;
    private readonly float _baseRatio;
    private readonly float _baseFov;
    private readonly float _coefficient;

    public CameraFitter(float baseRatio, float baseFov) {
      _lastRatio = 0;
      _lastFov = baseFov;
      _baseRatio = baseRatio;
      _baseFov = baseFov;
      _coefficient = Mathf.Tan(baseFov * Mathf.Deg2Rad / 2) * _baseRatio;
    }

    public bool TryUpdate(out float fov) {
      fov = _lastFov;
      return Update(ref fov);
    }

    public bool Update(ref float fov) {
      var ratio = Screen.width / (float)Screen.height;
      if (Math.Abs(ratio - _lastRatio) < 0.0001f) {
        return false;
      }

      _lastRatio = ratio;
      if (ratio < _baseRatio) {
        _lastFov = Mathf.Atan(_coefficient / ratio) * Mathf.Rad2Deg * 2;
      } else {
        _lastFov = _baseFov;
      }

      fov = _lastFov;
      return true;
    }
  }
}
