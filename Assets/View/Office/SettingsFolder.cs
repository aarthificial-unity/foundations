using UnityEngine;
using Utils.Tweening;
using View.Office.States;
using View.Settings;

namespace View.Office {
  public class SettingsFolder : MonoBehaviour {
    private static readonly int StateParam = Animator.StringToHash("State");

    [SerializeField] private float _closedValue;
    [SerializeField] private float _hoverValue = 0.2f;
    [SerializeField] private float _openValue = 0.5f;
    [SerializeField] private Animator _animator;
    [SerializeField] private SettingsState _state;
    [SerializeField] private SettingsTabs _tabs;
    [SerializeField] private CanvasGroup _label;

    private Clickable _clickable;
    private BoxCollider _collider;
    private SpringConfig _springConfig = SpringConfig.Snappy;
    private Dynamics _dynamics;
    private Dynamics _labelDynamics;

    private void Awake() {
      _clickable = GetComponent<Clickable>();
      _collider = GetComponent<BoxCollider>();

      _clickable.StateChanged += HandleStateChanged;
      _state.Entered += HandleTransitioned;
      _state.Exited += HandleTransitioned;
      _dynamics.ForceSet(_closedValue);
      _labelDynamics.ForceSet(1);
    }

    private void HandleStateChanged() {
      if (_state.IsActive) {
        return;
      }

      _springConfig = _clickable.IsHovered
        ? SpringConfig.Bouncy
        : SpringConfig.Snappy;
      _dynamics.Set(_clickable.IsHovered ? _hoverValue : _closedValue);
    }

    private void HandleTransitioned() {
      if (_state.IsActive) {
        _springConfig = SpringConfig.Slow;
        _dynamics.Set(_openValue);
        _collider.enabled = false;
        _tabs.SetInteractive(true);
        _labelDynamics.Set(0);
      } else {
        _collider.enabled = true;
        _tabs.SetInteractive(false);
        _labelDynamics.Set(1);
      }
    }

    private void Update() {
      var value = _dynamics.UnscaledUpdate(in _springConfig).x;
      _animator.SetFloat(StateParam, value);
      var alpha = _labelDynamics.UnscaledUpdate(in SpringConfig.Snappy).x;
      _label.alpha = alpha;
      _label.interactable = alpha > 0.5;
      _label.blocksRaycasts = alpha > 0.5;
    }
  }
}
