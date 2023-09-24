using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using View.Controls;

namespace View.Overlay.States {
  public class PauseState : OverlayState {
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
      _exitButton.Clicked += App.Game.Quit;
      _resumeButton.QuietSelect();
      App.Actions.UICancel.action.performed += HandleCancel;
    }

    public override void OnExit() {
      base.OnExit();
      _menu.SetInteractive(false);
      _resumeButton.Clicked -= HandleCancel;
      _settingsButton.Clicked -= Manager.SettingsState.Enter;
      _menuButton.Clicked -= App.Game.Menu.Enter;
      _exitButton.Clicked -= App.Game.Quit;
      App.Actions.UICancel.action.performed -= HandleCancel;
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
