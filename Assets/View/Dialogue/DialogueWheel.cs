﻿using System;
using System.Collections.Generic;
using Typewriter;
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
    private SpringTween _animationTween;

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
      _animationTween.Update(SpringConfig.Snappy);
      var t = _animationTween.X;
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

      Button.DrivenUpdate();
    }

    public void Restart() {
      for (var i = 0; i < _options.Length; i++) {
        _options[i].gameObject.SetActive(false);
      }
    }

    public void SetOptions(List<DialogueEntry> options) {
      if (options.Count == 0) {
        _animationTween.Set(0);
        return;
      }

      _animationTween.ForceSet(0);
      _animationTween.Set(1);
      _count = options.Count;
      var rtCount = 0;
      var ltCount = 0;
      for (var i = 0; i < options.Count; i++) {
        if (options[i].IsLT) {
          ltCount++;
        } else {
          rtCount++;
        }
      }

      var rtIndex = 0;
      var ltIndex = 0;
      for (var i = 0; i < options.Count; i++) {
        var option = options[i];
        _options[i].gameObject.SetActive(true);
        _options[i]
          .SetOption(
            option,
            option.IsLT ? ltIndex++ : rtIndex++,
            option.IsLT ? ltCount : rtCount
          );
      }

      DrivenUpdate();
      for (var i = options.Count; i < _options.Length; i++) {
        _options[i].gameObject.SetActive(false);
      }
    }

    public void OnPointerClick(PointerEventData eventData) {
      Clicked?.Invoke();
    }
  }
}
