using Framework;
using UnityEngine;

namespace View.Overlay.States {
  public class ExitState : OverlayState {
    [SerializeField] private Backdrop _backdrop;

    public override void OnEnter() {
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        App.Game.Menu.Enter();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
