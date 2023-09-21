﻿using UnityEngine;

namespace Utils {
  public static class MathExtensions {
    public static float ToRange(this float value, float from, float to) {
      return (value - from) / (to - from);
    }

    public static float ToClampedRange(this float value, float from, float to) {
      return Mathf.Clamp((value - from) / (to - from), 0, 1);
    }

    public static float Map(this float value, float fromOut, float toOut) {
      return value * (toOut - fromOut) + fromOut;
    }

    public static float ClampMap(this float value, float fromOut, float toOut) {
      return Mathf.Clamp(value * (toOut - fromOut) + fromOut, fromOut, toOut);
    }

    public static float Remap(
      this float value,
      float fromIn,
      float toIn,
      float fromOut,
      float toOut
    ) {
      return (value - fromIn) / (toIn - fromIn) * (toOut - fromOut) + fromOut;
    }

    public static float ClampRemap(
      this float value,
      float fromIn,
      float toIn,
      float fromOut,
      float toOut
    ) {
      return Mathf.Clamp(
        (value - fromIn) / (toIn - fromIn) * (toOut - fromOut) + fromOut,
        fromOut,
        toOut
      );
    }

    public static bool RoughlyEquals(
      this Vector4 lhs,
      Vector4 rhs,
      float precision = 0.0001f
    ) {
      return Mathf.Abs(lhs.x - rhs.x) < precision
        && Mathf.Abs(lhs.y - rhs.y) < precision
        && Mathf.Abs(lhs.z - rhs.z) < precision
        && Mathf.Abs(lhs.w - rhs.w) < precision;
    }
  }
}
