using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {
  public class PlayerController : MonoBehaviour {
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _speed = 1;

    private Rigidbody _rigidbody;
    private Vector2 _moveDirection;

    private void Awake() {
      _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable() {
      _moveAction.action.started += HandleMove;
      _moveAction.action.performed += HandleMove;
      _moveAction.action.canceled += HandleMove;
    }

    private void OnDisable() {
      _moveAction.action.started -= HandleMove;
      _moveAction.action.performed -= HandleMove;
      _moveAction.action.canceled -= HandleMove;
    }

    private void FixedUpdate() {
      var currentVelocity = _rigidbody.velocity;
      var movementVelocity =
        new Vector3(_moveDirection.x, 0, _moveDirection.y) * _speed;
      var change = (movementVelocity - currentVelocity) / 2;
      change.y = 0;
      _rigidbody.AddForce(change, ForceMode.Impulse);

      if (transform.position.y < -3) {
        transform.position = Vector3.up;
      }
    }

    private void HandleMove(InputAction.CallbackContext context) {
      _moveDirection = context.ReadValue<Vector2>();
    }
  }
}
