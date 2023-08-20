namespace View.Office.States {
  public class MainMenuState : MenuState {
    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
