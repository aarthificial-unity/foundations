using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Tweening;

namespace View.Office {
  public class SaveTape : Selectable, IPointerClickHandler, ISubmitHandler {
    public event Action<int> Clicked;

    [SerializeField] private float _extenstion = 0.1f;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Transform _tape;
    [SerializeField] private Transform _tapeContainer;
    private int _index;
    private bool _isSelected;
    private SpringTween _positionTween;
    private SpringConfig _springConfig = SpringConfig.Snappy;

    private void FixedUpdate() {
      if (_positionTween.FixedUpdate(_springConfig)) {
        _tape.localPosition = _positionTween.Value;
      }
    }

    public void SetIndex(int index) {
      _index = index;
      _label.text = $"SAVE {_index + 1}";
    }

    public void Insert(Transform tapePlayer) {
      if (_isSelected) {
        return;
      }

      _isSelected = true;
      _tapeContainer.position = tapePlayer.position;
      _tapeContainer.rotation = tapePlayer.rotation;
      Physics.SyncTransforms();
    }

    public void Eject() {
      if (!_isSelected) {
        return;
      }

      _isSelected = false;
      _tapeContainer.localPosition = Vector3.zero;
      _tapeContainer.localRotation = Quaternion.identity;
      Physics.SyncTransforms();
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      var isFocused = state == SelectionState.Selected
        || state == SelectionState.Pressed;

      _positionTween.Set(Vector3.forward * (isFocused ? _extenstion : 0));
      _springConfig = isFocused ? SpringConfig.Bouncy : SpringConfig.Snappy;
      _label.fontStyle = isFocused
        ? FontStyles.Underline | FontStyles.Bold
        : FontStyles.Bold;
    }

    public void OnPointerClick(PointerEventData eventData) {
      OnSubmit(eventData);
    }

    public void OnSubmit(BaseEventData eventData) {
      Clicked?.Invoke(_index);
    }
  }
}
