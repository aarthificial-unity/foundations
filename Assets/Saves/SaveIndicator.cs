using Framework;
using UnityEngine;
using Utils.Tweening;

namespace Saves {
  public class SaveIndicator : MonoBehaviour {
    [SerializeField] private float _angleRange;
    [SerializeField] private float _speed;
    [SerializeField] private float _minDuration = 1;
    private SpringTween _scaleTween;
    private float _angle;
    private float _lastLoadingTime = -1;

    private void Update() {
      transform.rotation = Quaternion.Euler(
        0,
        0,
        Mathf.Sin(Time.unscaledTime * _speed) * _angleRange
      );
    }

    private void FixedUpdate() {
      if (App.Game.Story.IsLoading) {
        _lastLoadingTime = Time.unscaledTime;
      }
      _scaleTween.Set(
        Time.unscaledTime - _lastLoadingTime > _minDuration ? -1 : 1
      );

      if (_scaleTween.FixedUpdate(SpringConfig.Medium)) {
        transform.localScale = _scaleTween.X > 0
          ? Vector3.one * _scaleTween.X
          : Vector3.zero;
      }
    }
  }
}
