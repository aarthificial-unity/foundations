using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    private int _index;
    private int _total;
    private float _spacing = Mathf.PI * 0.25f;
    private float _distance = 120f;
    private bool _isLtLeft;
    private bool _isLt;
    private Color _rtColor = new(0.945f, 0.753f, 0.714f);
    private Color _ltColor = new(0.655f, 0.835f, 0.886f);

    public void SetOption(CommandOption option, int index, int total) {
      _text.text = option.Text;
      _index = index;
      _total = total;
      _isLt = !option.IsRT;
      UpdatePosition();
    }

    public void SetLTLeft(bool isLtLeft) {
      _isLtLeft = isLtLeft;
      UpdatePosition();
    }

    private void UpdatePosition() {
      var angle = (_total - 1) * _spacing;
      var from = angle / 2f;
      var sign = _isLt == _isLtLeft ? -1 : 1;

      _panel.color = _isLt ? _ltColor : _rtColor;
      _transform.pivot = new Vector2(_isLt == _isLtLeft ? 1 : 0, 0.5f);
      _text.alignment = _isLt == _isLtLeft
        ? TextAlignmentOptions.Right
        : TextAlignmentOptions.Left;
      _transform.anchoredPosition = new Vector2(
        Mathf.Cos(from - _spacing * _index) * _distance * sign,
        Mathf.Sin(from - _spacing * _index) * _distance
      );
    }
  }
}
