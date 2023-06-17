using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Utils;

namespace Player {
  public class PlayerController : MonoBehaviour {
    private enum PlayerState {
      Following,
      Interacting,
      Navigating,
      Idle,
    }

    public Material Material;

    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _target;
    [SerializeField] private PlayerController _other;
    [SerializeField] private PlayerConfig _config;
    [SerializeField] private InputActionReference _targetAction;
    [SerializeField] private InputActionReference _commandAction;

    private NavMeshAgent _agent;
    private PlayerState _state = PlayerState.Idle;

    private bool _isFollowing;
    private bool _isCommanding;
    private Vector2 _lastCommandPosition;
    private float _lastCommandTime;
    private NavMeshPath _path;
    private Vector3 _corner;
    private Interactable _interactable;

    private void Awake() {
      _path = new NavMeshPath();
      _agent = GetComponent<NavMeshAgent>();
      _target.GetComponent<MeshRenderer>().material = Material;
      _target.SetActive(false);
    }

    private void OnEnable() {
      _agent.acceleration = _config.Acceleration;
      _commandAction.action.performed += HandleCommand;
    }

    private void OnDisable() {
      _commandAction.action.performed -= HandleCommand;
    }

    private void Update() {
      switch (_state) {
        case PlayerState.Following:
          UpdateFollowing();
          break;
        case PlayerState.Navigating:
          UpdateNavigating();
          break;
        case PlayerState.Interacting:
          UpdateInteracting();
          break;
        case PlayerState.Idle:
          // UpdateIdle();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void UpdateFollowing() {
      _agent.autoBraking = false;
      _agent.destination = _other.transform.position;

      // Slowly stop walking if the other lead is walking towards us.
      var direction =
        (transform.position - _other.transform.position).normalized;
      var targetDirection =
        (transform.position - _other._agent.destination).normalized;
      if (Vector3.Dot(_other._agent.desiredVelocity.normalized, direction)
        > 0.5f
        // TODO Consider going towards the target if it's between us and the other lead.
        && Vector3.Dot(targetDirection, direction) < 0.5f) {
        _agent.speed = 0;
        _agent.acceleration = 8f;
        return;
      }

      _agent.speed = _agent.remainingDistance.ClampRemap(
        _config.MinDistance,
        _config.MaxDistance,
        0f,
        _other._agent.speed
      );
      _agent.acceleration = _agent.remainingDistance.ClampRemap(
        _config.MinDistance,
        _config.MaxDistance,
        8f,
        _other._agent.acceleration
      );
    }

    private void UpdateNavigating() {
      if (_isCommanding
        && _commandAction.action.ReadValue<float>() > 0.5f
        && Raycast(out var hit)) {
        _agent.destination = hit.point;
      }

      if (_other._state == PlayerState.Interacting) {
        _other._agent.CalculatePath(_agent.destination, _path);
        var corners = _path.corners;
        var fullLength = 0f;
        for (var i = 1; i < corners.Length; i++) {
          var length = Vector3.Distance(corners[i - 1], corners[i]);
          fullLength += length;
          if (fullLength > _config.LimitDistance) {
            var difference = fullLength - _config.LimitDistance;
            var position = Vector3.Lerp(
              corners[i],
              corners[i - 1],
              difference / length
            );
            _agent.destination = position;
            break;
          }
        }
      }

      _target.transform.position = _agent.pathEndPosition;
      _target.SetActive(
        _agent.remainingDistance > 0.01f
        || _commandAction.action.ReadValue<float>() > 0.5f
      );
    }

    private void UpdateInteracting() {
      _agent.destination = _interactable.GetPosition();
      _interactable.SetReached(
        this,
        _agent.remainingDistance < _interactable.GetRadius()
      );
    }

    private void Follow() {
      if (_state == PlayerState.Interacting) {
        return;
      }

      _target.SetActive(false);
      _state = PlayerState.Following;
      _agent.stoppingDistance = _config.MinDistance;
    }

    private void Navigate() {
      TakeControl();
      _target.SetActive(true);
      _state = PlayerState.Navigating;
      _agent.destination = _lastCommandPosition;
    }

    private void Interact(Interactable interactable) {
      if (_state == PlayerState.Interacting && _interactable == interactable) {
        return;
      }

      TakeControl();
      _target.SetActive(false);
      _state = PlayerState.Interacting;
      _interactable = interactable;
      _interactable.StartInteraction(this);
      _agent.destination = interactable.GetPosition();
    }

    private void TakeControl() {
      if (_state == PlayerState.Interacting) {
        _interactable.StopInteraction(this);
        _interactable = null;
      }

      _other.Follow();
      _agent.stoppingDistance = 0;
      _agent.acceleration = _config.Acceleration;
      _agent.speed = _config.WalkSpeed;
      _agent.autoBraking = true;
    }

    private void PerformSpecialAction() {
      if (_state == PlayerState.Navigating) {
        _agent.speed = _config.SprintSpeed;
      }
    }

    private void HandleCommand(InputAction.CallbackContext context) {
      if (_isCommanding
        && Time.time - _lastCommandTime < _config.DoubleTapTime
        && _lastCommandPosition == _targetAction.action.ReadValue<Vector2>()) {
        PerformSpecialAction();
        return;
      }

      _lastCommandPosition = _targetAction.action.ReadValue<Vector2>();
      _lastCommandTime = Time.time;
      _isCommanding = true;

      if (Raycast(out var hit)) {
        if (hit.transform.gameObject.layer == _config.GroundLayer) {
          Navigate();
        } else if (hit.transform.TryGetComponent<Interactable>(
            out var interactable
          )) {
          Interact(interactable);
        }
      }
    }

    private bool Raycast(out RaycastHit hit) {
      var mousePosition = _targetAction.action.ReadValue<Vector2>();
      var ray = _camera.ScreenPointToRay(mousePosition);
      return Physics.Raycast(ray, out hit, 100, _config.InteractionMask);
    }
  }
}
