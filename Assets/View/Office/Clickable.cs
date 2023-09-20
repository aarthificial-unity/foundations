using Audio;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.Tweening;

namespace View.Office {
  public class Clickable : Selectable, IPointerClickHandler, ISubmitHandler {
    public event Action Clicked;
    public event Action StateChanged;

    public bool IsFocused =>
      currentSelectionState == SelectionState.Selected
      || currentSelectionState == SelectionState.Pressed;

    public bool IsSelected => currentSelectionState == SelectionState.Selected;

    [SerializeField] private UnityEvent _onClick;
    [SerializeField] protected TextMeshProUGUI Label;
    [SerializeField] private FMODEventInstance _clickSound;
    private SpringTween _toggle;
    private Color _textColor;
    private FontStyles _fontStyle;

    protected override void Awake() {
      base.Awake();
      _clickSound.Setup();
      _textColor = Label.color;
      _fontStyle = Label.fontStyle;
    }

    protected override void OnDestroy() {
      _clickSound.Release();
      base.OnDestroy();
    }

    private void FixedUpdate() {
      if (!_toggle.FixedUpdate(
          _toggle.Target.x == 0 ? SpringConfig.Snappy : SpringConfig.Bouncy
        )) {
        return;
      }

      Label.color = new Color(
        _textColor.r,
        _textColor.g,
        _textColor.b,
        _toggle.X.Map(_textColor.a, 1f)
      );
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      if (IsFocused) {
        Label.fontStyle = _fontStyle | FontStyles.Underline;
        _toggle.Set(1);
      } else {
        Label.fontStyle = _fontStyle;
        _toggle.Set(0);
      }

      StateChanged?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData) {
      OnSubmit(eventData);
    }

    public void OnSubmit(BaseEventData eventData) {
      Clicked?.Invoke();
      _onClick.Invoke();
      _clickSound.Play();
    }
  }
}
