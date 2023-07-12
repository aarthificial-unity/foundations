﻿using Interactions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;
using View;

namespace Player.ManagerStates {
  public class ExploreState : ManagerState {
    private enum Command {
      None,
      Move,
      Interact,
      Talk,
    }

    [Inject] [SerializeField] private PlayerChannel _players;
    [Inject] [SerializeField] private ViewChannel _view;
    [Inject] [SerializeField] private PlayerConfig _config;
    [SerializeField] private GameObject _target;
    [SerializeField] private InputActionReference _targetAction;

    private Command _currentCommand = Command.None;
    private Interactable _interactable;
    private Vector3 _targetPosition;
    private PlayerType _currentPlayer = PlayerType.None;
    private PlayerType _commandedPlayer = PlayerType.None;
    private PlayerType _talkedPlayer = PlayerType.None;

    private PlayerController CurrentController => _players[_currentPlayer];

    public PlayerType CurrentPlayer {
      get => _currentPlayer;
      set {
        _currentPlayer = value;
        _target.SetActive(value != PlayerType.None);
        _target.GetComponent<MeshRenderer>().material =
          _players[value]?.Material;
      }
    }

    public override void OnEnter() {
      _view.HUD.SetActive(true);
      _view.HUD.SetInteractive(true);
    }

    public override void OnExit() {
      _view.HUD.SetActive(false);
      _view.HUD.SetInteractive(false);
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
      _view.HUD.SetInteractive(!IsNavigating(currentController));

      if (currentController?.NavigateState.IsActive ?? false) {
        _target.transform.position = currentController.TargetPosition;
        _target.SetActive(true);
      } else {
        _target.SetActive(false);
      }
    }

    private void UpdatePlayer(PlayerController player) {
      if (player.CommandAction.action.WasPerformedThisFrame()
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

    private bool IsNavigating(PlayerController player) {
      return player != null
        && player.NavigateState.IsActive
        && _commandedPlayer == player.Type;
    }
  }
}