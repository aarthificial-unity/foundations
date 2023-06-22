namespace Player.States {
  public abstract class BaseState {
    public bool IsActive;
    protected readonly PlayerController Player;
    protected PlayerController Other => Player.Other;

    protected BaseState(PlayerController playerController) {
      Player = playerController;
    }

    public virtual void OnUpdate() { }

    public virtual void OnEnter() {
      IsActive = true;
    }

    public virtual void OnExit() {
      IsActive = false;
    }
  }
}
