using System;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Items;
using Player.States;
using Typewriter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

namespace Player {
  [RequireComponent(typeof(FollowState))]
  [RequireComponent(typeof(IdleState))]
  [RequireComponent(typeof(InteractState))]
  [RequireComponent(typeof(NavigateState))]
  public class PlayerController : MonoBehaviour {
    public Material Material;
    [NonSerialized] public PlayerController Other;
    public Vector3 TargetPosition => Agent.pathEndPosition;

    [Inject] public NavMeshAgent Agent;
    [Inject] public PlayerConfig Config;
    public PlayerType Type;
    public Rigidbody ChainTarget;
    public InputActionReference CommandAction;
    [EntryFilter(Type = EntryType.Fact)] public EntryReference Fact;

    [NonSerialized] public FollowState FollowState;
    [NonSerialized] public IdleState IdleState;
    [NonSerialized] public InteractState InteractState;
    [NonSerialized] public NavigateState NavigateState;

    [NonSerialized] public ItemSlot Slot;
    [NonSerialized] public Item CurrentItem;

    private BaseState _currentState;

    private void Awake() {
      FollowState = GetComponent<FollowState>();
      IdleState = GetComponent<IdleState>();
      InteractState = GetComponent<InteractState>();
      NavigateState = GetComponent<NavigateState>();
    }

    private void Start() {
      Slot.Dropped += HandleDropped;
      SwitchState(IdleState);
    }

    private void HandleDropped() {
      DropItem();
    }

    public void DrivenUpdate() {
      _currentState.OnUpdate();
    }

    public void ResetAgent() {
      Agent.stoppingDistance = 0;
      Agent.acceleration = Config.Acceleration;
      Agent.speed = Config.WalkSpeed;
      Agent.autoBraking = true;
    }

    public void SwitchState(BaseState state) {
      if (_currentState == state) {
        return;
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }

    public bool CanPickUpItem() {
      return CurrentItem == null || CurrentItem.CanDrop();
    }

    public void DropItem() {
      Debug.Log("Drop");
      if (CurrentItem == null) {
        return;
      }

      Assert.IsTrue(CurrentItem.CanDrop());

      CurrentItem.transform.parent = null;
      SceneManager.MoveGameObjectToScene(
        CurrentItem.gameObject,
        SceneManager.GetActiveScene()
      );
      CurrentItem.transform.position = transform.position.ToNavMesh();
      CurrentItem.gameObject.SetActive(true);
      CurrentItem = null;
      Slot.SetItem(CurrentItem);
    }

    public void PickUp(Item item) {
      DropItem();
      CurrentItem = item;
      item.gameObject.SetActive(false);
      item.transform.parent = transform;
      Slot.SetItem(CurrentItem);
    }

    public bool HasItem(Item item) {
      if (item == null) {
        return true;
      }

      if (CurrentItem == null) {
        return false;
      }

      return CurrentItem.PrefabReference.AssetGUID
        == item.PrefabReference.AssetGUID;
    }
  }
}
