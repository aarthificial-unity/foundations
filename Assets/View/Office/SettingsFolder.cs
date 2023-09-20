using UnityEngine;
using UnityEngine.Serialization;
using Utils.Tweening;
using View.Office.States;
using View.Settings;

namespace View.Office {
  public class SettingsFolder : MonoBehaviour {
    [SerializeField] private float _closedValue;
    [SerializeField] private float _hoverValue = 0.2f;
    [SerializeField] private float _openValue = 0.5f;
    [SerializeField] private SettingsState _state;
    [FormerlySerializedAs("_tabs")]
    [SerializeField]
    private SettingsView _view;
    [SerializeField] private CanvasGroup _label;

    private Clickable _clickable;
    private BoxCollider _collider;
    private SpringConfig _springConfig = SpringConfig.Snappy;
    private SpringTween _animationTween;
    private SpringTween _labelAlphaTween;

    private void Awake() {
      _clickable = GetComponent<Clickable>();
      _collider = GetComponent<BoxCollider>();

      _clickable.StateChanged += HandleStateChanged;
      _state.Entered += HandleTransitioned;
      _state.Exited += HandleTransitioned;
      _animationTween.ForceSet(_closedValue);
      _labelAlphaTween.ForceSet(1);
    }

    private void HandleStateChanged() {
      if (_state.IsActive) {
        return;
      }

      _springConfig = _clickable.IsFocused
        ? SpringConfig.Bouncy
        : SpringConfig.Snappy;
      _animationTween.Set(_clickable.IsFocused ? _hoverValue : _closedValue);
    }

    private void HandleTransitioned() {
      if (_state.IsActive) {
        _springConfig = SpringConfig.Slow;
        _animationTween.Set(_openValue);
        _collider.enabled = false;
        _clickable.interactable = false;
        _view.SetInteractive(true);
        _labelAlphaTween.Set(0);
      } else {
        _collider.enabled = true;
        _clickable.interactable = true;
        _view.SetInteractive(false);
        _labelAlphaTween.Set(1);
      }
    }

    private void FixedUpdate() {
      if (_animationTween.FixedUpdate(_springConfig)) {
        _view.SetAnimationFactor(_animationTween.X);
      }

      if (_labelAlphaTween.FixedUpdate(SpringConfig.Snappy)) {
        var alpha = _labelAlphaTween.X;
        _label.alpha = alpha;
        _label.interactable = alpha > 0.5;
        _label.blocksRaycasts = alpha > 0.5;
      }
    }
  }
}
