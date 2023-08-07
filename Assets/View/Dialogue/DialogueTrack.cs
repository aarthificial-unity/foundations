using System;
using Player;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace View.Dialogue {
  public class DialogueTrack : MonoBehaviour {
    private const float _speed = 0.05f;

    public event Action<DialogueEntry, bool> Finished;
    [SerializeField] private DialogueBubble _ltBubble;
    [SerializeField] private DialogueBubble _rtBubble;
    [Inject] [SerializeField] private PlayerChannel _players;

    private DialogueEntry _currentEntry;
    private DialogueBubble _currentBubble;
    private float _showTime;
    private float _duration;
    private string _text;

    public void Restart() {
      _ltBubble.gameObject.SetActive(false);
      _rtBubble.gameObject.SetActive(false);
    }

    public void SetDialogue(DialogueEntry entry, bool isLt) {
      Assert.IsNull(_currentEntry);
      _currentEntry = entry;
      _showTime = Time.time;
      if (_currentBubble != null) {
        _currentBubble.gameObject.SetActive(false);
      }

      _currentBubble = isLt ? _ltBubble : _rtBubble;

      _text = entry.Text.GetLocalizedString();
      _currentBubble.gameObject.SetActive(true);
      _currentBubble.SetText(_text);
      _duration = _text.Length * _speed;
    }

    public void Skip() {
      _currentBubble.SetCompletion(1);
      Finish(true);
    }

    private void Update() {
      if (_players.IsReady) {
        var ltPosition =
          Camera.main.WorldToScreenPoint(_players.LT.transform.position);
        var rtPosition =
          Camera.main.WorldToScreenPoint(_players.RT.transform.position);
        var isLtLeft = ltPosition.x < rtPosition.x;

        _ltBubble.UpdatePosition(ltPosition, isLtLeft);
        _rtBubble.UpdatePosition(rtPosition, !isLtLeft);
      }

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
    }
  }
}
