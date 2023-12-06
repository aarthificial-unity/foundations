using System;
using Player;
using System.Collections.Generic;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Audio.Events;
using Audio.Parameters;

namespace View.Dialogue {
  public class DialogueTrack : MonoBehaviour {
    private const float _speed = 0.03f;

    public event Action<DialogueEntry, bool> Finished;
    public DialogueScrollViewport Viewport;
    [SerializeField] private DialogueView _view;
    [SerializeField] private DialogueBubble _template;
    [SerializeField] private ScrollRect _container;
    [SerializeField] private FMODEventInstance _textScrollSound;
    [SerializeField] private FMODParameter _textCharacterParameter;
    [SerializeField] private FMODParameter _textStyleParameter;

    private readonly Stack<DialogueBubble> _pool = new();
    private readonly Stack<DialogueBubble> _bubbles = new();
    private RectTransform _rectTransform;
    private DialogueEntry _currentEntry;
    private DialogueBubble _currentBubble;
    private float _showTime;
    private float _duration;
    private string _text;

    private void Awake() {
      _rectTransform = GetComponent<RectTransform>();
      _textScrollSound.Setup();
    }

    public void Restart() {
      _textScrollSound.Pause();
      _currentBubble = null;
      while (_bubbles.Count > 0) {
        var bubble = _bubbles.Pop();
        bubble.gameObject.SetActive(false);
        _pool.Push(bubble);
      }
    }

    public void SetDialogue(DialogueEntry entry, PlayerController player) {
      Assert.IsNull(_currentEntry);
      _currentEntry = entry;
      _showTime = Time.time;
      _text = entry.Content;

      if (_currentBubble != null) {
        _currentBubble.Store();
      }
      _currentBubble = BorrowBubble();
      _currentBubble.gameObject.SetActive(true);
      _currentBubble.Setup(_text, entry.Style, player);
      _container.verticalNormalizedPosition = 0;
      _currentBubble.Text.ForceMeshUpdate();
      _duration = Mathf.Max(1, _currentBubble.Text.textInfo.characterCount)
        * _speed;
      _textScrollSound.SetParameter(_textCharacterParameter, player.IsLT ? 0 : 1);
      _textScrollSound.SetParameter(_textStyleParameter, (int)entry.Style);
      _textScrollSound.Play();
    }

    public void Skip() {
      _currentBubble.SetCompletion(1);
      Finish(true);
    }

    public void DrivenUpdate() {
      foreach (var bubble in _bubbles) {
        bubble.DrivenUpdate();
      }

      _rectTransform.anchoredPosition = new Vector2(
        _view.PlayerFrame.center.x,
        0
      );
      _rectTransform.offsetMin = Vector2.zero;
      _rectTransform.offsetMax = Vector2.zero;
      _rectTransform.sizeDelta = new Vector2(
        _rectTransform.sizeDelta.x,
        _view.CanvasSize.y - _view.PlayerFrame.yMin
      );

      if (_currentEntry == null) {
        return;
      }

      var passed = Time.time - _showTime;
      _currentBubble.SetCompletion(passed / _duration);
      if (passed > _duration) {
        Finish(false);
      }
    }

    private void Finish(bool force) {
      Finished?.Invoke(_currentEntry, force);
      _currentEntry = null;
      _textScrollSound.Pause();
    }

    private DialogueBubble BorrowBubble() {
      DialogueBubble bubble;
      if (_pool.Count == 0) {
        bubble = Instantiate(_template, _container.content);
        bubble.DrivenAwake(_view);
      } else {
        bubble = _pool.Pop();
      }

      bubble.transform.SetSiblingIndex(0);
      _bubbles.Push(bubble);
      return bubble;
    }
  }
}
