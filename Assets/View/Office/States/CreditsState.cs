namespace View.Office.States {
  public class CreditsState : MenuState {
    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
