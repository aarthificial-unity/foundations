using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using View.Office;

namespace View.Settings {
  public class Tab : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler {
    public event Action<int> Clicked;

    [Inject] [SerializeField] private OfficePalette _palette;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private bool _isToggled;
    private bool _isHovered;
    private int _index;

    public void DrivenAwake(int index) {
      _index = index;
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

    public void OnPointerClick(PointerEventData eventData) {
      Clicked?.Invoke(_index);
    }

    public void Toggle() {
      Toggle(!_isToggled);
    }

    public void Toggle(bool value) {
      _isToggled = value;
      Render();
    }

    private void Render() {
      _text.fontStyle = _isHovered ? FontStyles.Underline : FontStyles.Normal;
      _text.color = _isToggled ? _palette.Ink : _palette.PaperSelected;
      _background.color = _isToggled ? _palette.PaperSelected : Color.clear;
    }
  }
}
