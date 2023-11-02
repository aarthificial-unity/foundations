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
    private SpringTween _angleTween;
    private SpringTween _handleTween;
    private float _closedAngle;

    private void Awake() {
      _clickable.StateChanged += HandleStateChanged;
      _state.Entered += HandleStateChanged;
      _closedAngle = _door.localRotation.eulerAngles.y;
      _angleTween.ForceSet(_closedAngle);
    }

    private void HandleStateChanged() {
      if (_state.IsActive) {
        _clickable.interactable = false;
        _springConfig = SpringConfig.Slow;
        _angleTween.Set(_openAngle);
      } else {
        _springConfig = _clickable.IsFocused
          ? SpringConfig.Bouncy
          : SpringConfig.Snappy;
        _angleTween.Set(_clickable.IsFocused ? _tiltAngle : _closedAngle);

        if (_clickable.IsSelected) {
          _handleTween.AddImpulse(new Vector3(-600, 0));
        }
      }
    }

    private void FixedUpdate() {
      if (_angleTween.FixedUpdate(_springConfig)) {
        _door.localRotation = Quaternion.Euler(0, _angleTween.X, 0);
      }

      if (_handleTween.FixedUpdate(SpringConfig.Bouncy)) {
        _handle.localRotation = Quaternion.Euler(0, 0, _handleTween.X);
      }
    }
  }
}
