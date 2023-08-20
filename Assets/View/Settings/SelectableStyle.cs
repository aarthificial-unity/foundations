using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using View.Office;

namespace View.Settings {
  public class SelectableStyle : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler {
    [Inject] [SerializeField] private OfficePalette _palette;
    [SerializeField] private TextMeshProUGUI[] _texts;
    [SerializeField] private TextMeshProUGUI[] _underline;
    [SerializeField] private Image[] _images;
    [SerializeField] private Image[] _backgrounds;
    private Selectable _selectable;
    private bool _isInteractable;
    private bool _isSelected;
    private bool _isHovered;

    private void Awake() {
      _selectable = GetComponent<Selectable>();
      _isInteractable = _selectable.interactable;
      for (var i = 0; i < _backgrounds.Length; i++) {
        _backgrounds[i].color = _palette.PaperSelected;
      }
      Render();
    }

    private void Update() {
      var isInteractable = _selectable.IsInteractable();
      var isSelected =
        EventSystem.current.currentSelectedGameObject == gameObject;
      if (isInteractable != _isInteractable || isSelected != _isSelected) {
        _isInteractable = isInteractable;
        _isSelected = isSelected;
        Render();
      }
    }

    private void Render() {
      var color = _isInteractable
        ? (_isHovered || _isSelected) ? _palette.InkHovered : _palette.Ink
        : _palette.InkDisabled;

      var fontStyle = _isInteractable & (_isHovered || _isSelected)
        ? FontStyles.Underline
        : FontStyles.Normal;

      for (var i = 0; i < _texts.Length; i++) {
        _texts[i].color = color;
      }

      for (var i = 0; i < _images.Length; i++) {
        _images[i].color = color;
      }

      for (var i = 0; i < _underline.Length; i++) {
        _underline[i].fontStyle = fontStyle;
      }
    }

    public void OnPointerEnter(PointerEventData eventData) {
      _isHovered = true;
      Render();
    }

    public void OnPointerExit(PointerEventData eventData) {
      _isHovered = false;
      Render();
    }
  }
}
