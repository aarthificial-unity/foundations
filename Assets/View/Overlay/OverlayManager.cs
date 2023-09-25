using System;
using UnityEngine;
using Utils.Tweening;
using View.Overlay.States;

namespace View.Overlay {
  [DefaultExecutionOrder(-100)]
  public class OverlayManager : MonoBehaviour {
    public static Camera Camera => FindObjectOfType<OverlayManager>()._camera;

    [SerializeField] private Camera _camera;
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
      ExitState = GetComponent<ExitState>();
      SwapState = GetComponent<SwapState>();
      GameplayState = GetComponent<GameplayState>();
      PauseState = GetComponent<PauseState>();
      SettingsState = GetComponent<SettingsState>();
    }

    private void Start() {
      SwitchState(GameplayState);
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
