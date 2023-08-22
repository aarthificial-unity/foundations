using FMODUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.Controls {
  public class PaperButton : Selectable, IPointerClickHandler, ISubmitHandler {
    public event Action Clicked;
    private TextMeshProUGUI _text;
    private Color _defaultColor;

    [SerializeField] private float _disabledAlpha = 0.15f;
    [SerializeField] private StudioEventEmitter _focusSound;
    [SerializeField] private StudioEventEmitter _clickSound;

    protected override void Awake() {
      _text = GetComponentInChildren<TextMeshProUGUI>();
      _defaultColor = _text.color;
      base.Awake();
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      if (_text == null) {
        return;
      }

      var fontStyle = FontStyles.Normal;
      var alpha = _defaultColor.a;

      switch (state) {
        case SelectionState.Normal:
          break;
        case SelectionState.Highlighted:
          fontStyle = FontStyles.Underline;
          break;
        case SelectionState.Pressed:
          fontStyle = FontStyles.Underline;
          break;
        case SelectionState.Selected:
          fontStyle = FontStyles.Underline;
          break;
        case SelectionState.Disabled:
          alpha = _disabledAlpha;
          break;
      }

      _text.fontStyle = fontStyle;
      _text.color = new Color(
        _defaultColor.r,
        _defaultColor.g,
        _defaultColor.b,
        alpha
      );
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (IsInteractable()) {
        _clickSound.Play();
        Clicked?.Invoke();
      }
    }

    public void OnSubmit(BaseEventData eventData) {
      if (IsInteractable()) {
        Clicked?.Invoke();
      }
    }

    public override void OnSelect(BaseEventData eventData) {
      base.OnSelect(eventData);
      if (IsInteractable()) {
        _focusSound.Play();
      }
    }
  }
}
