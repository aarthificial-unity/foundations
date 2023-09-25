using Framework;
using Saves;
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
      App.Save.Current = new SaveController { SceneIndex = 1 };
      Manager.SwitchState(this);
    }

    public void ContinueGame() {
      App.Save.Current = new SaveController { SceneIndex = 2 };
      Manager.SwitchState(this);
    }
  }
}
