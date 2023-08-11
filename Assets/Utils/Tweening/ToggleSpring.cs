using System;

namespace Utils.Tweening {
  [Serializable]
  public struct ToggleSpring {
    private Dynamics _dynamics;
    private ToggleSpringPreset _preset;
    private bool _toggled;

    public ToggleSpring(ToggleSpringPreset preset) {
      _preset = preset;
      _dynamics = new Dynamics();
      _toggled = false;
    }

    public void Toggle() {
      Toggle(!_toggled);
    }

    public void Toggle(bool value) {
      _toggled = value;
      _dynamics.Set(_toggled ? 1 : 0);
    }

    public void ForceToggle(bool value) {
      _toggled = value;
      _dynamics.ForceSet(_toggled ? 1 : 0);
    }

    public float Update(float dt) {
      var preset = _preset.CustomOff && !_toggled ? _preset.Off : _preset.On;
      return _dynamics.Update(dt, in preset).x;
    }
  }
}
