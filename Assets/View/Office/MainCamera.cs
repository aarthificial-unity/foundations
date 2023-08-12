using UnityEngine;
using UnityEngine.InputSystem;

namespace View.Office {
  public class MainCamera : MonoBehaviour {
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private Vector2 _range;
    private Vector3 _initialRotation;

    private void Awake() {
      _initialRotation = transform.eulerAngles;
    }

    private void Update() {
      var mousePosition = _lookAction.action.ReadValue<Vector2>();
      var viewportPoint = Camera.main.ScreenToViewportPoint(mousePosition);

      transform.rotation = Quaternion.Euler(
        _initialRotation.x + _range.x * (viewportPoint.y - 0.5f),
        _initialRotation.y + _range.y * (viewportPoint.x - 0.5f),
        _initialRotation.z
      );
    }
  }
}
