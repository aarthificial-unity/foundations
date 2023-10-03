using UnityEngine;
using UnityEngine.InputSystem;

namespace View.Office {
  public class MainCamera : MonoBehaviour {
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private Vector2 _range;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _mainCamera;

    private void Update() {
      var mousePosition = _lookAction.action.ReadValue<Vector2>();
      var viewportPoint = _mainCamera.ScreenToViewportPoint(mousePosition);
      _target.localPosition = new Vector3(
        _offset.x + _range.x * (viewportPoint.x - 0.5f) * _mainCamera.aspect,
        _offset.y + _range.y * (viewportPoint.y - 0.5f),
        _target.localPosition.z
      );
    }
  }
}
