using Framework;
using UnityEngine.InputSystem;

namespace View.Office.States {
  public class SettingsState : MenuState {
    public override void OnEnter() {
      base.OnEnter();
      App.Actions.UICancel.action.performed += HandleCancel;
    }

    public override void OnExit() {
      base.OnExit();
      App.Actions.UICancel.action.performed -= HandleCancel;
    }

    private void HandleCancel(InputAction.CallbackContext obj) {
      if (IsActive) {
        Manager.MainMenuState.Enter();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
