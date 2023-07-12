using System;
using Aarthificial.Typewriter.Blackboards;
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
    [SerializeField] private InteractionContext _context;
    [SerializeField] private float _interactionCooldown = 0.5f;
    [NonSerialized] public Interactable Interactable;
    private CommandOption[] _options = new CommandOption[16];
    private DialogueEntry[] _rules = new DialogueEntry[16];
    private SubState _subState = SubState.Choice;
    private DialogueEntry _currentEntry;
    private float _lastUpdateTime;

    public override void OnEnter() {
      base.OnEnter();

      Interactable.OnInteractionEnter();
      _volume.weight = 1;
      _context.Setup();
      _context.Interaction = Interactable.Blackboard;

      _view.Dialogue.Wheel.OptionSelected += HandleOptionSelected;
      _view.Dialogue.Wheel.Clicked += HandleClicked;
      _view.Dialogue.Track.Finished += HandleFinished;
      _view.Dialogue.SetActive(true);
      _view.Dialogue.Track.Restart();
      ShowChoice(Interactable.Event.eventReference);
      _lastUpdateTime = Time.time;
    }

    public override void OnExit() {
      base.OnExit();
      _view.Dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _view.Dialogue.Wheel.Clicked -= HandleClicked;
      _view.Dialogue.Track.Finished -= HandleFinished;
      _view.Dialogue.SetActive(false);
      _volume.weight = 0;
      Interactable.OnInteractionExit();
      Interactable = null;
    }

    public override void OnUpdate() {
      _players.LT.DrivenUpdate();
      _players.RT.DrivenUpdate();
    }

    private void HandleFinished(DialogueEntry entry, bool force) {
      Assert.AreEqual(_currentEntry, entry);

      if (!force) {
        _lastUpdateTime = Time.time;
      }

      ApplyRule(_currentEntry);

      if (!_context.HasMatchingRule(_currentEntry.id)) {
        if (_context.FindMatchingRules(
            Interactable.Event.eventReference,
            _rules
          )
          > 0) {
          ShowInitialChoice();
          return;
        }

        _subState = SubState.Finished;
        _view.Dialogue.Wheel.SetAction("X");
        return;
      }

      if (_currentEntry.choice) {
        ProcessRule(_currentEntry);
      } else {
        _view.Dialogue.Wheel.SetAction(">");
        _subState = SubState.Proceed;
      }
    }

    private void HandleClicked() {
      var duration = Time.time - _lastUpdateTime;
      _lastUpdateTime = 0;
      if (duration < _interactionCooldown) {
        return;
      }

      switch (_subState) {
        case SubState.Choice:
        case SubState.Finished:
          Exit();
          break;
        case SubState.Dialogue:
          _view.Dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          ProcessRule(_currentEntry);
          break;
      }
    }

    private void HandleOptionSelected(int index) {
      ApplyRule(_rules[index]);
      ProcessRule(_rules[index]);
    }

    private void ApplyRule(DialogueEntry rule) {
      _context.Apply(rule);
      _context.SetSpeaker(rule.Speaker.id);
      var player = GetSpeaker(rule) ? _players.LT : _players.RT;

      foreach (var dispatcher in rule.onEnd) {
        if (dispatcher.reference.id == InteractionContext.CallOther) {
          player.Other.InteractState.Enter(player.InteractState.Conversation);
          // } else if (dispatcher.reference.id == InteractionContext.PickUp
          // && Interactable.Item != null
          // && player.CanPickUpItem()) {
          // _context.Interaction.Set(InteractionContext.PickUp, 0);
          // player.PickUp(Interactable.Item);
        }
      }
    }

    private void ProcessRule(DialogueEntry rule) {
      if (rule.choice) {
        ShowChoice(rule.Next);
      } else {
        ShowDialogue(rule.Next);
      }
    }

    private void ShowDialogue(EntryReference reference) {
      _view.Dialogue.Wheel.SetOptions(_options, 0);
      if (_context.FindMatchingRule(reference, out var entry)
        && entry is DialogueEntry dialogue) {
        _currentEntry = dialogue;
        _subState = SubState.Dialogue;
        _view.Dialogue.Wheel.SetOptions(_options, 0);
        _view.Dialogue.Wheel.SetAction(">>");
        _view.Dialogue.Track.SetDialogue(
          _currentEntry,
          GetSpeaker(_currentEntry)
        );
      } else {
        ShowInitialChoice();
      }
    }

    private bool GetSpeaker(DialogueEntry entry) {
      return _context.Context.Get(entry.Speaker.id) == InteractionContext.LT;
    }

    private void ShowInitialChoice() {
      _currentEntry = null;
      _subState = SubState.Choice;
      var ruleCount = _context.FindMatchingRules(
        Interactable.Event.eventReference,
        _rules
      );
      if (ruleCount == 0) {
        Exit();
        return;
      }

      for (var i = 0; i < ruleCount; i++) {
        var rule = _rules[i];
        _options[i] = new CommandOption {
          Text = rule.Text.GetLocalizedString(),
          IsRT = rule.Speaker == _players.RT.Fact,
        };
      }

      _view.Dialogue.Wheel.SetOptions(_options, ruleCount);
      _view.Dialogue.Wheel.SetAction("X");
    }

    private void ShowChoice(EntryReference reference) {
      _currentEntry = null;
      _subState = SubState.Choice;
      var ruleCount = _context.FindMatchingRules(reference, _rules);
      if (ruleCount == 0) {
        ShowInitialChoice();
        return;
      }

      for (var i = 0; i < ruleCount; i++) {
        var rule = _rules[i];
        _options[i] = new CommandOption {
          Text = rule.Text.GetLocalizedString(),
          IsRT = rule.Speaker == _players.RT.Fact,
        };
      }

      _view.Dialogue.Wheel.SetOptions(_options, ruleCount);
      _view.Dialogue.Wheel.SetAction("X");
    }

    public void Enter(Interactable interactable) {
      Assert.AreEqual(IsActive, Interactable != null);

      if (Interactable == interactable || !interactable.HasDialogue) {
        return;
      }

      Manager.SwitchState(null);
      Interactable = interactable;
      Manager.SwitchState(this);
    }

    private void Exit() {
      _subState = SubState.Finished;
      Manager.SwitchState(Manager.ExploreState);
    }
  }
}
