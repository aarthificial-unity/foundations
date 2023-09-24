using FMODUnity;
using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Tweening;

namespace View.Overlay.States {
  public class GameplayState : OverlayState {
    [SerializeField] private StudioEventEmitter _pauseSound;
    [SerializeField] private CanvasGroup _group;

    private SpringTween _alphaTween;

    public override void OnEnter() {
      base.OnEnter();
      _alphaTween.Set(1);
      _group.interactable = true;
      _group.blocksRaycasts = true;
      App.Actions.GameplayPause.action.performed += HandleCancel;
      App.Game.Story.Resume();
    }

    public override void OnExit() {
      base.OnExit();
      _alphaTween.Set(0);
      _group.interactable = false;
      _group.blocksRaycasts = false;
      App.Actions.GameplayPause.action.performed -= HandleCancel;

      if (App.Game != null) {
        App.Game.Story.Pause();
      }
    }

    private void Update() {
      if (_alphaTween.UnscaledUpdate(SpringConfig.Snappy)) {
        _group.alpha = _alphaTween.X;
      }
    }

    private void HandleCancel(InputAction.CallbackContext _) {
      if (IsActive) {
        _pauseSound.Play();
        Manager.PauseState.Enter();
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }
  }
}
