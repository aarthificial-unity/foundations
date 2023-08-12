using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utils;
using Utils.Tweening;

namespace View.Office {
  public class Clickable : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler {
    public event Action Clicked;
    public event Action StateChanged;

    [SerializeField] private UnityEvent _onClick;
    [SerializeField] private ToggleSpringPreset _preset;
    [SerializeField] protected TextMeshProUGUI Label;
    private ToggleSpring _toggle;
    private Color _textColor;
    private FontStyles _fontStyle;
    private bool _isHovered;
    private bool _isInteractable = true;

    public bool IsHovered => _isHovered;
    public bool IsInteractable => _isInteractable;

    protected virtual void Awake() {
      _toggle = _preset.Create();
      _textColor = Label.color;
      _fontStyle = Label.fontStyle;
    }

    private void Update() {
      var value = _toggle.Update(Time.unscaledDeltaTime);
      Label.color = new Color(
        _textColor.r,
        _textColor.g,
        _textColor.b,
        value.Map(_textColor.a, 1f)
      );
    }

    public void SetInteractable(bool interactable) {
      _isInteractable = interactable;
      if (!interactable) {
        _toggle.ForceToggle(false);
      }
      UpdateState();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      _isHovered = true;
      UpdateState();
    }

    public void OnPointerExit(PointerEventData eventData) {
      _isHovered = false;
      UpdateState();
    }

    private void UpdateState() {
      if (_isInteractable && _isHovered) {
        Label.fontStyle = _fontStyle | FontStyles.Underline;
        _toggle.Toggle(true);
      } else {
        Label.fontStyle = _fontStyle;
        _toggle.Toggle(false);
      }
      StateChanged?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData) {
      Clicked?.Invoke();
      _onClick.Invoke();
    }
  }
}
