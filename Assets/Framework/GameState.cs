using Player.States;

namespace Framework {
  public class GameState : BaseState {
    protected GameManager Manager;

    protected virtual void Awake() {
      Manager = GetComponent<GameManager>();
    }
  }
}
