using UnityEngine;
using UnityEngine.Rendering;

namespace View.Office.States {
  public class IntroState : MenuState {
    [SerializeField] private float _duration = 1f;
    [SerializeField] private Backdrop _backdrop;

    private float _startTime;

    protected override void Awake() {
      base.Awake();
      _startTime = Time.time;
      _backdrop.Request();
    }

    public override void OnUpdate() {
      base.OnUpdate();
      if (!SplashScreen.isFinished) {
        _startTime = Time.time;
      }

      if (Time.time - _startTime > _duration) {
        _backdrop.Release();
        Manager.SwitchState(Manager.MainMenuState);
      }
    }
  }
}
