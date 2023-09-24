using Aarthificial.Typewriter;
using System;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Audio;
using FMODUnity;
using Interactions;
using Items;
using Player.States;
using Player.Surfaces;
using Typewriter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Utils;
using View.Overlay;

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
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference ItemFact;
    [SerializeField] private EventReference _stepEvent;
    public bool IsLT => Type == PlayerType.LT;
    public bool IsRT => Type == PlayerType.RT;

    [NonSerialized] public NavMeshAgent Agent;
    [NonSerialized] public FollowState FollowState;
    [NonSerialized] public IdleState IdleState;
    [NonSerialized] public InteractState InteractState;
    [NonSerialized] public NavigateState NavigateState;

    [NonSerialized] public ItemSlot Slot;
    [NonSerialized] public EntryReference CurrentItem;

    [SerializeField] public FMODEventInstance FootstepAudio;
    public FMODParameter StepSpeedParam;
    public FMODParameter StepSurfaceParam;
    public FMODParameter StepCharacterParam;
    public FMODParameter StepFocusParam;

    private PlayerState _currentState;
    private PlayerAnimator _animator;
    private float _speedFactor;

    private void Awake() {
      if (FootstepAudio != null) {
        FootstepAudio.Setup();
        FootstepAudio.AttachToGameObject(gameObject);
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
      TypewriterDatabase.Instance.AddListener(ItemFact, HandleItemUsed);
      TypewriterDatabase.Instance.AddListener(
        InteractionContext.AvailableItem,
        HandleItemPickedUp
      );
      TypewriterDatabase.Instance.AddListener(
        InteractionContext.CallOther,
        HandleCallOther
      );
    }

    private void OnDisable() {
      _animator.Stepped -= HandleStepped;
      TypewriterDatabase.Instance.RemoveListener(ItemFact, HandleItemUsed);
      TypewriterDatabase.Instance.RemoveListener(
        InteractionContext.AvailableItem,
        HandleItemPickedUp
      );
      TypewriterDatabase.Instance.RemoveListener(
        InteractionContext.CallOther,
        HandleCallOther
      );
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
      SwitchState(IdleState);
    }

    public void DrivenUpdate() {
      _currentState.OnUpdate();
      transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        _currentState.Rotation,
        Config.RotationSpeed * Time.deltaTime
      );

      var speed = Agent.velocity.magnitude;
      if (Agent.remainingDistance > Agent.stoppingDistance + 1) {
        speed = (Agent.velocity.magnitude + Agent.desiredVelocity.magnitude)
          / 2f;
      }

      _speedFactor = Mathf.Lerp(
        _speedFactor,
        speed / Config.WalkSpeed,
        Config.Smoothing * Time.deltaTime
      );

      _animator.Animator.SetFloat(_animatorSpeed, _speedFactor);

      if (FootstepAudio.IsInitialized) {
        FootstepAudio.SetParameter(StepSpeedParam, _speedFactor);
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

    private void HandleItemUsed(BaseEntry entry, ITypewriterContext context) {
      if (!CurrentItem.HasValue
        || context is not InteractionContext interactionContext) {
        return;
      }

      interactionContext.Interactable.UseItem(CurrentItem.ID);
      context.Set(ItemFact, 0);
      CurrentItem = default;
      Slot.SetItem(null);
    }

    private void HandleItemPickedUp(
      BaseEntry entry,
      ITypewriterContext context
    ) {
      if (CurrentItem.HasValue
        || context is not InteractionContext interactionContext
        || context.Get(InteractionContext.CurrentSpeaker) != Fact.ID) {
        return;
      }

      CurrentItem = interactionContext.Interactable.PickUpItem();
      if (CurrentItem.HasValue) {
        context.Set(ItemFact, CurrentItem);
        Slot.SetItem(CurrentItem.GetEntry<ItemEntry>());
      }
    }

    private void HandleCallOther(BaseEntry entry, ITypewriterContext context) {
      if (context.Get(InteractionContext.Initiator) == Fact.ID) {
        Other.InteractState.Enter(InteractState.Conversation);
      }
    }

    public void SetFocus(int value) {
      FootstepAudio.SetParameter(StepFocusParam, value);
    }
  }
}
