using System;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Interactions;
using System.Collections.Generic;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
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

    [NonSerialized] public BaseEntry CurrentEntry;
    [NonSerialized] public InteractionContext Context;

    [SerializeField] private Volume _volume;
    [SerializeField] private float _interactionCooldown = 0.5f;
    private List<DialogueEntry> _options = new();
    private BaseEntry[] _rules = new BaseEntry[16];

    private SubState _subState = SubState.Choice;
    private float _lastUpdateTime;

    private BaseEntry _queuedEntry;
    private bool _isCancellable;
    private DialogueView _dialogue;

    protected override void Awake() {
      base.Awake();
      _dialogue = FindObjectOfType<DialogueView>();
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

      if (Context != newContext) {
        _dialogue.SetContext(newContext);
        Context?.Interactable.OnDialogueExit();
        Context = newContext;
        Context.Interactable.OnDialogueEnter();
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
      _dialogue.Wheel.OptionSelected += HandleOptionSelected;
      _dialogue.Wheel.Button.Clicked += HandleButtonClicked;
      _dialogue.Wheel.Clicked += HandleBackdropClicked;
      _dialogue.Track.Clicked += HandleBackdropClicked;
      _dialogue.Track.Finished += HandleFinished;
      _dialogue.SetActive(true);
      _dialogue.Track.Restart();
      _dialogue.Wheel.Restart();
      _lastUpdateTime = Time.time;
    }

    public override void OnExit() {
      base.OnExit();
      _dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _dialogue.Wheel.Button.Clicked -= HandleButtonClicked;
      _dialogue.Wheel.Clicked -= HandleBackdropClicked;
      _dialogue.Track.Clicked -= HandleBackdropClicked;
      _dialogue.Track.Finished -= HandleFinished;
      _dialogue.SetActive(false);
      _volume.weight = 0;
      CurrentEntry = null;
      _queuedEntry = null;
    }

    public override void OnUpdate() {
      Manager.LT.DrivenUpdate();
      Manager.RT.DrivenUpdate();

      if (CurrentEntry != null) {
        return;
      }

      if (_queuedEntry == null) {
        var initial = Context.Get(InteractionContext.InitialEvent);
        if (Context.Process(initial)) {
          return;
        }

        if (_subState != SubState.Proceed) {
          Exit();
        } else {
          _dialogue.Wheel.Button.SetAction(DialogueButton.ActionType.Cancel);
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
      CurrentEntry = entry;
      Debug.Log($"Processing: {entry.Key}");
      if (entry is DialogueEntry dialogue) {
        var speaker = Context.Get(dialogue.Speaker.ID);
        Manager.TryGetPlayer(speaker, out var player);
        Assert.IsNotNull(player, $"Missing speaker: {speaker}");

        _options.Clear();
        _dialogue.Wheel.SetOptions(_options);
        _dialogue.Wheel.Button.SetAction(DialogueButton.ActionType.Skip);
        _dialogue.Track.SetDialogue(dialogue, player);
        _subState = SubState.Dialogue;
      } else if (entry is ChoiceEntry choice) {
        var ruleCount = Context.FindMatchingRules(
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
          CurrentEntry = null;
          return;
        }

        _dialogue.Wheel.SetOptions(_options);
        _dialogue.Wheel.Button.SetAction(
          choice.IsCancellable
            ? DialogueButton.ActionType.Cancel
            : DialogueButton.ActionType.None
        );
        _subState = SubState.Choice;
        _isCancellable = choice.IsCancellable;
        choice.Apply(Context);
      } else if (entry is EventEntry rule) {
        CurrentEntry = null;
        _subState = SubState.Finished;
        Context.Process(rule);
      } else {
        CurrentEntry = null;
        _subState = SubState.Finished;
      }
    }

    private void HandleFinished(DialogueEntry entry, bool force) {
      Assert.AreEqual(CurrentEntry, entry);
      Assert.AreEqual(_subState, SubState.Dialogue);

      if (!force) {
        _lastUpdateTime = Time.time;
      }

      CurrentEntry = null;
      _subState = SubState.Proceed;
      _dialogue.Wheel.Button.SetAction(DialogueButton.ActionType.Play);
      Context.Process(entry);
    }

    private void HandleOptionSelected(int index) {
      Assert.AreEqual(_subState, SubState.Choice);
      _subState = SubState.Finished;
      CurrentEntry = null;
      Debug.Log($"Selected option: {_rules[index].Key}");
      Context.Process(_rules[index]);
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
          _dialogue.Track.Skip();
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
          _dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          _subState = SubState.Finished;
          break;
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }

    private void Exit() {
      _subState = SubState.Finished;
      Context.Interactable.OnDialogueExit();
      Context = null;
      _dialogue.SetContext(null);
      Manager.SwitchState(Manager.ExploreState);
    }
  }
}
