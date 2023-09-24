using System;
using UnityEngine;

namespace View {
  public class Backdrop : MonoBehaviour {
    [Serializable]
    public struct Handle {
      [SerializeField] private Backdrop _backdrop;
      private bool _requested;

      public void Request() {
        if (!_requested) {
          _requested = true;
          _backdrop.Request();
        }
      }

      public void Release() {
        if (_requested) {
          _requested = false;
          _backdrop.Release();
        }
      }

      public bool IsReady() {
        return _backdrop.IsReady();
      }
    }

    [SerializeField] private float _speed;
    [SerializeField] private float _delay;
    private CanvasGroup _group;
    private int _requests;
    private float _initialTime;

    private void Awake() {
      _group = GetComponent<CanvasGroup>();
      _group.alpha = 1;
      _group.blocksRaycasts = true;
      _initialTime = Time.unscaledTime;
    }

    public bool IsReady() {
      return _requests > 0 ? _group.alpha >= 1 : _group.alpha <= 0;
    }

    public void Request() {
      _requests++;
    }

    public void Release() {
      _requests--;
    }

    private void Update() {
      if (Time.unscaledTime - _initialTime < _delay) {
        return;
      }

      var direction = _requests > 0 ? 1 : -1;
      _group.alpha = Mathf.Clamp01(
        _group.alpha + Time.unscaledDeltaTime * _speed * direction
      );
      _group.blocksRaycasts = _requests > 0;
    }
  }
}
