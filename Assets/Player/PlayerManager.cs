using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Player {
  public class PlayerManager : MonoBehaviour {
    [SerializeField] private Camera _camera;
    [SerializeField] private InputActionReference _targetAction;
    [SerializeField] private InputActionReference _commandAction;
    [SerializeField] private float _acceleration = 30;
    [SerializeField] private float _minDistance = 4;
    [SerializeField] private float _maxDistance = 5;

    [SerializeField] private PlayerController _lt;
    [SerializeField] private PlayerController _rt;
    [SerializeField] private GameObject _target;
    [SerializeField] private Material _ltMaterial;
    [SerializeField] private Material _rtMaterial;

    private Vector3? _commandPosition;
    private bool _isFollowing;
    private bool _isCommandingLt;

    private void Awake() {
      _lt.Agent.acceleration = _acceleration;
      _rt.Agent.acceleration = _acceleration;
      _target.SetActive(false);
    }

    private void Update() {
      var commanding = _commandAction.action.ReadValue<float>();
      if (commanding != 0) {
        FindCommandPosition();
        _isCommandingLt = commanding > 0;
        _target.GetComponent<MeshRenderer>().material =
          _isCommandingLt ? _ltMaterial : _rtMaterial;
      }

      if (_commandPosition.HasValue) {
        _target.SetActive(true);
        var command = _isCommandingLt ? _lt : _rt;
        var follow = _isCommandingLt ? _rt : _lt;
        command.Agent.destination = _commandPosition.Value;
        Debug.DrawRay(_commandPosition.Value, Vector3.up, Color.red);
        command.Agent.stoppingDistance = 0;
        command.Agent.acceleration = _acceleration;
        follow.Agent.acceleration = _acceleration / 2;
        follow.Agent.destination = command.transform.position;
        if (follow.Agent.remainingDistance < _minDistance) {
          _isFollowing = false;
        }

        if (follow.Agent.remainingDistance > _maxDistance) {
          _isFollowing = true;
        }

        if (_isFollowing) {
          follow.Agent.stoppingDistance = _minDistance;
        } else {
          follow.Agent.stoppingDistance = _maxDistance;
        }

        if (!command.Agent.pathPending
          && command.Agent.remainingDistance < 0.001) {
          _target.SetActive(false);
        }
      } else {
        _lt.Agent.destination = _lt.transform.position;
        _rt.Agent.destination = _rt.transform.position;
      }
    }

    private void FindCommandPosition() {
      var ray =
        _camera.ScreenPointToRay(_targetAction.action.ReadValue<Vector2>());
      if (Physics.Raycast(
          ray,
          out var hit,
          100,
          ~LayerMask.NameToLayer("Ground")
        )
        && NavMesh.SamplePosition(
          hit.point,
          out var navHit,
          5,
          NavMesh.AllAreas
        )) {
        _commandPosition = navHit.position;
        _target.SetActive(true);
        _target.transform.position = _commandPosition.Value;
      } else {
        _commandPosition = null;
        _target.SetActive(false);
      }
    }
  }
}
