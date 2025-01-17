﻿using System;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Audio.Events;
using Framework;
using Interactions;
using Settings.Bundles;
using System.Collections.Generic;
using Typewriter;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Utils;
using View.Dialogue;
using Audio.Parameters;

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

    [SerializeField] private FMODEventInstance _skipSound;
    [SerializeField] private FMODEventInstance _nextSound;
    [SerializeField] private FMODEventInstance _exitSound;
    [SerializeField] private FMODEventInstance _optionSound;
    [SerializeField] private FMODParameterInstance _inDialogueParam;
    [SerializeField] [Inject] private GameplaySettingsBundle _bundle;

    [SerializeField] private Volume _volume;
    [SerializeField] private float _interactionCooldown = 0.2f;
    [SerializeField] private float _fastForwardCooldown = 0.5f;

    private List<DialogueEntry> _options = new();
    private BaseEntry[] _rules = new BaseEntry[16];

    private SubState _subState = SubState.Choice;
    private float _lastUpdateTime;
    private float _lastSkipTime;

    private BaseEntry _queuedEntry;
    private bool _isCancellable;
    private DialogueView _dialogue;

    protected override void Awake() {
      base.Awake();
      _dialogue = FindObjectOfType<DialogueView>();
      _skipSound.Setup();
      _nextSound.Setup();
      _exitSound.Setup();
      _optionSound.Setup();
      _inDialogueParam.Setup();
    }

    protected override void OnDestroy() {
      base.OnDestroy();
      _skipSound.Release();
      _nextSound.Release();
      _exitSound.Release();
      _optionSound.Release();
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
      _dialogue.Track.Viewport.Clicked += HandleBackdropClicked;
      _dialogue.Track.Finished += HandleFinished;
      _dialogue.SetActive(true);
      _dialogue.Track.Restart();
      _dialogue.Wheel.Restart();
      _lastUpdateTime = Time.time;
      _inDialogueParam.CurrentValue = 1;
    }

    public override void OnExit() {
      base.OnExit();
      _dialogue.Wheel.OptionSelected -= HandleOptionSelected;
      _dialogue.Wheel.Button.Clicked -= HandleButtonClicked;
      _dialogue.Wheel.Clicked -= HandleBackdropClicked;
      _dialogue.Track.Viewport.Clicked -= HandleBackdropClicked;
      _dialogue.Track.Finished -= HandleFinished;
      _dialogue.SetActive(false);
      _volume.weight = 0;
      CurrentEntry = null;
      _queuedEntry = null;
      _inDialogueParam.CurrentValue = 0;
    }

    public override void OnUpdate() {
      UpdatePlayer(Manager.LT);
      UpdatePlayer(Manager.RT);

      UpdateFastForward();

      if (CurrentEntry != null) {
        return;
      }

      if (_queuedEntry == null) {
        var initial = Context.Get(Facts.InitialEvent);
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

    private void UpdateFastForward() {
      if (App.Actions.PointingClick.action.WasPressedThisFrame()) {
        _lastSkipTime = Time.time;
      }

      if (App.Actions.PointingContinue.action.WasPressedThisFrame()) {
        _lastSkipTime = Time.time;
        HandleBackdropClicked();
      }

      var skipPress = _dialogue.Track.Viewport.IsPressed
        && _bundle.SkipDialogue.GetBool();

      if ((App.Actions.PointingContinue.action.IsPressed() || skipPress)
        && Time.time - _lastSkipTime > _fastForwardCooldown) {
        HandleBackdropClicked(false);
      }
    }

    private void UpdatePlayer(PlayerController player) {
      player.FollowState.TightDistance = false;
      player.DrivenUpdate(false);
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
      _optionSound.Play();
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
        case SubState.Choice when _isCancellable:
          _exitSound.Play();
          Exit();
          break;
        case SubState.Dialogue:
          _skipSound.Play();
          _dialogue.Track.Skip();
          break;
        case SubState.Proceed:
          _nextSound.Play();
          _subState = SubState.Finished;
          break;
      }
    }

    private void HandleBackdropClicked() {
      HandleBackdropClicked(true);
    }

    private void HandleBackdropClicked(bool withSound) {
      var duration = Time.time - _lastUpdateTime;
      _lastUpdateTime = 0;
      if (duration < _interactionCooldown) {
        return;
      }

      switch (_subState) {
        case SubState.Dialogue:
          if (withSound) {
            _skipSound.Play();
          }
          _dialogue.Track.Skip();
          break;
        case SubState.Proceed when _queuedEntry != null:
          if (withSound) {
            _nextSound.Play();
          }
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
