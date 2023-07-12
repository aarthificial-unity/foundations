using System;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Player;
using UnityEngine;
using Utils;

namespace Interactions {
  public class Interactable : MonoBehaviour {
    [NonSerialized] public bool IsInteracting;
    [NonSerialized] public bool IsFocused;
    [NonSerialized] public bool IsHovered;
    [NonSerialized] public PlayerType PlayerType;
    [NonSerialized] public bool HasDialogue;
    [NonSerialized] public EntryReference Initiator;
    [NonSerialized] public EntryReference Listener;

    [Inject] [SerializeField] protected PlayerChannel Players;
    public TypewriterEvent Event;

    public event Action StateChanged;
    public Blackboard Blackboard = new();
    [SerializeField] protected InteractionContext Context;

    private void Awake() {
      Context.Interaction = Blackboard;
      Context.Setup();
      Blackboard.Set(InteractionContext.PickUp, 1);
      Blackboard.Set(InteractionContext.IsLTPresent, 0);
      Blackboard.Set(InteractionContext.IsRTPresent, 0);
    }

    private void Start() {
      OnStateChanged();
    }

    public virtual void Interact(PlayerController player) { }

    public virtual void OnInteractionEnter() { }

    public virtual void OnInteractionExit() { }

    public void OnHoverEnter() {
      IsHovered = true;
      OnStateChanged();
    }

    public void OnHoverExit() {
      IsHovered = false;
      OnStateChanged();
    }

    protected void OnStateChanged() {
      StateChanged?.Invoke();
    }
  }
}
