using Framework;
using System;
using UnityEngine;
using Utils;
using Utils.Tweening;
using View.Overlay.States;

namespace View.Overlay {
  public class OverlayManager : MonoBehaviour {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] private Transform _folder;
    [SerializeField] private Backdrop.Handle _backdrop;

    [NonSerialized] public ExitState ExitState;
    [NonSerialized] public SwapState SwapState;
    [NonSerialized] public GameplayState GameplayState;
    [NonSerialized] public PauseState PauseState;
    [NonSerialized] public SettingsState SettingsState;

    private OverlayState _currentState;
    [NonSerialized] public SpringTween PositionTween;
    [NonSerialized] public SpringTween RotationTween;

    private void Awake() {
      _overlay.Manager = this;
      ExitState = GetComponent<ExitState>();
      SwapState = GetComponent<SwapState>();
      GameplayState = GetComponent<GameplayState>();
      PauseState = GetComponent<PauseState>();
      SettingsState = GetComponent<SettingsState>();
    }

    private void Start() {
      SwitchState(_storyMode.IsPaused ? PauseState : GameplayState);
      PositionTween.ForceSet(_currentState.FolderTransform.localPosition);
      RotationTween.ForceSet(_currentState.FolderTransform.localRotation);
    }

    private void Update() {
      _currentState?.OnUpdate();

      if (PositionTween.UnscaledUpdate(SpringConfig.Medium)) {
        _folder.localPosition = PositionTween.Value;
      }
      if (RotationTween.UnscaledUpdate(SpringConfig.Medium)) {
        _folder.localRotation = RotationTween.Quaternion;
      }
    }

    public void SwitchState(OverlayState state) {
      if (_currentState == state) {
        return;
      }

      if (state == GameplayState) {
        _backdrop.Release();
      } else {
        _backdrop.Request();
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }
  }
}
