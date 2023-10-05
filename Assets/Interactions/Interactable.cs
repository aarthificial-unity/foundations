using System;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Player;
using UnityEngine;

namespace Interactions {
  public class Interactable : MonoBehaviour {
    public event Action StateChanged;

    [NonSerialized] public bool IsInteracting;
    [NonSerialized] public bool IsFocused;
    [NonSerialized] public bool IsHovered;
    [NonSerialized] public bool IsDialogue;
    [NonSerialized] public PlayerType PlayerType;
    [NonSerialized] public bool HasDialogue;
    [NonSerialized] public EntryReference Initiator;
    [NonSerialized] public EntryReference Listener;
    public InteractionGizmo Gizmo;
    public TypewriterEvent Event;

    public Blackboard Blackboard = new();
    public InteractionContext Context;
    protected PlayerManager Players;

    protected virtual void Awake() {
      Players = FindObjectOfType<PlayerManager>();
      Context.Interaction = Blackboard;
      Context.Global = Players.GlobalBlackboard;
      Context.Setup(this);
      Blackboard.Set(InteractionContext.IsLTPresent, 0);
      Blackboard.Set(InteractionContext.IsRTPresent, 0);
      Blackboard.Set(InteractionContext.InitialEvent, Event.EventReference);
    }

    private void Start() {
      OnStateChanged();
    }

    public void SetEvent(EntryReference entry) {
      Event.EventReference = entry;
      Blackboard.Set(InteractionContext.InitialEvent, entry);
    }

    public virtual void Interact(PlayerController player) { }

    public void OnHoverEnter() {
      IsHovered = true;
      OnStateChanged();
    }

    public void OnHoverExit() {
      IsHovered = false;
      OnStateChanged();
    }

    public virtual void OnDialogueEnter() {
      IsDialogue = true;
      OnStateChanged();
    }

    public virtual void OnDialogueExit() {
      IsDialogue = false;
      OnStateChanged();
    }

    protected virtual void OnStateChanged() {
      StateChanged?.Invoke();
    }

    public virtual void UseItem(EntryReference item) {
      Blackboard.Set(InteractionContext.AvailableItem, item);
    }

    public virtual EntryReference PickUpItem() {
      Blackboard.Set(InteractionContext.AvailableItem, 0);
      return 0;
    }
  }
}
