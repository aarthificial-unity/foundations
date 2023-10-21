using Audio;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using View.Overlay;

namespace Player.ManagerStates {
  public class ExploreState : ManagerState {
    private enum Command {
      None,
      Move,
      Interact,
    }

    [Inject] [SerializeField] private PlayerConfig _config;
    [SerializeField] private TargetController _targetPrefab;
    [SerializeField] private InputActionReference _targetAction;
    [SerializeField] private FMODEventInstance _interactionHoverSound;
    [SerializeField] private float _keyReleaseDelay = 0.1f;

    private Command _currentCommand = Command.None;
    private Interactable _interactable;
    private Vector3 _targetPosition;
    private PlayerType _currentPlayer = PlayerType.None;
    private PlayerType _commandedPlayer = PlayerType.None;
    private TargetController _target;
    private Camera _mainCamera;
    private HUDView _hud;
    private float _lastBothTime;
    private bool _areBothPressed;

    private PlayerController CurrentController => Manager[_currentPlayer];

    public PlayerType CurrentPlayer {
      get => _currentPlayer;
      set {
        if (CurrentController != null) {
          CurrentController.SetFocus(0);
        }
        _currentPlayer = value;
        CurrentController.SetFocus(1);
        _target.Visible = false;
      }
    }

    protected override void Awake() {
      base.Awake();
      _mainCamera = OverlayManager.Camera;
      _hud = FindObjectOfType<HUDView>();
      _target = Instantiate(_targetPrefab);
      _interactionHoverSound.Setup();
    }

    protected override void OnDestroy() {
      base.OnDestroy();
      _interactionHoverSound.Release();
    }

    public override void OnEnter() {
      base.OnEnter();
      _currentPlayer = PlayerType.None;
      _hud.SetActive(true);
      _hud.SetInteractive(true);
      Manager.CameraWeightTween.Settle();
    }

    public override void OnExit() {
      base.OnExit();
      _hud.SetActive(false);
      _hud.SetInteractive(false);
      if (_target != null) {
        _target.Visible = false;
      }
    }

    public override void OnUpdate() {
      _currentCommand = Command.None;
      var previousInteractable = _interactable;
      _interactable = null;

      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      var ray = _mainCamera.ScreenPointToRay(mousePosition);

      if (Physics.Raycast(ray, out var hit, 100, _config.GroundMask)) {
        _currentCommand = Command.Move;
        _targetPosition = hit.point.ToNavMesh();
      }

      if (!IsNavigating(CurrentController)
        && (TryHitInteractable(
            ray,
            _config.InteractionMask,
            out var interactable
          )
          || TryHitInteractable(ray, _config.PlayerMask, out interactable))) {
        _currentCommand = Command.Interact;
        _interactable = interactable;
      }

      if (_interactable != previousInteractable) {
        if (previousInteractable != null) {
          previousInteractable.OnHoverExit();
        }

        if (_interactable != null) {
          _interactionHoverSound.Play();
          _interactable.OnHoverEnter();
        }
      }

      if (Manager.LT.CommandAction.action.WasPerformedThisFrame()
        || Manager.RT.CommandAction.action.WasPerformedThisFrame()) {
        _areBothPressed = false;
        _lastBothTime = -_keyReleaseDelay;
      }

      if (Manager.LT.CommandAction.action.IsPressed()
        && Manager.RT.CommandAction.action.IsPressed()) {
        _areBothPressed = true;
        _lastBothTime = Time.unscaledTime;
      }

      if (Manager.LT.CommandAction.action.IsPressed()
        != Manager.RT.CommandAction.action.IsPressed()
        && Time.unscaledTime - _lastBothTime > _keyReleaseDelay) {
        _areBothPressed = false;
      }

      UpdatePlayer(Manager.LT);
      UpdatePlayer(Manager.RT);

      var currentController = CurrentController;
      _hud.SetInteractive(!IsNavigating(currentController));
      _target.DrivenUpdate(Manager, currentController, _areBothPressed);

      if (currentController?.NavigateState.IsActive ?? false) {
        Manager.FocusedPlayer =
          _areBothPressed ? PlayerType.Both : currentController.Type;
      }
    }

    private void UpdatePlayer(PlayerController player) {
      if (player.CommandAction.action.WasPerformedThisFrame()
        && _commandedPlayer == PlayerType.None
        && IsMouseWithinBounds()) {
        CurrentPlayer = player.Type;

        switch (_currentCommand) {
          case Command.Interact:
            _interactable.Interact(player);
            break;
          case Command.Move:
            _commandedPlayer = player.Type;
            player.NavigateState.Enter(_targetPosition);
            break;
        }
      }

      if (player.CommandAction.action.WasPerformedThisFrame()
        && _commandedPlayer == player.Other.Type
        && _currentCommand == Command.Move) {
        player.FollowState.Enter();
      }

      if (!player.CommandAction.action.IsPressed()
        && _commandedPlayer == player.Type) {
        if (_currentCommand == Command.Move
          && player.Other.CommandAction.action.IsPressed()) {
          if (Time.unscaledTime - _lastBothTime > _keyReleaseDelay) {
            _commandedPlayer = player.Other.Type;
            CurrentPlayer = player.Other.Type;
            player.Other.NavigateState.Enter(_targetPosition);
          }
        } else {
          _commandedPlayer = PlayerType.None;
        }
      }

      player.FollowState.TightDistance = _areBothPressed;
      player.DrivenUpdate();
      if (IsNavigating(player)) {
        player.NavigateState.TargetPosition = _targetPosition;
      }
    }

    private bool IsMouseWithinBounds() {
      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      return mousePosition.x >= 0
        && mousePosition.x <= Screen.width
        && mousePosition.y >= 0
        && mousePosition.y <= Screen.height;
    }

    private bool IsNavigating(PlayerController player) {
      return player != null
        && player.NavigateState.IsActive
        && _commandedPlayer == player.Type;
    }

    private bool TryHitInteractable(
      Ray ray,
      int mask,
      out Interactable interactable
    ) {
      interactable = default;
      return Physics.Raycast(ray, out var hit, 100, mask)
        && hit.transform.TryGetComponent(out interactable);
    }
  }
}
