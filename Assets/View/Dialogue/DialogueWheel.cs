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

    [SerializeField] private CommandOptionButton _optionTemplate;
    [SerializeField] private RectTransform _optionContainer;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionText;
    [Inject] [SerializeField] private PlayerChannel _players;

    private readonly CommandOptionButton[] _options =
      new CommandOptionButton[16];

    private bool _isLtLeft;

    private void Awake() {
      for (var i = 0; i < _options.Length; i++) {
        var index = i;
        _options[i] = Instantiate(_optionTemplate, _optionContainer);
        _options[i].gameObject.SetActive(false);
        _options[i].Clicked += () => OptionSelected?.Invoke(index);
      }
    }

    private void Update() {
      if (_players.IsReady) {
        UpdateDirection();
      }
    }

    private void UpdateDirection() {
      var ltPosition =
        Camera.main.WorldToScreenPoint(_players.LT.transform.position);
      var rtPosition =
        Camera.main.WorldToScreenPoint(_players.RT.transform.position);
      var isLtLeft = ltPosition.x < rtPosition.x;

      if (isLtLeft != _isLtLeft) {
        _isLtLeft = isLtLeft;
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

      UpdateDirection();
      for (var i = count; i < _options.Length; i++) {
        _options[i].gameObject.SetActive(false);
      }
    }
  }
}
