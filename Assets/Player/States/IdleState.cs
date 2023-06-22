namespace Player.States {
  public class IdleState : BaseState {
    public IdleState(PlayerController playerController) :
      base(playerController) { }

    public override void OnUpdate() {
      if (Other.NavigateState.IsActive) {
        Player.FollowState.Enter();
      }
    }
  }
}
