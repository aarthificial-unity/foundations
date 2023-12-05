using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace Input {
  public class InputManager : MonoBehaviour {
    public InputActions Actions;
    [NonSerialized] public string CurrentMap = "UI";
    [SerializeField] private InputActionAsset _actions;
    private InputUser _user;
    private InputActionMap _currentActionMap;

    public void SwitchToGameplay() {
      SetMap("Gameplay");
    }

    public void SwitchToUI() {
      SetMap("UI");
    }

    public void SetMap(string map) {
      CurrentMap = map;
      HandleMapChanged(CurrentMap);
    }

    private void OnEnable() {
      _user = InputUser.CreateUserWithoutPairedDevices();

      using (var availableDevices = InputUser.GetUnpairedInputDevices()) {
        var controlScheme = InputControlScheme.FindControlSchemeForDevices(
          availableDevices,
          _actions.controlSchemes
        );
        if (controlScheme.HasValue) {
          _user.ActivateControlScheme(controlScheme.Value)
            .AndPairRemainingDevices();
        }
      }

      if (_user.valid) {
        _user.AssociateActionsWithUser(_actions);
      }

      _actions.Disable();
      _actions.FindActionMap("Pointing").Enable();
      HandleMapChanged(CurrentMap);
      InputUser.onChange += HandleUserChange;
      InputUser.onPrefilterUnpairedDeviceActivity +=
        HandlePrefilterUnpairedDeviceActivity;
      InputUser.onUnpairedDeviceUsed += HandleUnpairedDeviceUsed;
      InputUser.listenForUnpairedDeviceActivity++;
    }

    private void OnDisable() {
      InputUser.listenForUnpairedDeviceActivity--;
      InputUser.onUnpairedDeviceUsed -= HandleUnpairedDeviceUsed;
      InputUser.onPrefilterUnpairedDeviceActivity -=
        HandlePrefilterUnpairedDeviceActivity;
      InputUser.onChange -= HandleUserChange;
      _actions.Disable();
      if (_user.valid) {
        _user.UnpairDevicesAndRemoveUser();
      }

      _actions.devices = null;
    }

    private void HandleMapChanged(string map) {
      var oldMap = _currentActionMap;
      _currentActionMap = null;
      oldMap?.Disable();
      _currentActionMap = _actions.FindActionMap(map);
      _currentActionMap?.Enable();

      if (_currentActionMap == null) {
        _actions.FindActionMap("Pointing").Disable();
      } else {
        _actions.FindActionMap("Pointing").Enable();
      }
    }

    private bool HandlePrefilterUnpairedDeviceActivity(
      InputDevice device,
      InputEventPtr eventPtr
    ) {
      return _actions.IsUsableWithDevice(device);
    }

    private void HandleUnpairedDeviceUsed(
      InputControl control,
      InputEventPtr eventPtr
    ) {
      var device = control.device;
      using var availableDevices = InputUser.GetUnpairedInputDevices();

      if (availableDevices.Count > 1) {
        var indexOfDevice = availableDevices.IndexOf(device);
        availableDevices.SwapElements(0, indexOfDevice);
      }

      if (_user.valid) {
        availableDevices.AddRange(_user.pairedDevices);
      }

      if (InputControlScheme.FindControlSchemeForDevices(
          availableDevices,
          _actions.controlSchemes,
          out var controlScheme,
          out var matchResult,
          device
        )) {
        try {
          if (_user.valid) {
            _user.UnpairDevices();
          }

          _user.ActivateControlScheme(controlScheme).AndPairRemainingDevices();

          if (_user.valid) {
            _user.AssociateActionsWithUser(_actions);
          }
        } finally {
          matchResult.Dispose();
        }
      }
    }

    private void HandleUserChange(
      InputUser user,
      InputUserChange change,
      InputDevice device
    ) {
      switch (change) {
        case InputUserChange.DeviceLost:
        case InputUserChange.DeviceRegained:
        case InputUserChange.ControlsChanged:
          // TODO Handle these
          break;
      }
    }
  }
}
