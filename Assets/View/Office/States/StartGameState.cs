using Framework;
using UnityEngine;
using Utils;

namespace View.Office.States {
  public class StartGameState : MenuState {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private Backdrop _backdrop;
    private bool _newGame;

    public override void OnEnter() {
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        _storyMode.RequestStart();
      }
    }

    public void NewGame() {
      _newGame = true;
      Manager.SwitchState(this);
    }

    public void ContinueGame() {
      _newGame = false;
      Manager.SwitchState(this);
    }
  }
}
