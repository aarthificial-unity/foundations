using Framework;
using Saves;
using UnityEngine;
using UnityEngine.Assertions;

namespace View.Office.States {
  public class StartGameState : MenuState {
    [SerializeField] private Backdrop _backdrop;
    private SaveController _save;

    public override void OnEnter() {
      Assert.IsNotNull(_save);
      base.OnEnter();
      _backdrop.Request();
    }

    public override void OnExit() {
      base.OnExit();
      _save = null;
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (_backdrop.IsReady()) {
        App.Game.Story.Enter(_save);
      }
    }

    public void Enter(int saveIndex) {
      _save = App.Save.GetSaveController(saveIndex);
      Manager.SwitchState(this);
    }
  }
}
