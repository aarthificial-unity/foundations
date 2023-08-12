using System;
using UnityEngine;
using Utils.Tweening;
using View.Office.States;

namespace View.Office {
  public class ExitDoor : MonoBehaviour {
    [SerializeField] private float _tiltAngle = 10;
    [SerializeField] private float _openAngle = 90;
    [SerializeField] private Transform _door;
    [SerializeField] private Transform _handle;
    [SerializeField] private Clickable _clickable;
    [SerializeField] private ExitState _state;

    private SpringConfig _springConfig = SpringConfig.Snappy;
    private Dynamics _dynamics;
    private Dynamics _handleDynamics;
    private float _closedAngle;

    private void Awake() {
      _clickable.StateChanged += HandleStateChanged;
      _state.Entered += HandleStateChanged;
      _closedAngle = _door.localRotation.eulerAngles.y;
      _dynamics.ForceSet(_closedAngle);
    }

    private void HandleStateChanged() {
      if (_state.IsActive) {
        _springConfig = SpringConfig.Slow;
        _dynamics.Set(_openAngle);
      } else {
        _springConfig = _clickable.IsHovered
          ? SpringConfig.Bouncy
          : SpringConfig.Snappy;
        _dynamics.Set(_clickable.IsHovered ? _tiltAngle : _closedAngle);

        if (_clickable.IsHovered) {
          _handleDynamics.AddImpulse(new Vector3(-600, 0));
        }
      }
    }

    private void Update() {
      var angle = _dynamics.UnscaledUpdate(in _springConfig).x;
      _door.localRotation = Quaternion.Euler(0, angle, 0);

      var handleAngle =
        _handleDynamics.UnscaledUpdate(in SpringConfig.Bouncy).x;
      _handle.localRotation = Quaternion.Euler(0, 0, handleAngle);
    }
  }
}
