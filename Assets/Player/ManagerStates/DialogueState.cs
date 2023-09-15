using System;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Input;
using Interactions;
using System.Collections.Generic;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Utils;
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
    [Inject] [SerializeField] private InputChannel _input;
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] private Volume _volume;
    [SerializeField] private float _interactionCooldown = 0.5f;
    private List<DialogueEntry> _options = new();
    private BaseEntry[] _rules = new BaseEntry[16];

    private SubState _subState = SubState.Choice;
    private float _lastUpdateTime;

    private BaseEntry _queuedEntry;
    private BaseEntry _currentEntry;
    private InteractionContext _context;
    private bool _isCancellable;

    protected override void Awake() {
      base.Awake();
      Assert.IsNull(_context, "How?");
    }

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
      var newContext = context as InteractionContext;
      Assert.IsNotNull(newContext);

      if (_context != newContext) {
        _overlay.Dialogue.SetContext(newContext);
        _context?.Interactable.OnDialogueExit();
        _context = newContext;
        _context.Interactable.OnDialogueEnter();
      }

      _queuedEntry = entry;

      Debug.Log($"New rule occurred: {entry.Key}");
      if (!IsActive) {
        Debug.Log($"Starting conversation: {entry.Key}");
        Enter();
      }
    }

    public override void OnEnter() {
      base.OnEnter();

      _volume.weight = 1;
      _overlay.Dialogue.Wheel.OptionSelected += HandleOptionSelected;
      _overlay.Dialogue.Wheel.Button.Clicked += HandleButtonClicked;
      _overlay.Dialogue.Wheel.Clicked += HandleBackdropClicked;
      _overlay.Dialogue.Track.Clicked += HandleBackdropClicked;
      _overlay.Dialogue.Track.Finished += HandleFinished;
      _overlay.Dialogue.SetActive(true);
      _overlay.Dialogue.Track.Restart();
      _overlay.Dialogue.Wheel.Restart();
      _lastUpdateTime = Time.time;
    }

    public override void OnExit() {
      base.OnExit();
      _overlay.Dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _overlay.Dialogue.Wheel.Button.Clicked -= HandleButtonClicked;
      _overlay.Dialogue.Wheel.Clicked -= HandleBackdropClicked;
      _overlay.Dialogue.Track.Clicked -= HandleBackdropClicked;
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
          _overlay.Dialogue.Wheel.Button.SetAction(
            DialogueButton.ActionType.Cancel
          );
        }

        return;
      }

      if (_subState == SubState.Proceed && _queuedEntry is DialogueEntry) {
        return;
      }

      var entry = _queuedEntry;
      _queuedEntry = null;
      ProcessEntry(entry);
    }

    private void ProcessEntry(BaseEntry entry) {
      _currentEntry = entry;
      Debug.Log($"Processing: {entry.Key}");
      if (entry is DialogueEntry dialogue) {
        var speaker = _context.Get(dialogue.Speaker.ID);
        _players.TryGetPlayer(speaker, out var player);
        Assert.IsNotNull(player, $"Missing speaker: {speaker}");

        _options.Clear();
        _overlay.Dialogue.Wheel.SetOptions(_options);
        _overlay.Dialogue.Wheel.Button.SetAction(
          DialogueButton.ActionType.Skip
        );
        _overlay.Dialogue.Track.SetDialogue(dialogue, player);
        _subState = SubState.Dialogue;
      } else if (entry is ChoiceEntry choice) {
        var ruleCount = _context.FindMatchingRules(
          (EntryReference)choice,
          _rules
        );
        _options.Clear();
        for (var i = 0; i < ruleCount; i++) {
          var rule = _rules[i];
          if (rule is DialogueEntry response) {
            _options.Add(response);
          }
        }

        if (_options.Count == 0) {
          _currentEntry = null;
          return;
        }

        _overlay.Dialogue.Wheel.SetOptions(_options);
        _overlay.Dialogue.Wheel.Button.SetAction(
          choice.IsCancellable
            ? DialogueButton.ActionType.Cancel
            : DialogueButton.ActionType.None
        );
        _subState = SubState.Choice;
        _isCancellable = choice.IsCancellable;
        choice.Apply(_context);
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
      _overlay.Dialogue.Wheel.Button.SetAction(DialogueButton.ActionType.Play);
      _context.Process(entry);
    }

    private void HandleOptionSelected(int index) {
      Assert.AreEqual(_subState, SubState.Choice);
      ApplyRule(_rules[index]);
      _subState = SubState.Finished;
      _currentEntry = null;
      Debug.Log($"Selected option: {_rules[index].Key}");
      _context.Process(_rules[index]);
    }

    private void HandleButtonClicked() {
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

    private void HandleBackdropClicked() {
      var duration = Time.time - _lastUpdateTime;
      _lastUpdateTime = 0;
      if (duration < _interactionCooldown) {
        return;
      }

      switch (_subState) {
        case SubState.Dialogue:
          _overlay.Dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          _subState = SubState.Finished;
          break;
      }
    }

    private void ApplyRule(BaseEntry rule) {
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

    public void Enter() {
      Manager.SwitchState(this);
    }

    private void Exit() {
      _subState = SubState.Finished;
      _context.Interactable.OnDialogueExit();
      _context = null;
      _overlay.Dialogue.SetContext(null);
      Manager.SwitchState(Manager.ExploreState);
    }
  }
}
