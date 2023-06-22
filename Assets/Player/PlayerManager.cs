using System;
using Cinemachine;
using Interactions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Utils;

namespace Player {
  public class PlayerManager : MonoBehaviour {
    public enum Command {
      None,
      Move,
      Interact,
    }

    [NonSerialized] public Command CurrentCommand = Command.None;
    [NonSerialized] public Interactable Interactable;
    [NonSerialized] public Vector3 TargetPosition;
    private PlayerType _currentPlayer = 0;

    public PlayerType CurrentPlayer {
      get => _currentPlayer;
      set {
        _currentPlayer = value;
        _target.SetActive(value != 0);
        _target.GetComponent<MeshRenderer>().material = this[value].Material;
      }
    }

    [SerializeField] [Inject] private PlayerConfig _config;
    [SerializeField] private GameObject _target;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayerController _rtPrefab;
    [SerializeField] private PlayerController _ltPrefab;
    [SerializeField] private Vector3 _rtStartPosition;
    [SerializeField] private Vector3 _ltStartPosition;
    [SerializeField] private InputActionReference _targetAction;

    [NonSerialized] public PlayerController RT;
    [NonSerialized] public PlayerController LT;

    private void Awake() {
      RT = Instantiate(_rtPrefab, _rtStartPosition, Quaternion.identity);
      LT = Instantiate(_ltPrefab, _ltStartPosition, Quaternion.identity);
      RT.Other = LT;
      LT.Other = RT;
      RT.Manager = this;
      LT.Manager = this;

      var group = GetComponent<CinemachineTargetGroup>();
      group.m_Targets[0].target = RT.transform;
      group.m_Targets[1].target = LT.transform;
    }

    private void Update() {
      CurrentCommand = Command.None;
      var previousInteractable = Interactable;
      Interactable = null;

      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      var ray = _camera.ScreenPointToRay(mousePosition);
      var currentController = CurrentController;
      var mask = _config.InteractionMask;
      if (currentController?.IsActivelyNavigating() ?? false) {
        mask = 1 << _config.GroundLayer;
      }

      if (!Physics.Raycast(ray, out var hit, 100, mask)) {
        return;
      }

      TargetPosition = hit.point;
      if (hit.transform.TryGetComponent<Interactable>(out var interactable)
        && interactable.CanInteract()) {
        CurrentCommand = Command.Interact;
        Interactable = interactable;
      } else {
        CurrentCommand = Command.Move;
        if (NavMesh.SamplePosition(
            hit.point,
            out var point,
            100,
            NavMesh.AllAreas
          )) {
          TargetPosition = point.position;
        }
      }

      if (Interactable != previousInteractable) {
        if (previousInteractable != null) {
          previousInteractable.OnHoverExit();
        }

        if (Interactable != null) {
          Interactable.OnHoverEnter();
        }
      }

      LT.DrivenUpdate();
      RT.DrivenUpdate();

      if (currentController?.NavigateState.IsActive ?? false) {
        _target.transform.position = currentController.TargetPosition;
        _target.SetActive(true);
      } else {
        _target.SetActive(false);
      }
    }

    public PlayerController CurrentController => this[_currentPlayer];

    public PlayerController this[PlayerType type] =>
      type switch {
        PlayerType.RT => RT,
        PlayerType.LT => LT,
        _ => null,
      };
  }
}
