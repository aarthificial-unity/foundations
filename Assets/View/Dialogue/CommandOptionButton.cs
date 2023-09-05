using Player;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace View.Dialogue {
  public class CommandOptionButton : MonoBehaviour {
    public event UnityAction Clicked {
      add => _button.onClick.AddListener(value);
      remove => _button.onClick.RemoveListener(value);
    }

    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _panel;
    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _transform;
    [SerializeField] private PlayerLookup<Color> _backgroundColors;

    private DialogueView _view;
    private int _index;
    private int _total;
    private float _spacing = Mathf.PI * 0.25f;
    private float _distance = 130f;
    private bool _isLtLeft;
    private bool _isLt;
    private Vector2 _size;

    public void DrivenAwake(DialogueView view) {
      _view = view;
      _size = _transform.sizeDelta;
      gameObject.SetActive(false);
    }

    public void DrivenUpdate(float t) {
      _isLtLeft = _view.ScreenPosition.LT.x < _view.ScreenPosition.RT.x;
      var spacing = _spacing;
      var distance = _distance;
      var angle = (_total - 1) * spacing;
      var from = angle / 2f;
      var sign = _isLt == _isLtLeft ? -1 : 1;

      _panel.color = _isLt ? _backgroundColors.LT : _backgroundColors.RT;
      _transform.pivot = new Vector2(_isLt == _isLtLeft ? 1 : 0, 0.5f);
      _text.rectTransform.pivot = new Vector2(_isLt == _isLtLeft ? 1 : 0, 0.5f);
      _text.rectTransform.anchorMin = _text.rectTransform.anchorMax =
        new Vector2(_isLt == _isLtLeft ? 1 : 0, 0.5f);
      _text.alignment = _isLt == _isLtLeft
        ? TextAlignmentOptions.Right
        : TextAlignmentOptions.Left;
      _transform.anchoredPosition = new Vector2(
        Mathf.Cos(from - spacing * _index) * distance * sign,
        Mathf.Sin(from - spacing * _index) * distance
      );
      _transform.sizeDelta = new Vector2(_size.x * t, _size.y);
    }

    public void SetOption(CommandOption option, int index, int total) {
      _text.text = option.Text;
      _index = index;
      _total = total;
      _isLt = !option.IsRT;
    }
  }
}
