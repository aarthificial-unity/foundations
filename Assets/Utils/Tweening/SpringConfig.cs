using System;
using System.Diagnostics;
using UnityEngine;

namespace Utils.Tweening {
  [Serializable]
  public struct SpringConfig : ISerializationCallbackReceiver {
    public static readonly SpringConfig Bouncy = new(200, 17, 0);
    public static readonly SpringConfig Snappy = new(300, 50, 0);
    public static readonly SpringConfig Slow = new(50, 50, 0);

    [SerializeField] private float _frequency;
    [SerializeField] private float _damping;
    [SerializeField] private float _response;

    [NonSerialized] public float K1;
    [NonSerialized] public float K2;
    [NonSerialized] public float K3;

    public SpringConfig(float frequency, float damping, float response) {
      _frequency = frequency;
      _damping = damping;
      _response = response;

      if (_frequency != 0) {
        var w = 2 * Mathf.PI * _frequency;
        K1 = _damping / (Mathf.PI * _frequency);
        K2 = 1 / (Mathf.PI * _frequency);
        K3 = _response * _damping / w;
      } else {
        K1 = 0;
        K2 = 0;
        K3 = 0;
      }
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
      if (_frequency != 0) {
        var w = 2 * Mathf.PI * _frequency;
        K1 = _damping / (Mathf.PI * _frequency);
        K2 = 1 / (Mathf.PI * _frequency);
        K3 = w != 0 ? _response * _damping / w : 0;
      } else {
        K1 = 0;
        K2 = 0;
        K3 = 0;
      }
    }
  }
}
