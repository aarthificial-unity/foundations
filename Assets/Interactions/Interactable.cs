using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Attributes;
using Aarthificial.Safekeeper.Stores;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using System;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Player;
using Saves;
using Typewriter;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interactions {
  public class Interactable : MonoBehaviour, ISaveStore {
    public event Action StateChanged;

    [ObjectLocation] [SerializeField] private SaveLocation _id;
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
    [SerializeField]
    [FormerlySerializedAs("Item")]
    [EntryFilter(BaseType = typeof(ItemEntry), AllowEmpty = true)]
    private EntryReference _initialItem;

    public Blackboard Blackboard = new();
    public InteractionContext Context;
    protected PlayerManager Players;

    protected virtual void Awake() {
      Players = FindObjectOfType<PlayerManager>();
      Context.Interaction = Blackboard;
      Context.Setup(this);
      Context.Set(InteractionContext.IsLTPresent, 0);
      Context.Set(InteractionContext.IsRTPresent, 0);
      Context.Set(InteractionContext.InitialEvent, Event.EventReference);
    }

    private void OnEnable() {
      SaveStoreRegistry.Register(this);
    }

    private void OnDisable() {
      SaveStoreRegistry.Unregister(this);
    }

    public void OnLoad(SaveControllerBase save) {
      Context.Global = ((SaveController)save).GlobalData.Blackboard;
      if (!save.Data.Read(_id, Blackboard) && _initialItem.HasValue) {
        Context.Set(_initialItem, 1);
      }
    }

    public void OnSave(SaveControllerBase save) {
      save.Data.Write(_id, Blackboard);
    }

    protected virtual void Start() {
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

    protected void OnStateChanged() {
      StateChanged?.Invoke();
    }

    public void UseItem(EntryReference item) {
      if (item.HasValue) {
        Blackboard.Set(item, 1);
      }
    }
  }
}
