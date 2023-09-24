using Framework;
using UnityEngine;
using Utils;

namespace View.Overlay.States {
  public class SwapState : OverlayState {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private Backdrop.Handle _backdrop;

    private string _scenePath;
    private string _isLoaded;
    private bool _swapped;

    public override void OnEnter() {
      base.OnEnter();
      _swapped = false;
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady() && !_swapped) {
        _swapped = true;
        _storyMode.SwapScene(_scenePath);
      }

      if (_swapped && _storyMode.IsReady()) {
        _backdrop.Release();
        Manager.GameplayState.Enter();
      }
    }

    public void Enter(string scenePath) {
      _scenePath = scenePath;
      Manager.SwitchState(this);
    }
  }
}
