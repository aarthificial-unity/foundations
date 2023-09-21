using Framework;
using UnityEngine;
using Utils;

namespace View.Overlay.States {
  public class ExitState : OverlayState {
    [Inject] [SerializeField] private MenuMode _menuMode;
    [SerializeField] private Backdrop _backdrop;

    public override void OnEnter() {
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        _menuMode.RequestStart();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
