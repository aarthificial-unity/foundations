using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View.Office {
  public class ComputerButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler {
    public event Action Clicked;

    [SerializeField] private bool _isInteractable = true;
    private bool _isHovered;
    private TextMeshProUGUI _text;
    private Color _defaultColor;

    private void Awake() {
      _text = GetComponentInChildren<TextMeshProUGUI>();
      _defaultColor = _text.color;
    }

    private void OnEnable() {
      Render();
    }

    private void OnDisable() {
      _isHovered = false;
    }

    private void Render() {
      _text.fontStyle = _isInteractable && _isHovered
        ? FontStyles.Underline
        : FontStyles.Normal;
      _text.color = new Color(
        _defaultColor.r,
        _defaultColor.g,
        _defaultColor.b,
        _isInteractable ? _defaultColor.a : 0.15f
      );
    }

    public void SetInteractable(bool value) {
      _isInteractable = value;
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

    public void OnPointerDown(PointerEventData eventData) {
      if (_isInteractable) {
        Clicked?.Invoke();
      }
    }
  }
}
