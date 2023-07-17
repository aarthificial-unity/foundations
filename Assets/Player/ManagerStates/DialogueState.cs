using System;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.Common;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Interactions;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Utils;
using View;
using View.Dialogue;

namespace Player.ManagerStates {
  [Serializable]
  public class DialogueState : ManagerState {
    private enum SubState {
      Choice,
      Dialogue,
      Proceed,
      Finished,
    }

    [Inject] [SerializeField] private PlayerChannel _players;
    [Inject] [SerializeField] private ViewChannel _view;
    [SerializeField] private Volume _volume;
    [SerializeField] private float _interactionCooldown = 0.5f;
    private CommandOption[] _options = new CommandOption[16];
    private BaseEntry[] _rules = new BaseEntry[16];

    private SubState _subState = SubState.Choice;
    private float _lastUpdateTime;

    private BaseEntry _queuedEntry;
    private BaseEntry _currentEntry;
    private ITypewriterContext _context;
    private bool _isCancellable;

    private void OnEnable() {
      TypewriterDatabase.Instance.AddListener(HandleTypewriterEvent);
    }

    private void OnDisable() {
      TypewriterDatabase.Instance.RemoveListener(HandleTypewriterEvent);
    }

    private void HandleTypewriterEvent(
      BaseEntry entry,
      ITypewriterContext context
    ) {
      if (context.FindMatchingRule((EntryReference)entry, out var match)) {
        _context = context;
        _queuedEntry = match;
      }

      if (!IsActive) {
        Enter();
      }
    }

    public override void OnEnter() {
      base.OnEnter();

      _volume.weight = 1;
      _view.Dialogue.Wheel.OptionSelected += HandleOptionSelected;
      _view.Dialogue.Wheel.Clicked += HandleClicked;
      _view.Dialogue.Track.Finished += HandleFinished;
      _view.Dialogue.SetActive(true);
      _view.Dialogue.Track.Restart();
      _lastUpdateTime = Time.time;
    }

    public override void OnExit() {
      base.OnExit();
      _view.Dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _view.Dialogue.Wheel.Clicked -= HandleClicked;
      _view.Dialogue.Track.Finished -= HandleFinished;
      _view.Dialogue.SetActive(false);
      _volume.weight = 0;
      _currentEntry = null;
      _queuedEntry = null;
    }

    public override void OnUpdate() {
      _players.LT.DrivenUpdate();
      _players.RT.DrivenUpdate();

      if (_currentEntry != null) {
        return;
      }

      if (_queuedEntry == null) {
        var initial = _context.Get(InteractionContext.InitialEvent);
        if (_context.HasMatchingRule(initial)) {
          _context.Invoke(initial);
          return;
        }

        if (_subState != SubState.Proceed) {
          Exit();
        } else {
          _view.Dialogue.Wheel.SetAction("X");
        }

        return;
      }

      if (_subState == SubState.Proceed && _queuedEntry is not ChoiceEntry) {
        return;
      }

      var entry = _queuedEntry;
      _queuedEntry = null;
      ProcessEntry(entry);
    }

    private void ProcessEntry(BaseEntry entry) {
      _currentEntry = entry;
      if (entry is DialogueEntry dialogue) {
        _view.Dialogue.Wheel.SetOptions(_options, 0);
        _view.Dialogue.Wheel.SetAction(">>");
        _view.Dialogue.Track.SetDialogue(dialogue, GetSpeaker(dialogue));
        _subState = SubState.Dialogue;
        _isCancellable = dialogue.IsCancellable;
      } else if (entry is ChoiceEntry choice) {
        var ruleCount = _context.FindMatchingRules(
          (EntryReference)choice,
          _rules
        );
        var count = 0;
        for (var i = 0; i < ruleCount; i++) {
          var rule = _rules[i];
          if (rule is not DialogueEntry response) {
            continue;
          }
          _options[count++] = new CommandOption {
            Text = response.Text.GetLocalizedString(),
            IsRT = response.Speaker == _players.RT.Fact,
          };
        }

        if (count == 0) {
          _currentEntry = null;
          return;
        }

        _view.Dialogue.Wheel.SetOptions(_options, count);
        _view.Dialogue.Wheel.SetAction(choice.IsCancellable ? "X" : "");
        _subState = SubState.Choice;
        _isCancellable = choice.IsCancellable;
        _context.Invoke(choice);
      } else if (entry is EventEntry rule) {
        _currentEntry = null;
        _subState = SubState.Finished;
        _context.Invoke(rule);
      } else {
        _currentEntry = null;
        _subState = SubState.Finished;
      }
    }

    private void HandleFinished(DialogueEntry entry, bool force) {
      Assert.AreEqual(_currentEntry, entry);
      Assert.AreEqual(_subState, SubState.Dialogue);

      if (!force) {
        _lastUpdateTime = Time.time;
      }

      ApplyRule(entry);
      _currentEntry = null;
      _subState = SubState.Proceed;
      _view.Dialogue.Wheel.SetAction(">");
      _context.Invoke(entry);
    }

    private void HandleOptionSelected(int index) {
      Assert.AreEqual(_subState, SubState.Choice);
      ApplyRule(_rules[index]);
      _subState = SubState.Finished;
      _currentEntry = null;
      _context.Invoke(_rules[index]);
    }

    private void HandleClicked() {
      var duration = Time.time - _lastUpdateTime;
      _lastUpdateTime = 0;
      if (duration < _interactionCooldown) {
        return;
      }

      switch (_subState) {
        case SubState.Finished:
          Exit();
          break;
        case SubState.Choice when _isCancellable:
          Exit();
          break;
        case SubState.Dialogue:
          _view.Dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          _subState = SubState.Finished;
          break;
      }
    }

    private void ApplyRule(BaseEntry rule) {
      _context.Apply(rule);
      if (rule is not DialogueEntry dialogue) {
        return;
      }
      _context.Set(InteractionContext.CurrentSpeaker, dialogue.Speaker.ID);

      PlayerController player;
      foreach (var dispatcher in dialogue.OnEnd) {
        if (dispatcher.Reference.ID == InteractionContext.CallOther
          && _players.TryGetPlayer(
            _context.Get(InteractionContext.Initiator),
            out player
          )) {
          player.Other.InteractState.Enter(player.InteractState.Conversation);
        }

        if (dispatcher.Reference.ID == InteractionContext.PickUp
          && _players.TryGetPlayer(
            _context.Get(InteractionContext.CurrentSpeaker),
            out player
          )
          && player.InteractState.Conversation.Item != null
          && player.CanPickUpItem()) {
          _context.Set(InteractionContext.PickUp, 0);
          player.PickUp(player.InteractState.Conversation.Item);
        }
      }
    }

    private bool GetSpeaker(DialogueEntry entry) {
      return _context.Get(entry.Speaker.ID) == InteractionContext.LT;
    }

    public void Enter() {
      Manager.SwitchState(this);
    }

    private void Exit() {
      _subState = SubState.Finished;
      Manager.SwitchState(Manager.ExploreState);
    }
  }
}
