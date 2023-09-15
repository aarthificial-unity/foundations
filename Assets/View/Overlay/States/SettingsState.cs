using Input;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;
using Utils.Tweening;
using View.Settings;

namespace View.Overlay.States {
  public class SettingsState : OverlayState {
    [Inject] [SerializeField] private InputChannel _input;
    [SerializeField] private SettingsView _view;

    private SpringTween _animationTween;

    public override void OnEnter() {
      base.OnEnter();
      _view.SetInteractive(true);
      _animationTween.Set(1);
      _view.ExitButton.Clicked += HandleCancel;
    }

    public override void OnExit() {
      base.OnExit();
      _view.SetInteractive(false);
      _animationTween.Set(0);
      _view.ExitButton.Clicked -= HandleCancel;
    }

    private void OnEnable() {
      _input.UICancel.action.performed += HandleCancel;
    }

    private void OnDisable() {
      _input.UICancel.action.performed -= HandleCancel;
    }

    private void Update() {
      if (_animationTween.UnscaledUpdate(SpringConfig.Medium)) {
        _view.SetAnimationFactor(_animationTween.X);
      }
    }

    private void HandleCancel(InputAction.CallbackContext _) {
      if (IsActive) {
        if (EventSystem.current != null
          && EventSystem.current.currentSelectedGameObject != null
          && EventSystem.current.currentSelectedGameObject
            .TryGetComponent<Dropdown>(out var dropdown)) {
          dropdown.Hide();
        }
        HandleCancel();
      }
    }

    private void HandleCancel() {
      Manager.PauseState.Enter();
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
