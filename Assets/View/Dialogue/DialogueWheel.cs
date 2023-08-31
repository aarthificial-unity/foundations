using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace View.Dialogue {
  public class DialogueWheel : MonoBehaviour {
    public event UnityAction Clicked {
      add => _actionButton.onClick.AddListener(value);
      remove => _actionButton.onClick.RemoveListener(value);
    }

    public event Action<int> OptionSelected;

    [SerializeField] private DialogueView _view;
    [SerializeField] private CommandOptionButton _optionTemplate;
    [SerializeField] private RectTransform _optionContainer;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionText;

    private readonly CommandOptionButton[] _options =
      new CommandOptionButton[16];

    private Cached<bool> _isLtLeft;
    private RectTransform _rectTransform;

    private void Awake() {
      for (var i = 0; i < _options.Length; i++) {
        var index = i;
        _options[i] = Instantiate(_optionTemplate, _optionContainer);
        _options[i].gameObject.SetActive(false);
        _options[i].Clicked += () => OptionSelected?.Invoke(index);
      }
      _rectTransform = GetComponent<RectTransform>();
    }

    public void DrivenUpdate() {
      if (_isLtLeft.HasChanged(
          _view.ScreenPosition.LT.x < _view.ScreenPosition.RT.x
        )) {
        foreach (var option in _options) {
          option.SetLTLeft(_isLtLeft);
        }
      }
    }

    public void SetAction(string text) {
      _actionText.text = text;
    }

    public void SetOptions(CommandOption[] options, int count) {
      var rtCount = 0;
      var ltCount = 0;
      for (var i = 0; i < count; i++) {
        if (options[i].IsRT) {
          rtCount++;
        } else {
          ltCount++;
        }
      }

      var rtIndex = 0;
      var ltIndex = 0;
      for (var i = 0; i < count; i++) {
        var option = options[i];
        _options[i].gameObject.SetActive(true);
        _options[i]
          .SetOption(
            option,
            option.IsRT ? rtIndex++ : ltIndex++,
            option.IsRT ? rtCount : ltCount
          );
      }

      DrivenUpdate();
      for (var i = count; i < _options.Length; i++) {
        _options[i].gameObject.SetActive(false);
      }
    }
  }
}
