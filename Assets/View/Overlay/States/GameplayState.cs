using FMODUnity;
using Framework;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Tweening;

namespace View.Overlay.States {
  public class GameplayState : OverlayState {
    [Inject] [SerializeField] private InputChannel _input;
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private StudioEventEmitter _pauseSound;
    [SerializeField] private CanvasGroup _group;

    private Dynamics _dynamics;

    public override void OnEnter() {
      base.OnEnter();
      _dynamics.Set(1);
      _group.interactable = true;
      _group.blocksRaycasts = true;
      _storyMode.Resume();
    }

    public override void OnExit() {
      base.OnExit();
      _dynamics.Set(0);
      _group.interactable = false;
      _group.blocksRaycasts = false;
      _storyMode.Pause();
    }

    private void OnEnable() {
      _input.GameplayPause.action.performed += HandleCancel;
    }

    private void OnDisable() {
      _input.GameplayPause.action.performed -= HandleCancel;
    }

    private void Update() {
      _group.alpha = _dynamics.UnscaledUpdate(SpringConfig.Snappy).x;
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
