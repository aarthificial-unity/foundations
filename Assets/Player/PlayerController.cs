using System;
using Player.States;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Player {
  public class PlayerController : MonoBehaviour {
    public Material Material;
    [NonSerialized] public PlayerController Other;
    [NonSerialized] public PlayerManager Manager;
    public Vector3 TargetPosition => Agent.pathEndPosition;

    public NavMeshAgent Agent;
    public PlayerConfig Config;
    public PlayerType Type;
    public Rigidbody ChainTarget;
    [SerializeField] private InputActionReference _commandAction;

    [NonSerialized] public FollowState FollowState;
    [NonSerialized] public IdleState IdleState;
    [NonSerialized] public InteractState InteractState;
    [NonSerialized] public NavigateState NavigateState;

    private BaseState _currentState;

    private void Awake() {
      FollowState = new FollowState(this);
      NavigateState = new NavigateState(this);
      InteractState = new InteractState(this);
      IdleState = new IdleState(this);
      SwitchState(IdleState);
    }

    private void OnEnable() {
      Agent.acceleration = Config.Acceleration;
      _commandAction.action.performed += HandleCommand;
    }

    private void OnDisable() {
      _commandAction.action.performed -= HandleCommand;
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

    public bool IsCurrent() {
      return Manager.CurrentPlayer == Type;
    }

    public bool IsActivelyNavigating() {
      return NavigateState.IsActive
        && IsCurrent()
        && _commandAction.action.ReadValue<float>() > 0.5f;
    }

    public void SwitchState(BaseState state) {
      if (_currentState == state) {
        return;
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }

    private void HandleCommand(InputAction.CallbackContext context) {
      Manager.CurrentPlayer = Type;

      if (Manager.CurrentCommand == PlayerManager.Command.Interact) {
        InteractState.Enter(Manager.Interactable);
      } else if (Manager.CurrentCommand == PlayerManager.Command.Move) {
        NavigateState.Enter();
      }
    }
  }
}
