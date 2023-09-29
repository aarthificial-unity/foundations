using UnityEngine;
using Utils;

namespace Environment {
  public class HighwayLights : MonoBehaviour {
    [SerializeField] private Vector2 _speed;
    [SerializeField] private float _fluctuation;

    private float _angle;

    private void Awake() {
      _angle = transform.localRotation.eulerAngles.y;
    }

    private void Update() {
      var speed = Mathf.Sin(Time.time * _fluctuation)
        .Remap(-1, 1, _speed.x, _speed.y);
      _angle += speed * Time.deltaTime;
      transform.localRotation = Quaternion.Euler(0, _angle, 0);
    }
  }
}
