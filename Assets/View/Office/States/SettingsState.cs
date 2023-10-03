using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Tweening;
using View.Settings;

namespace View.Office.States {
  public class SettingsState : MenuState {
    [SerializeField] private float _closedValue;
    [SerializeField] private float _hoverValue = 0.4f;
    [SerializeField] private float _openValue = 1f;
    [SerializeField] private SettingsView _view;
    [SerializeField] private CanvasGroup _label;
    [SerializeField] private Clickable _clickable;
    [SerializeField] private BoxCollider _collider;

    private SpringConfig _springConfig = SpringConfig.Snappy;
    private SpringTween _animationTween;

    protected override void Awake() {
      base.Awake();
      _clickable.StateChanged += HandleStateChanged;
      _animationTween.ForceSet(_closedValue);
    }

    public override void OnEnter() {
      base.OnEnter();
      App.Actions.UICancel.action.performed += HandleCancel;
      _springConfig = SpringConfig.Slow;
      _animationTween.Set(_openValue);
      _clickable.interactable = false;
      _collider.enabled = false;
      _label.interactable = false;
      _label.blocksRaycasts = false;
      HandleStateChanged();
    }

    public override void OnExit() {
      base.OnExit();
      App.Actions.UICancel.action.performed -= HandleCancel;
      _clickable.interactable = true;
      _collider.enabled = true;
      _label.interactable = true;
      _label.blocksRaycasts = true;
      HandleStateChanged();
    }

    private void HandleCancel(InputAction.CallbackContext obj) {
      Manager.MainMenuState.Enter();
    }

    public void Enter() {
      Manager.SwitchState(this);
    }

    private void HandleStateChanged() {
      if (IsActive) {
        _springConfig = SpringConfig.Slow;
        _animationTween.Set(_openValue);
        return;
      }

      _springConfig = _clickable.IsFocused
        ? SpringConfig.Bouncy
        : SpringConfig.Snappy;
      _animationTween.Set(_clickable.IsFocused ? _hoverValue : _closedValue);
    }

    protected override void OnProgress(float value) {
      _view.SetInteractive(value == 1f);
      _label.alpha = value.ClampRemap(0, 0.75f, 1, 0);
    }

    private void FixedUpdate() {
      if (_animationTween.FixedUpdate(_springConfig)) {
        _view.SetAnimationFactor(_animationTween.X);
      }
    }
  }
}
