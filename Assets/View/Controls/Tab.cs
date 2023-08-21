using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using View.Office;

namespace View.Controls {
  public class Tab : Selectable,
    IPointerClickHandler,
    IPointerMoveHandler,
    ISubmitHandler {
    public event Action<int> Clicked;

    [Inject] [SerializeField] private OfficePalette _palette;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private bool _isToggled;
    private int _index;

    public void DrivenAwake(int index) {
      _index = index;
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (IsInteractable()) {
        Clicked?.Invoke(_index);
      }
    }

    public void OnSubmit(BaseEventData eventData) {
      if (IsInteractable()) {
        Clicked?.Invoke(_index);
      }
    }

    public void OnPointerMove(PointerEventData eventData) {
      if (IsInteractable()) {
        Select();
      }
    }

    public void Toggle() {
      Toggle(!_isToggled);
    }

    public void Toggle(bool value) {
      _isToggled = value;
      DoStateTransition(currentSelectionState, true);
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      var fontStyle = FontStyles.Normal;
      var onColor = _palette.Ink;
      var onBackground = _palette.PaperSelected;
      var offColor = _palette.PaperSelected;
      var offBackground = Color.clear;

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
          break;
      }

      _text.fontStyle = fontStyle;
      _text.color = _isToggled ? onColor : offColor;
      _background.color = _isToggled ? onBackground : offBackground;
    }
  }
}
