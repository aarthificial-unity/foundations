using Framework;
using Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using View.Controls;
using View.Office;
using View.Settings;

namespace View.Overlay.States {
  public class PauseState : OverlayState {
    [Inject] [SerializeField] private InputChannel _input;
    [Inject] [SerializeField] private MenuMode _menuMode;

    [SerializeField] private Camera _worldCamera;
    [SerializeField] private InteractiveGroup _menu;
    [SerializeField] private PaperButton _resumeButton;
    [SerializeField] private PaperButton _settingsButton;
    [SerializeField] private PaperButton _menuButton;
    [SerializeField] private PaperButton _exitButton;

    protected override void Awake() {
      base.Awake();
      _menu.DrivenAwake(_worldCamera);
    }

    public override void OnEnter() {
      base.OnEnter();
      _menu.SetInteractive(true);
      _resumeButton.Clicked += HandleCancel;
      _settingsButton.Clicked += Manager.SettingsState.Enter;
      _menuButton.Clicked += Manager.ExitState.Enter;
      _exitButton.Clicked += _menuMode.Quit;
      _resumeButton.QuietSelect();
    }

    public override void OnExit() {
      base.OnExit();
      _menu.SetInteractive(false);
      _resumeButton.Clicked -= HandleCancel;
      _settingsButton.Clicked -= Manager.SettingsState.Enter;
      _menuButton.Clicked -= _menuMode.RequestStart;
      _exitButton.Clicked -= _menuMode.Quit;
    }

    private void OnEnable() {
      _input.UICancel.action.performed += HandleCancel;
    }

    private void OnDisable() {
      _input.UICancel.action.performed -= HandleCancel;
    }

    private void HandleCancel(InputAction.CallbackContext _) {
      if (IsActive) {
        HandleCancel();
      }
    }

    private void HandleCancel() {
      Manager.GameplayState.Enter();
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
