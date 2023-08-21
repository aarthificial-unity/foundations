using System;
using Aarthificial.Typewriter;
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
using View.Overlay;

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
    [Inject] [SerializeField] private OverlayChannel _overlay;
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
      // if (context.FindMatchingRule((EntryReference)entry, out var match)) {
      _context = context;
      _queuedEntry = entry;
      // }

      if (!IsActive) {
        Enter();
      }
    }

    public override void OnEnter() {
      base.OnEnter();

      _volume.weight = 1;
      _overlay.Dialogue.Wheel.OptionSelected += HandleOptionSelected;
      _overlay.Dialogue.Wheel.Clicked += HandleClicked;
      _overlay.Dialogue.Track.Finished += HandleFinished;
      _overlay.Dialogue.SetActive(true);
      _overlay.Dialogue.Track.Restart();
      _lastUpdateTime = Time.time;
    }

    public override void OnExit() {
      base.OnExit();
      _overlay.Dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _overlay.Dialogue.Wheel.Clicked -= HandleClicked;
      _overlay.Dialogue.Track.Finished -= HandleFinished;
      _overlay.Dialogue.SetActive(false);
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
        if (_context.Process(initial)) {
          return;
        }

        if (_subState != SubState.Proceed) {
          Exit();
        } else {
          _overlay.Dialogue.Wheel.SetAction("X");
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
        _overlay.Dialogue.Wheel.SetOptions(_options, 0);
        _overlay.Dialogue.Wheel.SetAction(">>");
        _overlay.Dialogue.Track.SetDialogue(dialogue, GetSpeaker(dialogue));
        _subState = SubState.Dialogue;
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

        _overlay.Dialogue.Wheel.SetOptions(_options, count);
        _overlay.Dialogue.Wheel.SetAction(choice.IsCancellable ? "X" : "");
        _subState = SubState.Choice;
        _isCancellable = choice.IsCancellable;
        _context.Process(choice);
      } else if (entry is EventEntry rule) {
        _currentEntry = null;
        _subState = SubState.Finished;
        _context.Process(rule);
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
      _overlay.Dialogue.Wheel.SetAction(">");
      _context.Process(entry);
    }

    private void HandleOptionSelected(int index) {
      Assert.AreEqual(_subState, SubState.Choice);
      ApplyRule(_rules[index]);
      _subState = SubState.Finished;
      _currentEntry = null;
      _context.Process(_rules[index]);
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
          _overlay.Dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          _subState = SubState.Finished;
          break;
      }
    }

    private void ApplyRule(BaseEntry rule) {
      // _context.Apply(rule);
      if (rule is not DialogueEntry dialogue) {
        return;
      }
      _context.Set(InteractionContext.CurrentSpeaker, dialogue.Speaker.ID);

      PlayerController player;
      foreach (var dispatcher in dialogue.OnApply) {
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
