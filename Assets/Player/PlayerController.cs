using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Attributes;
using Aarthificial.Safekeeper.Stores;
using Aarthificial.Typewriter;
using System;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Audio.Events;
using Audio.Parameters;
using Interactions;
using Items;
using Player.States;
using Player.Surfaces;
using Saves;
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
  public class PlayerController : MonoBehaviour, ISaveStore {
    [Serializable]
    private class SerializedTransform {
      public Vector3 Position;
      public Quaternion Rotation;
    }

    private static readonly int _animatorSpeed = Animator.StringToHash("speed");

    public PlayerController Other;
    public Vector3 TargetPosition => Agent.pathEndPosition;

    [Inject] public PlayerConfig Config;
    public PlayerType Type;
    public Rigidbody ChainTarget;
    public InputActionReference CommandAction;
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference Fact;
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference ItemFact;
    [EntryFilter(Variant = EntryVariant.Fact)]
    public EntryReference PresenceFact;
    [ObjectLocation] [SerializeField] private SaveLocation _id;

    public bool IsLT => Type == PlayerType.LT;
    public bool IsRT => Type == PlayerType.RT;

    [NonSerialized] public NavMeshAgent Agent;
    [NonSerialized] public FollowState FollowState;
    [NonSerialized] public IdleState IdleState;
    [NonSerialized] public InteractState InteractState;
    [NonSerialized] public NavigateState NavigateState;

    [NonSerialized] public ItemSlot Slot;
    [NonSerialized] public EntryReference CurrentItem;

    public FMODEventInstance InteractSound;
    public FMODEventInstance EnterDialogueSound;
    public FMODEventInstance FootstepAudio;
    public FMODParameter StepSpeedParam;
    public FMODParameter StepSurfaceParam;
    public FMODParameter StepCharacterParam;
    public FMODParameter StepFocusParam;

    private PlayerState _currentState;
    private PlayerAnimator _animator;
    private float _speedFactor;
    private SerializedTransform _savedTransform = new();

    private void Awake() {
      _animator = GetComponentInChildren<PlayerAnimator>();
      Agent = GetComponent<NavMeshAgent>();
      FollowState = GetComponent<FollowState>();
      IdleState = GetComponent<IdleState>();
      InteractState = GetComponent<InteractState>();
      NavigateState = GetComponent<NavigateState>();
      Slot = FindObjectOfType<HUDView>().ItemSlots[Type];

      Agent.updateRotation = false;

      InteractSound.Setup();
      EnterDialogueSound.Setup();
      FootstepAudio.Setup();
      FootstepAudio.SetParameter(StepCharacterParam, IsLT ? 0 : 1);
      FootstepAudio.AttachToGameObject(gameObject);
      SetFocus(0);
    }

    private void OnDestroy() {
      InteractSound.Release();
      EnterDialogueSound.Release();
      FootstepAudio.Release();
    }

    public void OnLoad(SaveControllerBase save) {
      if (save.Data.Read(_id, _savedTransform)) {
        transform.position = _savedTransform.Position;
        Agent.destination = _savedTransform.Position;
        IdleState.TargetRotation = FollowState.TargetRotation =
          transform.rotation = _savedTransform.Rotation;
      }

      CurrentItem = ((SaveController)save).GlobalData.Blackboard.Get(ItemFact);
      Slot.SetItem(CurrentItem.GetEntry<ItemEntry>());
    }

    public void OnSave(SaveControllerBase save) {
      _savedTransform.Position = transform.position;
      _savedTransform.Rotation = transform.rotation;
      save.Data.Write(_id, _savedTransform);
    }

    private void OnEnable() {
      SaveStoreRegistry.Register(this);
      _animator.Stepped += HandleStepped;
      TypewriterDatabase.Instance.AddListener(ItemFact, HandleItemUsed);
      TypewriterDatabase.Instance.AddListener(HandleItemPickedUp);
      TypewriterDatabase.Instance.AddListener(Facts.CallOther, HandleCallOther);
    }

    private void OnDisable() {
      SaveStoreRegistry.Unregister(this);
      _animator.Stepped -= HandleStepped;
      TypewriterDatabase.Instance.RemoveListener(ItemFact, HandleItemUsed);
      TypewriterDatabase.Instance.RemoveListener(HandleItemPickedUp);
      TypewriterDatabase.Instance.RemoveListener(
        Facts.CallOther,
        HandleCallOther
      );
    }

    private void HandleStepped() {
      if (!FootstepAudio.IsInitialized) {
        return;
      }
      if (Physics.Raycast(
          transform.position + transform.forward * 0.4f,
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

    public void DrivenUpdate() {
      _currentState.OnUpdate();
      transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        _currentState.TargetRotation,
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

      FootstepAudio.SetParameter(StepSpeedParam, _speedFactor);
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
        || context is not InteractionContext
        || entry is not ItemEntry
        || context.Get(Facts.CurrentSpeaker) != Fact.ID
        || context.Get(entry.ID) != 1) {
        return;
      }

      context.Set(entry.ID, 0);
      CurrentItem = entry.ID;
      context.Set(ItemFact, CurrentItem);
      Slot.SetItem(CurrentItem.GetEntry<ItemEntry>());
    }

    private void HandleCallOther(BaseEntry entry, ITypewriterContext context) {
      if (context.Get(PresenceFact) == 0
        && context is InteractionContext interactionContext) {
        InteractState.Enter(interactionContext.Interactable);
      }
    }

    public void SetFocus(int value) {
      FootstepAudio.SetParameter(StepFocusParam, value);
    }
  }
}
