using UnityEngine;
using UnityEngine.InputSystem;

namespace View.Office {
  public class MainCamera : MonoBehaviour {
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private Vector2 _range;
    [SerializeField] private Transform _target;

    private void Update() {
      var mousePosition = _lookAction.action.ReadValue<Vector2>();
      var viewportPoint = Camera.main.ScreenToViewportPoint(mousePosition);
      _target.localPosition = new Vector3(
        _range.x * (viewportPoint.x - 0.5f) * Camera.main.aspect,
        _range.y * (viewportPoint.y - 0.5f),
        _target.localPosition.z
      );
    }
  }
}
