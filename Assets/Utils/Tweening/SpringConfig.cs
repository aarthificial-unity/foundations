using System;
using System.Diagnostics;
using UnityEngine;

namespace Utils.Tweening {
  [Serializable]
  public class SpringConfig : ISerializationCallbackReceiver {
    public static readonly SpringConfig Bouncy = new(200, 17);
    public static readonly SpringConfig Snappy = new(300, 50);
    public static readonly SpringConfig Medium = new(150, 50);
    public static readonly SpringConfig Slow = new(50, 50);

    [SerializeField] private float _frequency;
    [SerializeField] private float _damping;

    [NonSerialized] public float K1;
    [NonSerialized] public float K2;

    public SpringConfig(float frequency, float damping) {
      _frequency = frequency;
      _damping = damping;

      if (_frequency != 0) {
        K1 = _damping / (Mathf.PI * _frequency);
        K2 = 1 / (Mathf.PI * _frequency);
      } else {
        K1 = 0;
        K2 = 0;
      }
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
      if (_frequency != 0) {
        var w = 2 * Mathf.PI * _frequency;
        K1 = _damping / (Mathf.PI * _frequency);
        K2 = 1 / (Mathf.PI * _frequency);
      } else {
        K1 = 0;
        K2 = 0;
      }
    }
  }
}
