using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.Office;

namespace View.Controls {
  public class PaperStyle : MonoBehaviour {
    public enum SelectionState {
      Normal,
      Highlighted,
      Pressed,
      Selected,
      Disabled,
    }

    [Inject] [SerializeField] private OfficePalette _palette;
    [SerializeField] private TextMeshProUGUI[] _texts;
    [SerializeField] private TextMeshProUGUI[] _underline;
    [SerializeField] private Image[] _images;

    [SerializeField] private Image[] _backgrounds;

    private void Awake() {
      for (var i = 0; i < _backgrounds.Length; i++) {
        _backgrounds[i].color = _palette.PaperSelected;
      }
      DoStateTransition(SelectionState.Normal);
    }

    public void DoStateTransition(SelectionState state) {
      var fontStyle = FontStyles.Normal;
      var color = _palette.Ink;

      switch (state) {
        case SelectionState.Normal:
          break;
        case SelectionState.Highlighted:
          // fontStyle = FontStyles.Underline;
          color = _palette.InkHovered;
          break;
        case SelectionState.Pressed:
          fontStyle = FontStyles.Underline;
          break;
        case SelectionState.Selected:
          fontStyle = FontStyles.Underline;
          break;
        case SelectionState.Disabled:
          color = _palette.InkDisabled;
          break;
      }

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
  }
}
