using Framework;
using UnityEngine;

namespace View.Overlay.States {
  public class SwapState : OverlayState {
    [SerializeField] private Backdrop.Handle _backdrop;

    private string _scenePath;
    private string _isLoaded;

    public override void OnEnter() {
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        App.Game.Story.SwapScene(_scenePath);
      }
    }

    public void Enter(string scenePath) {
      _scenePath = scenePath;
      Manager.SwitchState(this);
    }
  }
}
