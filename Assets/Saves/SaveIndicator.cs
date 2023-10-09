using Framework;
using System;
using UnityEngine;
using Utils.Tweening;

namespace Saves {
  public class SaveIndicator : MonoBehaviour {
    [SerializeField] private float _angleRange;
    [SerializeField] private float _speed;
    private SpringTween _scaleTween;
    private float _angle;

    private void Awake() {
      App.Game.Story.LoadingChanged += HandleLoadingChanged;
      HandleLoadingChanged(App.Game.Story.IsLoading);
    }

    private void Update() {
      transform.rotation = Quaternion.Euler(
        0,
        0,
        Mathf.Sin(Time.unscaledTime * _speed) * _angleRange
      );
    }

    private void FixedUpdate() {
      if (_scaleTween.FixedUpdate(SpringConfig.Medium)) {
        transform.localScale = _scaleTween.X > 0
          ? Vector3.one * _scaleTween.X
          : Vector3.zero;
      } else if (!App.Game.Story.IsLoading) {
        gameObject.SetActive(false);
      }
    }

    private void OnDestroy() {
      App.Game.Story.LoadingChanged -= HandleLoadingChanged;
    }

    private void HandleLoadingChanged(bool isLoading) {
      if (isLoading) {
        gameObject.SetActive(true);
      }
      _scaleTween.Set(isLoading ? 1 : -1);
    }
  }
}
