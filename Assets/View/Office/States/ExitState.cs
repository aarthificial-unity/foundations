using Framework;
using UnityEngine;
using Utils;

namespace View.Office.States {
  public class ExitState : MenuState {
    [Inject] [SerializeField] private MenuMode _menuMode;
    [SerializeField] private float _duration = 1f;
    private float _enterTime;

    public override void OnEnter() {
      base.OnEnter();
      _enterTime = Time.unscaledTime;
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (Time.unscaledTime - _enterTime > _duration) {
        _menuMode.Quit();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
