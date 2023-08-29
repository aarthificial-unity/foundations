using UnityEngine;

namespace Environment {
  [DefaultExecutionOrder(300)]
  public class UICamera : MonoBehaviour {
    private Camera _mainCamera;
    private Camera _camera;

    private void Awake() {
      _mainCamera = transform.parent.GetComponent<Camera>();
      _camera = GetComponent<Camera>();
    }

    private void LateUpdate() {
      _camera.orthographicSize = _mainCamera.orthographicSize;
    }
  }
}
