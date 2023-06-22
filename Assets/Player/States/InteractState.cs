using Interactions;
using UnityEngine.Assertions;

namespace Player.States {
  public class InteractState : BaseState {
    private Interactable _interactable;
    private bool _reachedInteractable;

    public InteractState(PlayerController playerController) : base(
      playerController
    ) { }

    public override void OnEnter() {
      base.OnEnter();
      Assert.IsNotNull(
        _interactable,
        "Interact state was entered without an interactable"
      );

      if (Other.NavigateState.IsActive
        || Other.InteractState.IsInteractingWith(_interactable)) {
        Other.FollowState.Enter();
      }

      Player.ResetAgent();
      _reachedInteractable = false;
      _interactable.OnFocusEnter(Player);
      Player.Agent.destination = _interactable.GetPosition();
    }

    public override void OnExit() {
      base.OnExit();
      if (_reachedInteractable) {
        _interactable.OnInteractExit();
      }

      _interactable.OnFocusExit(Player);
      _interactable = null;
    }

    public override void OnUpdate() {
      if (!_interactable.CanInteract()
        || !_interactable.IsCompatibleWith(Player)) {
        if (Other.FollowState.IsActive) {
          Player.SwitchState(Player.IdleState);
        } else {
          Player.SwitchState(Player.FollowState);
        }

        return;
      }

      Player.Agent.destination = _interactable.GetPosition();
      var reachedInteractable =
        Player.Agent.remainingDistance < _interactable.GetRadius();

      if (reachedInteractable != _reachedInteractable) {
        _reachedInteractable = reachedInteractable;
        if (_reachedInteractable) {
          _interactable.OnInteractEnter();
        } else {
          _interactable.OnInteractExit();
        }
      }
    }

    public void Enter(Interactable interactable) {
      Assert.AreEqual(IsActive, _interactable != null);

      if (_interactable == interactable
        || !interactable.IsCompatibleWith(Player)) {
        return;
      }

      Player.SwitchState(null);
      _interactable = interactable;
      Player.SwitchState(this);
    }

    public bool IsInteractingWith(Interactable interactable) {
      return IsActive && _interactable == interactable;
    }
  }
}
