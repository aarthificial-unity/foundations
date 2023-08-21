using Framework;
using System;
using UnityEngine;
using Utils;
using Utils.Tweening;
using View.Overlay.States;

namespace View.Overlay {
  public class OverlayManager : MonoBehaviour {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private Transform _folder;
    [SerializeField] private Backdrop _backdrop;

    [NonSerialized] public ExitState ExitState;
    [NonSerialized] public GameplayState GameplayState;
    [NonSerialized] public PauseState PauseState;
    [NonSerialized] public SettingsState SettingsState;

    private OverlayState _currentState;
    private Backdrop.Handle _backdropHandle;
    public Dynamics PositionDynamics;
    public Dynamics RotationDynamics;

    private void Awake() {
      _backdropHandle = _backdrop.GetHandle();
      ExitState = GetComponent<ExitState>();
      GameplayState = GetComponent<GameplayState>();
      PauseState = GetComponent<PauseState>();
      SettingsState = GetComponent<SettingsState>();
    }

    private void Start() {
      SwitchState(_storyMode.IsPaused ? PauseState : GameplayState);
      PositionDynamics.ForceSet(_currentState.FolderTransform.position);
      RotationDynamics.ForceSet(
        _currentState.FolderTransform.rotation.AsVector()
      );
    }

    private void Update() {
      _currentState?.OnUpdate();
      _folder.position = PositionDynamics.UnscaledUpdate(SpringConfig.Medium);
      _folder.rotation = RotationDynamics.UnscaledUpdate(SpringConfig.Medium)
        .AsQuaternion();
    }

    public void SwitchState(OverlayState state) {
      if (_currentState == state) {
        return;
      }

      if (state == GameplayState) {
        _backdropHandle.Release();
      } else {
        _backdropHandle.Request();
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }
  }
}
