using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Utils.Tweening;

namespace View.Dialogue {
  public class DialogueWheel : MonoBehaviour, IPointerClickHandler {
    public event Action Clicked;

    public event Action<int> OptionSelected;

    public DialogueButton Button;
    [SerializeField] private DialogueView _view;
    [SerializeField] private CommandOptionButton _optionTemplate;
    [SerializeField] private RectTransform _optionContainer;

    private readonly CommandOptionButton[] _options =
      new CommandOptionButton[16];

    private Cached<bool> _isLtLeft;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private int _count;
    private Dynamics _dynamics;

    private void Awake() {
      _rectTransform = GetComponent<RectTransform>();
      _canvasGroup = GetComponent<CanvasGroup>();
      for (var i = 0; i < _options.Length; i++) {
        var index = i;
        _options[i] = Instantiate(_optionTemplate, _optionContainer);
        _options[i].DrivenAwake(_view);
        _options[i].Clicked += () => OptionSelected?.Invoke(index);
      }
    }

    public void DrivenUpdate() {
      var t = _dynamics.Update(in SpringConfig.Snappy).x;
      for (var i = 0; i < _count; i++) {
        _options[i].DrivenUpdate(t);
      }

      _canvasGroup.alpha = t;
      _rectTransform.offsetMin = Vector2.zero;
      _rectTransform.offsetMax = Vector2.zero;
      _rectTransform.sizeDelta = new Vector2(
        _rectTransform.sizeDelta.x,
        _view.PlayerFrame.yMin
      );
    }

    public void Restart() {
      for (var i = 0; i < _options.Length; i++) {
        _options[i].gameObject.SetActive(false);
      }
    }

    public void SetOptions(CommandOption[] options, int count) {
      if (count == 0) {
        _dynamics.Set(0);
        return;
      }

      _dynamics.ForceSet(0);
      _dynamics.Set(1);
      _count = count;
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

    public void OnPointerClick(PointerEventData eventData) {
      Clicked?.Invoke();
    }
  }
}
