namespace Player.States {
  public class IdleState : PlayerState {
    public override void OnUpdate() {
      if (Other.NavigateState.IsActive) {
        Player.FollowState.Enter();
      }
    }
  }
}
