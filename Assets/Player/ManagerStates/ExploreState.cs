using Interactions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;
using View;
using View.Overlay;

namespace Player.ManagerStates {
  public class ExploreState : ManagerState {
    private enum Command {
      None,
      Move,
      Interact,
    }

    [Inject] [SerializeField] private PlayerChannel _players;
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [Inject] [SerializeField] private PlayerConfig _config;
    [Inject] [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private InputActionReference _targetAction;

    private Command _currentCommand = Command.None;
    private Interactable _interactable;
    private Vector3 _targetPosition;
    private PlayerType _currentPlayer = PlayerType.None;
    private PlayerType _commandedPlayer = PlayerType.None;
    private GameObject _target;

    private PlayerController CurrentController => _players[_currentPlayer];

    public PlayerType CurrentPlayer {
      get => _currentPlayer;
      set {
        if (CurrentController != null) {
          CurrentController.SetFocus(0);
        }
        _currentPlayer = value;
        CurrentController.SetFocus(1);
        _target.SetActive(value != PlayerType.None);
        _target.GetComponent<MeshRenderer>().material =
          _players[value]?.Material;
      }
    }

    protected override void Awake() {
      base.Awake();
      _target = Instantiate(_targetPrefab);
    }

    public override void OnEnter() {
      _overlay.HUD.SetActive(true);
      _overlay.HUD.SetInteractive(true);
    }

    public override void OnExit() {
      _overlay.HUD.SetActive(false);
      _overlay.HUD.SetInteractive(false);
      _target.SetActive(false);
    }

    public override void OnUpdate() {
      _currentCommand = Command.None;
      var previousInteractable = _interactable;
      _interactable = null;

      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      var ray = Camera.main.ScreenPointToRay(mousePosition);

      if (Physics.Raycast(ray, out var hit, 100, _config.GroundMask)) {
        _currentCommand = Command.Move;
        _targetPosition = hit.point.ToNavMesh();
      }

      if (!EventSystem.current.IsPointerOverGameObject()
        && !IsNavigating(CurrentController)
        && Physics.Raycast(ray, out hit, 100, _config.InteractionMask)
        && hit.transform.TryGetComponent<Interactable>(out var interactable)) {
        _currentCommand = Command.Interact;
        _interactable = interactable;
      }

      if (_interactable != previousInteractable) {
        if (previousInteractable != null) {
          previousInteractable.OnHoverExit();
        }

        if (_interactable != null) {
          _interactable.OnHoverEnter();
        }
      }

      UpdatePlayer(_players.LT);
      UpdatePlayer(_players.RT);

      var currentController = CurrentController;
      _overlay.HUD.SetInteractive(!IsNavigating(currentController));

      if (currentController?.NavigateState.IsActive ?? false) {
        _target.transform.position = currentController.TargetPosition;
        _target.SetActive(true);
      } else {
        _target.SetActive(false);
      }
    }

    private void UpdatePlayer(PlayerController player) {
      if (player.CommandAction.action.WasPerformedThisFrame()
        && IsMouseWithinBounds()
        && !EventSystem.current.IsPointerOverGameObject()) {
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

      if (player.CommandAction.action.WasReleasedThisFrame()
        && _commandedPlayer == player.Type) {
        _commandedPlayer = PlayerType.None;
      }

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
  }
}
