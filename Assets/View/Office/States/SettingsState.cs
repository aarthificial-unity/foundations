using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace View.Office.States {
  public class SettingsState : MenuState {
    [Inject] [SerializeField] private InputChannel _input;

    public override void OnEnter() {
      base.OnEnter();
      _input.UICancel.action.performed += HandleCancel;
    }

    public override void OnExit() {
      base.OnExit();
      _input.UICancel.action.performed -= HandleCancel;
    }

    private void HandleCancel(InputAction.CallbackContext obj) {
      Manager.MainMenuState.Enter();
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
