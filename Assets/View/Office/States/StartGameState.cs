using Framework;
using UnityEngine;

namespace View.Office.States {
  public class StartGameState : MenuState {
    [SerializeField] private Backdrop _backdrop;

    public override void OnEnter() {
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        App.Game.Story.Enter();
      }
    }

    public void NewGame() {
      Manager.SwitchState(this);
    }

    public void ContinueGame() {
      Manager.SwitchState(this);
    }
  }
}
