using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Tweening;

namespace View.Office {
  public class SaveTape : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler {
    public event Action<int> Selected;

    [SerializeField] private float _extenstion = 0.1f;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Transform _tape;
    [SerializeField] private Transform _tapeContainer;
    private int _index;
    private bool _isSelected;
    private bool _isHovered;
    private SpringTween _positionTween;
    private SpringConfig _springConfig = SpringConfig.Snappy;

    private void Start() {
      Render();
    }

    private void FixedUpdate() {
      if (_positionTween.FixedUpdate(_springConfig)) {
        _tape.localPosition = _positionTween.Value;
      }
    }

    public void SetIndex(int index) {
      _index = index;
      _label.text = $"SAVE {_index + 1}";
    }

    public void Select(Transform tapePlayer) {
      if (_isSelected) {
        return;
      }

      _isSelected = true;
      _tapeContainer.position = tapePlayer.position;
      _tapeContainer.rotation = tapePlayer.rotation;
      Physics.SyncTransforms();

      _positionTween.ForceSet(Vector3.forward * _extenstion);
      _positionTween.Set(Vector3.zero);
    }

    public void Deselect() {
      if (!_isSelected) {
        return;
      }

      _isSelected = false;
      _tapeContainer.localPosition = Vector3.zero;
      _tapeContainer.localRotation = Quaternion.identity;
      Physics.SyncTransforms();

      _positionTween.ForceSet(Vector3.forward * -_extenstion);
      Render();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      _isHovered = true;
      Render();
    }

    public void OnPointerExit(PointerEventData eventData) {
      _isHovered = false;
      Render();
    }

    private void Render() {
      _positionTween.Set(Vector3.forward * (_isHovered ? _extenstion : 0));
      _springConfig = _isHovered ? SpringConfig.Bouncy : SpringConfig.Snappy;
      _label.fontStyle = _isHovered
        ? FontStyles.Underline | FontStyles.Bold
        : FontStyles.Bold;
    }

    public void OnPointerClick(PointerEventData eventData) {
      Selected?.Invoke(_index);
    }
  }
}
