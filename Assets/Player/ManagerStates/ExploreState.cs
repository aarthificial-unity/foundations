﻿using Interactions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;
using Utils.Tweening;
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
    [Inject] [SerializeField] private TargetController _targetPrefab;
    [SerializeField] private InputActionReference _targetAction;

    private Command _currentCommand = Command.None;
    private Interactable _interactable;
    private Vector3 _targetPosition;
    private PlayerType _currentPlayer = PlayerType.None;
    private PlayerType _commandedPlayer = PlayerType.None;
    private TargetController _target;
    private Dynamics _dynamics;

    private PlayerController CurrentController => _players[_currentPlayer];

    public PlayerType CurrentPlayer {
      get => _currentPlayer;
      set {
        if (CurrentController != null) {
          CurrentController.SetFocus(0);
        }
        _currentPlayer = value;
        CurrentController.SetFocus(1);
        _target.Visible = false;
        _target.Material = _players[value]?.Material;
      }
    }

    protected override void Awake() {
      base.Awake();
      _target = Instantiate(_targetPrefab);
    }

    public override void OnEnter() {
      base.OnEnter();
      _currentPlayer = PlayerType.None;
      _overlay.HUD.SetActive(true);
      _overlay.HUD.SetInteractive(true);
      Manager.CameraWeight.Settle();
    }

    public override void OnExit() {
      base.OnExit();
      _overlay.HUD.SetActive(false);
      _overlay.HUD.SetInteractive(false);
      _target.Visible = false;
    }

    public override void OnUpdate() {
      _currentCommand = Command.None;
      var previousInteractable = _interactable;
      _interactable = null;

      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      var ray =
        _overlay.CameraManager.MainCamera.ScreenPointToRay(mousePosition);

      if (Physics.Raycast(ray, out var hit, 100, _config.GroundMask)) {
        _currentCommand = Command.Move;
        _targetPosition = hit.point.ToNavMesh();
      }

      int interactionMask = _config.InteractionMask;
      if (!_players.LT.InteractState.IsActive
        && !_players.RT.InteractState.IsActive) {
        interactionMask |= _config.PlayerMask;
      }
      if (!EventSystem.current.IsPointerOverGameObject()
        && !IsNavigating(CurrentController)
        && Physics.Raycast(ray, out hit, 100, interactionMask)
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
        Manager.CameraWeight.Set(currentController.IsLT ? 0.2f : 0.8f);
        _target.transform.position = currentController.TargetPosition;
        if (currentController.CommandAction.action.IsPressed()) {
          _dynamics.ForceSet(0.5f);
        } else {
          _dynamics.Set(1f);
        }
        _target.Scale = _dynamics.Update(in SpringConfig.Medium).x;
        _target.Visible = true;
      } else {
        _target.Visible = false;
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
