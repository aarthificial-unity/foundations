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
using View.Overlay;
using Debug = UnityEngine.Debug;

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
    [Inject] [SerializeField] private OverlayChannel _overlay;
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
    public FMODParameter StepSpeedParam;
    public FMODParameter StepSurfaceParam;
    public FMODParameter StepCharacterParam;
    public FMODParameter StepFocusParam;

    private PlayerState _currentState;
    private PlayerAnimator _animator;

    private void Awake() {
      if (FootstepAudio != null) {
        FootstepAudio.Setup();
        FootstepAudio.AttachToGameObject(this.gameObject);
        FootstepAudio.SetParameter(StepCharacterParam, IsLT ? 0 : 1);
        SetFocus(0);
      }

      _animator = GetComponentInChildren<PlayerAnimator>();
      Agent = GetComponent<NavMeshAgent>();
      FollowState = GetComponent<FollowState>();
      IdleState = GetComponent<IdleState>();
      InteractState = GetComponent<InteractState>();
      NavigateState = GetComponent<NavigateState>();

      Agent.updateRotation = false;
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
        FootstepAudio.SetParameter(StepSurfaceParam, surface.GetSurface(hit));
      }
      FootstepAudio.Play();
    }

    private void Start() {
      Slot = _overlay.HUD.ItemSlots[Type];
      Slot.Dropped += HandleDropped;
      SwitchState(IdleState);
    }

    private void HandleDropped() {
      DropItem();
    }

    public void DrivenUpdate() {
      _currentState.OnUpdate();
      transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        _currentState.Rotation,
        Config.RotationSpeed
        * Time.deltaTime
        * Agent.velocity.magnitude.ClampRemap(0, Agent.speed, 0.5f, 1)
      );

      var speed = Agent.velocity.magnitude;
      if (Agent.remainingDistance > Agent.stoppingDistance + 1) {
        speed = (Agent.velocity.magnitude + Agent.desiredVelocity.magnitude)
          / 2f;
      }

      var speedFactor = speed / Config.WalkSpeed;
      _animator.Animator.SetFloat(_animatorSpeed, speedFactor);

      if (FootstepAudio.IsInitialized) {
        FootstepAudio.SetParameter(StepSpeedParam, speedFactor);
      }
    }

    public void ResetAgent() {
      Agent.stoppingDistance = 0;
      Agent.acceleration = Config.Acceleration;
      Agent.speed = Config.WalkSpeed;
      Agent.autoBraking = true;
    }

    public void SwitchState(PlayerState state) {
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

    public void SetFocus(int value) {
      FootstepAudio.SetParameter(StepFocusParam, value);
    }
  }
}
