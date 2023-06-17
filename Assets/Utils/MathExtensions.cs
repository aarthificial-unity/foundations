using UnityEngine;

namespace Utils {
  public static class MathExtensions {
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
  }
}
