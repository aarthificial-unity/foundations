using UnityEngine;
using View.Overlay;

namespace Environment {
  [DefaultExecutionOrder(1000)]
  public class LookAtCamera : MonoBehaviour {
    private Camera _mainCamera;

    private void Awake() {
      _mainCamera = OverlayManager.Camera;
    }

    private void LateUpdate() {
      transform.rotation = _mainCamera.transform.rotation;
    }
  }
}
