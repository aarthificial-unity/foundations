namespace Player.States {
  public class IdleState : PlayerState {
    public override void OnUpdate() {
      Player.Agent.destination = Player.transform.position;
      if (Other.NavigateState.IsActive || Other.InteractState.IsActive) {
        Player.FollowState.Enter();
      }
      base.OnUpdate();
    }
  }
}
