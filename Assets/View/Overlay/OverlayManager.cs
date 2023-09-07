﻿using Framework;
using System;
using UnityEngine;
using Utils;
using Utils.Tweening;
using View.Overlay.States;

namespace View.Overlay {
  public class OverlayManager : MonoBehaviour {
    [Inject] [SerializeField] private StoryMode _storyMode;
    [SerializeField] private Transform _folder;
    [SerializeField] private Backdrop.Handle _backdrop;

    [NonSerialized] public ExitState ExitState;
    [NonSerialized] public GameplayState GameplayState;
    [NonSerialized] public PauseState PauseState;
    [NonSerialized] public SettingsState SettingsState;

    private OverlayState _currentState;
    public Dynamics PositionDynamics;
    public Dynamics RotationDynamics;

    private void Awake() {
      ExitState = GetComponent<ExitState>();
      GameplayState = GetComponent<GameplayState>();
      PauseState = GetComponent<PauseState>();
      SettingsState = GetComponent<SettingsState>();
    }

    private void Start() {
      SwitchState(_storyMode.IsPaused ? PauseState : GameplayState);
      PositionDynamics.ForceSet(_currentState.FolderTransform.localPosition);
      RotationDynamics.ForceSet(
        _currentState.FolderTransform.localRotation.AsVector()
      );
    }

    private void Update() {
      _currentState?.OnUpdate();
      _folder.localPosition =
        PositionDynamics.UnscaledUpdate(SpringConfig.Medium);
      _folder.localRotation = RotationDynamics
        .UnscaledUpdate(SpringConfig.Medium)
        .AsQuaternion();
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
