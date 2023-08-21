using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace View.Office.States {
  public class SettingsState : MenuState {
    [Inject] [SerializeField] private InputChannel _input;

    private void OnEnable() {
      _input.UICancel.action.performed += HandleCancel;
    }

    public void OnDisable() {
      _input.UICancel.action.performed -= HandleCancel;
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
