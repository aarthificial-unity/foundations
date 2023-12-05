using TMPro;
using Typewriter;
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

    [Inject] [SerializeField] private ButtonStyle _style;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _icon;
    [SerializeField] private BoxSDF _fill;
    [SerializeField] private BoxSDF _stroke;
    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _transform;

    private const char _eyeIcon = '\ue8f4';
    private const char _itemIcon = '\ue574';
    private const char _exitIcon = '\ue879';

    private DialogueView _view;
    private ButtonStyle.Settings _settings;
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
      _isLtLeft = _view.IsLTLeft;
      var spacing = _spacing;
      var distance = _distance;
      var angle = (_total - 1) * spacing;
      var from = angle / 2f;
      var sign = _isLt == _isLtLeft ? -1 : 1;

      _transform.pivot = new Vector2(_isLt == _isLtLeft ? 1 : 0, 0.5f);
      UpdateText(_text, _isLt == _isLtLeft);
      UpdateText(_icon, _isLt != _isLtLeft);
      _text.alignment = _isLt == _isLtLeft
        ? TextAlignmentOptions.Right
        : TextAlignmentOptions.Left;
      _transform.anchoredPosition = new Vector2(
        Mathf.Cos(from - spacing * _index) * distance * sign,
        Mathf.Sin(from - spacing * _index) * distance
      );
      _transform.sizeDelta = new Vector2(_size.x * t, _size.y);
    }

    private void UpdateText(TextMeshProUGUI text, bool onLeft) {
      text.rectTransform.pivot = new Vector2(onLeft ? 1 : 0, 0.5f);
      text.rectTransform.anchorMin = text.rectTransform.anchorMax =
        new Vector2(onLeft ? 1 : 0, 0.5f);
    }

    public void SetOption(DialogueEntry option, int index, int total) {
      _text.text = option.Content;
      _icon.text = option.Icon switch {
        DialogueEntry.BubbleIcon.Eye => _eyeIcon.ToString(),
        DialogueEntry.BubbleIcon.Item => _itemIcon.ToString(),
        DialogueEntry.BubbleIcon.Exit => _exitIcon.ToString(),
        _ => "",
      };
      _index = index;
      _total = total;
      _isLt = option.IsLT;

      var settings = _style[option.Style];
      var backgroundColor = settings.BackgroundColors[option.PlayerType];

      _fill.Color = backgroundColor;
      _fill.TextureStrength = settings.TextureStrength;
      _stroke.Color = backgroundColor;
      _stroke.TextureStrength = settings.TextureStrength;
      _stroke.Dash = settings.Stroke ? 4 : 0;
      _text.color = settings.TextColors[option.PlayerType];
      _icon.color = settings.IconColors[option.PlayerType];
    }
  }
}
