using System;
using UnityEngine;

namespace Environment {
  public class FlickeringLight : MonoBehaviour {
    [SerializeField] private bool[] _pattern;
    [SerializeField] private float _speed;
    private Light _light;

    private void Awake() {
      _light = GetComponent<Light>();
    }

    private void Update() {
      var index = (int)(Time.time * _speed) % _pattern.Length;
      _light.enabled = _pattern[index];
    }
  }
}
