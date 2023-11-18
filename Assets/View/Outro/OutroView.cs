using Framework;
using UnityEngine;
using View.Controls;

namespace View.Outro {
  public class OutroView : MonoBehaviour {
    [SerializeField] private Backdrop.Handle _backdrop;
    [SerializeField] private PaperButton _menuButton;
    [SerializeField] private PaperButton _exitButton;

    private void Awake() {
      _menuButton.Clicked += HandleMenuClicked;
      _exitButton.Clicked += App.Game.Quit;
    }

    private void Update() {
      if (_backdrop.IsRequested() && _backdrop.IsReady()) {
        App.Game.Menu.Enter();
      }
    }

    private void HandleMenuClicked() {
      _backdrop.Request();
    }
  }
}
