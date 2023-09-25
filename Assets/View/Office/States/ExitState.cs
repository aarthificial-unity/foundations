using Framework;
using UnityEngine;

namespace View.Office.States {
  public class ExitState : MenuState {
    [SerializeField] private float _duration = 1f;
    private float _enterTime;

    public override void OnEnter() {
      base.OnEnter();
      _enterTime = Time.unscaledTime;
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (Time.unscaledTime - _enterTime > _duration) {
        App.Game.Quit();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
