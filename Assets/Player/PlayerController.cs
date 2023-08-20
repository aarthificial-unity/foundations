using System;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Audio;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Items;
using Player.States;
using Player.Surfaces;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utils;
using View;

namespace Player {
  [RequireComponent(typeof(FollowState))]
  [RequireComponent(typeof(IdleState))]
  [RequireComponent(typeof(InteractState))]
  [RequireComponent(typeof(NavigateState))]
  public class PlayerController : MonoBehaviour {
    private static readonly int _animatorSpeed = Animator.StringToHash("speed");

    public Material Material;
    [NonSerialized] public PlayerController Other;
    public Vector3 TargetPosition => Agent.pathEndPosition;

    [Inject] public PlayerConfig Config;
    [Inject] [SerializeField] private ViewChannel _view;
    public PlayerType Type;
    public Rigidbody ChainTarget;
    public InputActionReference CommandAction;
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference Fact;
    [SerializeField] private EventReference _stepEvent;
    public bool IsLT => Type == PlayerType.LT;
    public bool IsRT => Type == PlayerType.RT;

    [NonSerialized] public NavMeshAgent Agent;
    [NonSerialized] public FollowState FollowState;
    [NonSerialized] public IdleState IdleState;
    [NonSerialized] public InteractState InteractState;
    [NonSerialized] public NavigateState NavigateState;

    [NonSerialized] public ItemSlot Slot;
    [NonSerialized] public Item CurrentItem;

    [SerializeField] public FMODEventInstance FootstepAudio;
    private const string _stepSpeedParam = "speed";
    private const string _stepSurfaceParam = "surface";
    private const string _stepCharacterParam = "character";

    private BaseState _currentState;
    private PlayerAnimator _animator;


    private void Awake() {
      if (FootstepAudio != null) {
        FootstepAudio.Setup();
        FootstepAudio.AttachToTransform(this.transform);
        //FootstepAudio.SetParameter(_stepCharacterParam, IsLT ? 0 : 1);
      }

      _animator = GetComponentInChildren<PlayerAnimator>();
      Agent = GetComponent<NavMeshAgent>();
      FollowState = GetComponent<FollowState>();
      IdleState = GetComponent<IdleState>();
      InteractState = GetComponent<InteractState>();
      NavigateState = GetComponent<NavigateState>();
    }

    private void OnDestroy() {
      FootstepAudio.Release();
    }

    private void OnEnable() {
      _animator.Stepped += HandleStepped;
    }

    private void OnDisable() {
      _animator.Stepped -= HandleStepped;
    }

    private void HandleStepped() {
      if (!FootstepAudio.IsInitialized) {
        return;
      }
      if (Physics.Raycast(
          transform.position,
          Vector3.down,
          out var hit,
          2f,
          Config.GroundMask
        )
        && hit.collider.TryGetComponent<ISurfaceProvider>(out var surface)) {
        //FootstepAudio.SetParameter(_stepSurfaceParam, surface.GetSurface(hit));
      }
      FootstepAudio.Play();
    }

    private void Start() {
      Slot = _view.HUD.ItemSlots[Type];
      Slot.Dropped += HandleDropped;
      SwitchState(IdleState);
    }

    private void HandleDropped() {
      DropItem();
    }

    public void DrivenUpdate() {
      _currentState.OnUpdate();
      var speed = Agent.velocity.magnitude / Config.WalkSpeed;
      _animator.Animator.SetFloat(_animatorSpeed, speed);

      if (FootstepAudio.IsInitialized) {
        //FootstepAudio.SetParameter(_stepSpeedParam, speed);
      }
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
      UnityEngine.Debug.Log("Drop");
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
